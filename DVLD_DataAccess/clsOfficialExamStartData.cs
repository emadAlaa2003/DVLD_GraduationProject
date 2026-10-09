using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DVLD_DataAccess
{
    // Only safe-to-display fields belong in this response. Never include
    // CorrectOption, Explanation or IsCorrect in the candidate's exam payload.
    public sealed class OfficialExamCandidateQuestion
    {
        public long ExamQuestionID { get; set; }
        public short QuestionNumber { get; set; }
        public string QuestionType { get; set; }
        public string QuestionText { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
    }

    public sealed class OfficialExamStartResult
    {
        public long ExamAttemptID { get; set; }
        public int TestAppointmentID { get; set; }
        public DateTime StartedAtUtc { get; set; }
        public DateTime DeadlineAtUtc { get; set; }
        public List<OfficialExamCandidateQuestion> Questions { get; set; }
            = new List<OfficialExamCandidateQuestion>();
    }

    // This class is called ONLY by the Business layer after a trusted employee
    // confirms the candidate's identity. NationalNo is a lookup key, NOT a login.
    public static class clsOfficialExamStartData
    {
        public static OfficialExamStartResult StartExam(
            int testAppointmentID,
            string nationalNo,
            int questionCount,
            int requiredCorrectAnswers,
            int durationSeconds,
            int startEarlyMinutes,
            int startLateMinutes)
        {
            if (testAppointmentID <= 0)
                throw new ArgumentOutOfRangeException(nameof(testAppointmentID));
            if (string.IsNullOrWhiteSpace(nationalNo))
                throw new ArgumentException("National number is required.", nameof(nationalNo));
            if (questionCount < 1 || questionCount > 200)
                throw new ArgumentOutOfRangeException(nameof(questionCount));
            if (requiredCorrectAnswers < 1 || requiredCorrectAnswers > questionCount)
                throw new ArgumentOutOfRangeException(nameof(requiredCorrectAnswers));
            if (durationSeconds <= 0 || durationSeconds > 86400)
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            if (startEarlyMinutes < 0 || startEarlyMinutes > 1440)
                throw new ArgumentOutOfRangeException(nameof(startEarlyMinutes));
            if (startLateMinutes < 0 || startLateMinutes > 1440)
                throw new ArgumentOutOfRangeException(nameof(startLateMinutes));

            using (SqlConnection connection = new SqlConnection(
                clsDataAccessSettings.ConnectionString))
            {
                connection.Open();

                // Lock the booked appointment for the whole operation: concurrent
                // Start requests for the same appointment cannot create two exams.
                using (SqlTransaction transaction = connection.BeginTransaction(
                    IsolationLevel.Serializable))
                {
                    try
                    {
                        const string eligibilitySql = @"
SELECT TOP (1)
    A.ApplicantPersonID AS PersonID,
    TA.TestTypeID,
    TA.AppointmentDate,
    A.ApplicationStatus,
    TA.IsLocked,
    SYSDATETIME() AS ServerLocalNow,
    CAST(CASE WHEN EXISTS
        (SELECT 1 FROM dbo.Tests T
         WHERE T.TestAppointmentID = TA.TestAppointmentID)
         THEN 1 ELSE 0 END AS bit) AS HasRecordedTest,
    CAST(CASE WHEN EXISTS
        (SELECT 1 FROM dbo.OfficialExamAttempts EA
         WHERE EA.TestAppointmentID = TA.TestAppointmentID)
         THEN 1 ELSE 0 END AS bit) AS HasOfficialAttempt,
    CAST(CASE WHEN EXISTS
        (SELECT 1 FROM dbo.TestAppointments VA
         INNER JOIN dbo.Tests VT
             ON VT.TestAppointmentID = VA.TestAppointmentID
         WHERE VA.LocalDrivingLicenseApplicationID =
               TA.LocalDrivingLicenseApplicationID
           AND VA.TestTypeID = 1 AND VT.TestResult = 1)
         THEN 1 ELSE 0 END AS bit) AS HasPassedVisionTest
FROM dbo.TestAppointments TA WITH (UPDLOCK, HOLDLOCK)
INNER JOIN dbo.LocalDrivingLicenseApplications LDLA
    ON LDLA.LocalDrivingLicenseApplicationID =
       TA.LocalDrivingLicenseApplicationID
INNER JOIN dbo.Applications A
    ON A.ApplicationID = LDLA.ApplicationID
INNER JOIN dbo.People P
    ON P.PersonID = A.ApplicantPersonID
WHERE TA.TestAppointmentID = @AppointmentID
  AND P.NationalNo = @NationalNo;";

                        int personID;
                        using (SqlCommand cmd = new SqlCommand(
                            eligibilitySql, connection, transaction))
                        {
                            cmd.Parameters.Add("@AppointmentID", SqlDbType.Int)
                                .Value = testAppointmentID;
                            cmd.Parameters.Add("@NationalNo", SqlDbType.NVarChar, 30)
                                .Value = nationalNo.Trim();

                            using (SqlDataReader reader = cmd.ExecuteReader(
                                CommandBehavior.SingleRow))
                            {
                                if (!reader.Read())
                                    throw new InvalidOperationException(
                                        "Matching appointment not found.");

                                personID = Convert.ToInt32(reader["PersonID"]);
                                int testTypeID = Convert.ToInt32(reader["TestTypeID"]);
                                int applicationStatus = Convert.ToInt32(
                                    reader["ApplicationStatus"]);
                                DateTime appointmentDate = Convert.ToDateTime(
                                    reader["AppointmentDate"]);
                                DateTime serverLocalNow = Convert.ToDateTime(
                                    reader["ServerLocalNow"]);
                                bool locked = Convert.ToBoolean(reader["IsLocked"]);
                                bool alreadyTested = Convert.ToBoolean(
                                    reader["HasRecordedTest"]);
                                bool alreadyStarted = Convert.ToBoolean(
                                    reader["HasOfficialAttempt"]);
                                bool passedVision = Convert.ToBoolean(
                                    reader["HasPassedVisionTest"]);

                                if (testTypeID != 2 || applicationStatus != 1 ||
                                    locked || alreadyTested || alreadyStarted ||
                                    !passedVision)
                                {
                                    throw new InvalidOperationException(
                                        "This appointment is not eligible for a written exam.");
                                }

                                // Existing AppointmentDate is local SQL Server time.
                                // Configure the SQL Server time zone to match the
                                // examination centre before real deployment.
                                if (serverLocalNow < appointmentDate.AddMinutes(
                                        -startEarlyMinutes) ||
                                    serverLocalNow > appointmentDate.AddMinutes(
                                        startLateMinutes))
                                {
                                    throw new InvalidOperationException(
                                        "The appointment is outside its permitted start window.");
                                }
                            }
                        }

                        // Freeze the scoring policy and deadline at exam start.
                        const string createAttemptSql = @"
DECLARE @StartedAtUtc datetime2(0) = SYSUTCDATETIME();
INSERT dbo.OfficialExamAttempts
    (TestAppointmentID, PersonID, Status, QuestionCount,
     RequiredCorrectAnswers, DurationSeconds, StartedAtUtc, DeadlineAtUtc)
OUTPUT INSERTED.ExamAttemptID,
       INSERTED.StartedAtUtc,
       INSERTED.DeadlineAtUtc
VALUES
    (@AppointmentID, @PersonID, N'InProgress', @QuestionCount,
     @RequiredCorrectAnswers, @DurationSeconds, @StartedAtUtc,
     DATEADD(second, @DurationSeconds, @StartedAtUtc));";

                        OfficialExamStartResult result = new OfficialExamStartResult
                        {
                            TestAppointmentID = testAppointmentID
                        };

                        using (SqlCommand cmd = new SqlCommand(
                            createAttemptSql, connection, transaction))
                        {
                            cmd.Parameters.Add("@AppointmentID", SqlDbType.Int)
                                .Value = testAppointmentID;
                            cmd.Parameters.Add("@PersonID", SqlDbType.Int)
                                .Value = personID;
                            cmd.Parameters.Add("@QuestionCount", SqlDbType.SmallInt)
                                .Value = (short)questionCount;
                            cmd.Parameters.Add("@RequiredCorrectAnswers", SqlDbType.SmallInt)
                                .Value = (short)requiredCorrectAnswers;
                            cmd.Parameters.Add("@DurationSeconds", SqlDbType.Int)
                                .Value = durationSeconds;

                            using (SqlDataReader reader = cmd.ExecuteReader(
                                CommandBehavior.SingleRow))
                            {
                                if (!reader.Read())
                                    throw new InvalidOperationException(
                                        "Could not create the exam attempt.");

                                result.ExamAttemptID = Convert.ToInt64(
                                    reader["ExamAttemptID"]);
                                result.StartedAtUtc = Convert.ToDateTime(
                                    reader["StartedAtUtc"]);
                                result.DeadlineAtUtc = Convert.ToDateTime(
                                    reader["DeadlineAtUtc"]);
                            }
                        }

                        // Choose active, human-approved questions only. Save a
                        // snapshot, including correct answers, in SQL; never send
                        // those answer fields to the examination client.
                        const string selectAndFreezeSql = @"
INSERT dbo.OfficialExamQuestions
    (ExamAttemptID, QuestionID, QuestionNumber, QuestionType,
     QuestionText, OptionA, OptionB, OptionC, OptionD,
     CorrectOption, Explanation, SourceDocumentID, SourcePageNumber)
SELECT
    @AttemptID, Picked.QuestionID,
    ROW_NUMBER() OVER (ORDER BY Picked.QuestionID),
    Picked.QuestionType, Picked.QuestionText,
    Picked.OptionA, Picked.OptionB, Picked.OptionC, Picked.OptionD,
    Picked.CorrectOption, Picked.Explanation,
    Picked.SourceDocumentID, Picked.SourcePageNumber
FROM
(
    SELECT TOP (@QuestionCount)
        Q.QuestionID, Q.QuestionType, Q.QuestionText,
        Q.OptionA, Q.OptionB, Q.OptionC, Q.OptionD,
        Q.CorrectOption, Q.Explanation,
        Q.SourceDocumentID, Q.SourcePageNumber
    FROM dbo.QuestionBank Q
    WHERE Q.ReviewStatus = N'Approved'
      AND ISNULL(Q.IsActive, 1) = 1
      AND NULLIF(LTRIM(RTRIM(Q.QuestionText)), N'') IS NOT NULL
      AND NULLIF(LTRIM(RTRIM(Q.OptionA)), N'') IS NOT NULL
      AND NULLIF(LTRIM(RTRIM(Q.OptionB)), N'') IS NOT NULL
      AND
      (
          (Q.QuestionType = N'TrueFalse'
           AND Q.CorrectOption IN ('A', 'B'))
          OR
          (Q.QuestionType = N'MultipleChoice'
           AND NULLIF(LTRIM(RTRIM(Q.OptionC)), N'') IS NOT NULL
           AND NULLIF(LTRIM(RTRIM(Q.OptionD)), N'') IS NOT NULL
           AND Q.CorrectOption IN ('A', 'B', 'C', 'D'))
      )
    ORDER BY NEWID()
) AS Picked;";

                        using (SqlCommand cmd = new SqlCommand(
                            selectAndFreezeSql, connection, transaction))
                        {
                            cmd.Parameters.Add("@AttemptID", SqlDbType.BigInt)
                                .Value = result.ExamAttemptID;
                            cmd.Parameters.Add("@QuestionCount", SqlDbType.Int)
                                .Value = questionCount;

                            int inserted = cmd.ExecuteNonQuery();
                            if (inserted != questionCount)
                            {
                                // Roll back the attempt too: never start an
                                // incomplete exam when the bank has too few
                                // approved questions.
                                throw new InvalidOperationException(
                                    "Not enough approved questions to start this exam.");
                            }
                        }

                        const string safeQuestionsSql = @"
SELECT ExamQuestionID, QuestionNumber, QuestionType, QuestionText,
       OptionA, OptionB, OptionC, OptionD
FROM dbo.OfficialExamQuestions
WHERE ExamAttemptID = @AttemptID
ORDER BY QuestionNumber;";

                        using (SqlCommand cmd = new SqlCommand(
                            safeQuestionsSql, connection, transaction))
                        {
                            cmd.Parameters.Add("@AttemptID", SqlDbType.BigInt)
                                .Value = result.ExamAttemptID;

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    result.Questions.Add(new OfficialExamCandidateQuestion
                                    {
                                        ExamQuestionID = Convert.ToInt64(
                                            reader["ExamQuestionID"]),
                                        QuestionNumber = Convert.ToInt16(
                                            reader["QuestionNumber"]),
                                        QuestionType = reader["QuestionType"].ToString(),
                                        QuestionText = reader["QuestionText"].ToString(),
                                        OptionA = reader["OptionA"].ToString(),
                                        OptionB = reader["OptionB"].ToString(),
                                        OptionC = reader["OptionC"] == DBNull.Value
                                            ? null : reader["OptionC"].ToString(),
                                        OptionD = reader["OptionD"] == DBNull.Value
                                            ? null : reader["OptionD"].ToString()
                                    });
                                }
                            }
                        }

                        transaction.Commit();
                        return result;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
