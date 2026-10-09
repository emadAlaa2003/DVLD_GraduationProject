using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DVLD_DataAccess
{
    public sealed class OfficialExamFinalResult
    {
        public long ExamAttemptID { get; set; }
        public int TestAppointmentID { get; set; }
        public int TestID { get; set; }
        public int QuestionCount { get; set; }
        public int RequiredCorrectAnswers { get; set; }
        public int CorrectAnswers { get; set; }
        public bool Passed { get; set; }
        public string Status { get; set; }
        public DateTime FinishedAtUtc { get; set; }
    }

    // Only the Business layer calls this class. API authorization is separate.
    public static class clsOfficialExamSessionData
    {
        // Saves one answer immediately. A later Submit uses the answers already
        // stored in the frozen OfficialExamQuestions snapshot.
        public static void SaveAnswer(long examAttemptID, long examQuestionID,
                                      string selectedOption)
        {
            if (examAttemptID <= 0 || examQuestionID <= 0)
                throw new ArgumentOutOfRangeException("Exam identifiers must be positive.");

            string answer = string.IsNullOrWhiteSpace(selectedOption)
                ? null : selectedOption.Trim().ToUpperInvariant();
            if (answer != null && answer != "A" && answer != "B" &&
                answer != "C" && answer != "D")
                throw new ArgumentException("Answer must be A, B, C, D or null.",
                                            nameof(selectedOption));

            using (var connection = new SqlConnection(
                clsDataAccessSettings.ConnectionString))
            {
                connection.Open();
                using (var tx = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        const string stateSql = @"
SELECT Status, DeadlineAtUtc, SYSUTCDATETIME() AS ServerNowUtc
FROM dbo.OfficialExamAttempts WITH (UPDLOCK, HOLDLOCK)
WHERE ExamAttemptID = @AttemptID;";
                        using (var command = new SqlCommand(stateSql, connection, tx))
                        {
                            command.Parameters.Add("@AttemptID", SqlDbType.BigInt).Value = examAttemptID;
                            using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                            {
                                if (!reader.Read())
                                    throw new InvalidOperationException("Exam attempt was not found.");
                                if (!string.Equals(reader["Status"].ToString(), "InProgress",
                                                   StringComparison.Ordinal))
                                    throw new InvalidOperationException("The exam is already closed.");
                                if (Convert.ToDateTime(reader["ServerNowUtc"]) >=
                                    Convert.ToDateTime(reader["DeadlineAtUtc"]))
                                    throw new InvalidOperationException("The exam time has expired.");
                            }
                        }

                        // The chosen option must exist on this frozen question.
                        const string saveSql = @"
UPDATE dbo.OfficialExamQuestions
SET SelectedOption = @SelectedOption,
    AnsweredAtUtc = CASE WHEN @SelectedOption IS NULL
                         THEN NULL ELSE SYSUTCDATETIME() END,
    IsCorrect = NULL
WHERE ExamQuestionID = @QuestionID
  AND ExamAttemptID = @AttemptID
  AND (@SelectedOption IS NULL OR
      (@SelectedOption = 'A' AND OptionA IS NOT NULL) OR
      (@SelectedOption = 'B' AND OptionB IS NOT NULL) OR
      (@SelectedOption = 'C' AND OptionC IS NOT NULL) OR
      (@SelectedOption = 'D' AND OptionD IS NOT NULL))
  AND (QuestionType <> N'TrueFalse' OR
       @SelectedOption IS NULL OR @SelectedOption IN ('A', 'B'));";
                        using (var command = new SqlCommand(saveSql, connection, tx))
                        {
                            command.Parameters.Add("@AttemptID", SqlDbType.BigInt).Value = examAttemptID;
                            command.Parameters.Add("@QuestionID", SqlDbType.BigInt).Value = examQuestionID;
                            command.Parameters.Add("@SelectedOption", SqlDbType.Char, 1).Value =
                                answer == null ? (object)DBNull.Value : answer;
                            if (command.ExecuteNonQuery() != 1)
                                throw new InvalidOperationException(
                                    "Question does not belong to this attempt, or the option is invalid.");
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        // Atomically grades and closes the exam and its appointment.
        // The existing appointment creator is used for the required DVLD
        // Tests.CreatedByUserID field (no extra login for the examinee).
        // Repeated Submit returns the saved result instead of creating a second Test.
        public static OfficialExamFinalResult SubmitExam(long examAttemptID)
        {
            if (examAttemptID <= 0)
                throw new ArgumentOutOfRangeException(nameof(examAttemptID));

            using (var connection = new SqlConnection(
                clsDataAccessSettings.ConnectionString))
            {
                connection.Open();
                using (var tx = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        const string attemptSql = @"
SELECT ExamAttemptID, TestAppointmentID, Status,
       QuestionCount, RequiredCorrectAnswers, DeadlineAtUtc,
       CorrectAnswers, Passed, TestID, FinishedAtUtc,
       SYSUTCDATETIME() AS ServerNowUtc
FROM dbo.OfficialExamAttempts WITH (UPDLOCK, HOLDLOCK)
WHERE ExamAttemptID = @AttemptID;";
                        OfficialExamFinalResult result = new OfficialExamFinalResult
                        { ExamAttemptID = examAttemptID };
                        DateTime deadline;
                        DateTime nowUtc;
                        bool alreadyFinished = false;
                        using (var command = new SqlCommand(attemptSql, connection, tx))
                        {
                            command.Parameters.Add("@AttemptID", SqlDbType.BigInt).Value = examAttemptID;
                            using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                            {
                                if (!reader.Read())
                                    throw new InvalidOperationException("Exam attempt was not found.");
                                result.TestAppointmentID = Convert.ToInt32(reader["TestAppointmentID"]);
                                result.QuestionCount = Convert.ToInt32(reader["QuestionCount"]);
                                result.RequiredCorrectAnswers = Convert.ToInt32(reader["RequiredCorrectAnswers"]);
                                result.Status = reader["Status"].ToString();
                                deadline = Convert.ToDateTime(reader["DeadlineAtUtc"]);
                                nowUtc = Convert.ToDateTime(reader["ServerNowUtc"]);
                                if (result.Status == "Submitted" || result.Status == "TimedOut")
                                {
                                    // Previously committed: safe, idempotent retry.
                                    result.TestID = Convert.ToInt32(reader["TestID"]);
                                    result.CorrectAnswers = Convert.ToInt32(reader["CorrectAnswers"]);
                                    result.Passed = Convert.ToBoolean(reader["Passed"]);
                                    result.FinishedAtUtc = Convert.ToDateTime(reader["FinishedAtUtc"]);
                                    alreadyFinished = true;
                                }
                                if (!alreadyFinished && result.Status != "InProgress")
                                    throw new InvalidOperationException("The exam cannot be submitted.");
                            }
                        }

                        // Dispose the reader before committing a replayed result.
                        if (alreadyFinished)
                        {
                            tx.Commit();
                            return result;
                        }

                        // Serialize against appointment locking and check that
                        // a legacy/manual DVLD Test has not already been written.
                        const string appointmentSql = @"
SELECT IsLocked, CreatedByUserID
FROM dbo.TestAppointments WITH (UPDLOCK, HOLDLOCK)
WHERE TestAppointmentID = @AppointmentID;";
                        int appointmentCreatorUserID;
                        using (var command = new SqlCommand(appointmentSql, connection, tx))
                        {
                            command.Parameters.Add("@AppointmentID", SqlDbType.Int)
                                .Value = result.TestAppointmentID;
                            using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                            {
                                if (!reader.Read() || Convert.ToBoolean(reader["IsLocked"]))
                                    throw new InvalidOperationException(
                                        "Appointment is missing or already closed.");
                                appointmentCreatorUserID = Convert.ToInt32(
                                    reader["CreatedByUserID"]);
                            }
                        }

                        const string priorTestSql = @"
SELECT COUNT(1) FROM dbo.Tests WITH (UPDLOCK, HOLDLOCK)
WHERE TestAppointmentID = @AppointmentID;";
                        using (var command = new SqlCommand(priorTestSql, connection, tx))
                        {
                            command.Parameters.Add("@AppointmentID", SqlDbType.Int)
                                .Value = result.TestAppointmentID;
                            if (Convert.ToInt32(command.ExecuteScalar()) != 0)
                                throw new InvalidOperationException("A result already exists for this appointment.");
                        }

                        // Mark ALL unanswered questions incorrect. Never trust
                        // the client for correct answers or elapsed time.
                        const string gradeSql = @"
UPDATE dbo.OfficialExamQuestions
SET IsCorrect = CASE WHEN SelectedOption IS NOT NULL
                            AND SelectedOption = CorrectOption
                     THEN 1 ELSE 0 END
WHERE ExamAttemptID = @AttemptID;
SELECT COUNT(1) FROM dbo.OfficialExamQuestions
WHERE ExamAttemptID = @AttemptID AND IsCorrect = 1;";
                        using (var command = new SqlCommand(gradeSql, connection, tx))
                        {
                            command.Parameters.Add("@AttemptID", SqlDbType.BigInt).Value = examAttemptID;
                            result.CorrectAnswers = Convert.ToInt32(command.ExecuteScalar());
                        }
                        result.Passed = result.CorrectAnswers >= result.RequiredCorrectAnswers;
                        result.Status = nowUtc >= deadline ? "TimedOut" : "Submitted";

                        const string createTestSql = @"
INSERT dbo.Tests(TestAppointmentID, TestResult, Notes, CreatedByUserID)
OUTPUT INSERTED.TestID
VALUES (@AppointmentID, @Passed, @Notes, @UserID);";
                        using (var command = new SqlCommand(createTestSql, connection, tx))
                        {
                            command.Parameters.Add("@AppointmentID", SqlDbType.Int)
                                .Value = result.TestAppointmentID;
                            command.Parameters.Add("@Passed", SqlDbType.Bit).Value = result.Passed;
                            command.Parameters.Add("@Notes", SqlDbType.NVarChar, 500).Value =
                                "Official written exam: " + result.CorrectAnswers + "/" +
                                result.QuestionCount + "; " + result.Status;
                            command.Parameters.Add("@UserID", SqlDbType.Int).Value = appointmentCreatorUserID;
                            result.TestID = Convert.ToInt32(command.ExecuteScalar());
                        }

                        const string closeAppointmentSql = @"
UPDATE dbo.TestAppointments SET IsLocked = 1
WHERE TestAppointmentID = @AppointmentID AND IsLocked = 0;";
                        using (var command = new SqlCommand(closeAppointmentSql, connection, tx))
                        {
                            command.Parameters.Add("@AppointmentID", SqlDbType.Int)
                                .Value = result.TestAppointmentID;
                            if (command.ExecuteNonQuery() != 1)
                                throw new InvalidOperationException("Could not close the appointment.");
                        }

                        const string closeAttemptSql = @"
UPDATE dbo.OfficialExamAttempts
SET Status = @Status, FinishedAtUtc = SYSUTCDATETIME(),
    CorrectAnswers = @CorrectAnswers, Passed = @Passed, TestID = @TestID
OUTPUT INSERTED.FinishedAtUtc
WHERE ExamAttemptID = @AttemptID AND Status = N'InProgress';";
                        using (var command = new SqlCommand(closeAttemptSql, connection, tx))
                        {
                            command.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = result.Status;
                            command.Parameters.Add("@CorrectAnswers", SqlDbType.SmallInt)
                                .Value = (short)result.CorrectAnswers;
                            command.Parameters.Add("@Passed", SqlDbType.Bit).Value = result.Passed;
                            command.Parameters.Add("@TestID", SqlDbType.Int).Value = result.TestID;
                            command.Parameters.Add("@AttemptID", SqlDbType.BigInt).Value = examAttemptID;
                            object saved = command.ExecuteScalar();
                            if (saved == null)
                                throw new InvalidOperationException("Could not finalize the exam attempt.");
                            result.FinishedAtUtc = Convert.ToDateTime(saved);
                        }

                        tx.Commit();
                        return result;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
