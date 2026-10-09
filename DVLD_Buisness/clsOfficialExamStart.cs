using System;
using System.Collections.Generic;
using DVLD_DataAccess;

namespace DVLD_Buisness
{
    // Exam operations for an in-centre, supervised exam application.
    public static class clsOfficialExamStart
    {
        public sealed class CandidateQuestion
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

        public sealed class StartResult
        {
            public long ExamAttemptID { get; set; }
            public int TestAppointmentID { get; set; }
            public DateTime StartedAtUtc { get; set; }
            public DateTime DeadlineAtUtc { get; set; }
            public List<CandidateQuestion> Questions { get; set; }
                = new List<CandidateQuestion>();
        }

        // Find the eligible written-test appointment by national number.
        // Caller supplies NO appointment ID. StartExam rechecks everything
        // inside a SQL transaction before creating an attempt.
        public static StartResult StartByNationalNo(
            string nationalNo,
            int questionCount,
            int requiredCorrectAnswers,
            int durationSeconds,
            int startEarlyMinutes,
            int startLateMinutes)
        {
            if (string.IsNullOrWhiteSpace(nationalNo))
                throw new ArgumentException("National number is required.", nameof(nationalNo));

            DateTime now = DateTime.Now;
            int selectedAppointmentID = 0;
            DateTime selectedDate = DateTime.MaxValue;

            foreach (OfficialExamAppointmentInfo appointment in
                clsOfficialExamData.GetWrittenAppointmentsByNationalNo(nationalNo))
            {
                if (appointment.TestTypeID != 2 || appointment.ApplicationStatus != 1 ||
                    appointment.IsLocked || appointment.HasRecordedTest ||
                    appointment.HasOfficialAttempt || !appointment.HasPassedVisionTest ||
                    now < appointment.AppointmentDate.AddMinutes(-startEarlyMinutes) ||
                    now > appointment.AppointmentDate.AddMinutes(startLateMinutes))
                {
                    continue;
                }

                if (appointment.AppointmentDate < selectedDate)
                {
                    selectedDate = appointment.AppointmentDate;
                    selectedAppointmentID = appointment.TestAppointmentID;
                }
            }

            if (selectedAppointmentID == 0)
                throw new InvalidOperationException(
                    "No eligible written exam appointment is available now.");

            return StartForVerifiedCandidate(
                selectedAppointmentID, nationalNo, questionCount,
                requiredCorrectAnswers, durationSeconds,
                startEarlyMinutes, startLateMinutes);
        }

        // All policy values come from trusted server configuration.
        public static StartResult StartForVerifiedCandidate(
            int testAppointmentID,
            string nationalNo,
            int questionCount,
            int requiredCorrectAnswers,
            int durationSeconds,
            int startEarlyMinutes,
            int startLateMinutes)
        {
            if (string.IsNullOrWhiteSpace(nationalNo))
                throw new ArgumentException("National number is required.", nameof(nationalNo));

            // The DataAccess operation rechecks appointment eligibility under
            // a serializable SQL transaction, freezes approved questions and
            // returns only candidate-safe fields (no correct answers).
            OfficialExamStartResult stored = clsOfficialExamStartData.StartExam(
                testAppointmentID,
                nationalNo.Trim(),
                questionCount,
                requiredCorrectAnswers,
                durationSeconds,
                startEarlyMinutes,
                startLateMinutes);

            StartResult result = new StartResult
            {
                ExamAttemptID = stored.ExamAttemptID,
                TestAppointmentID = stored.TestAppointmentID,
                // SQL datetime2 has no time-zone metadata. These values were
                // created with SYSUTCDATETIME(), so mark them UTC explicitly.
                StartedAtUtc = DateTime.SpecifyKind(stored.StartedAtUtc, DateTimeKind.Utc),
                DeadlineAtUtc = DateTime.SpecifyKind(stored.DeadlineAtUtc, DateTimeKind.Utc)
            };

            foreach (OfficialExamCandidateQuestion question in stored.Questions)
            {
                result.Questions.Add(new CandidateQuestion
                {
                    ExamQuestionID = question.ExamQuestionID,
                    QuestionNumber = question.QuestionNumber,
                    QuestionType = question.QuestionType,
                    QuestionText = question.QuestionText,
                    OptionA = question.OptionA,
                    OptionB = question.OptionB,
                    OptionC = question.OptionC,
                    OptionD = question.OptionD
                });
            }

            return result;
        }
    }
}
