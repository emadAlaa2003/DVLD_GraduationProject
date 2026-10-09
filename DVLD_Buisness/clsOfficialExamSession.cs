using System;
using DVLD_DataAccess;

namespace DVLD_Buisness
{
    public static class clsOfficialExamSession
    {
        public sealed class FinalResult
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

        public static void SaveAnswer(long examAttemptID, long examQuestionID,
                                      string selectedOption)
        {
            clsOfficialExamSessionData.SaveAnswer(examAttemptID, examQuestionID,
                                                   selectedOption);
        }

        public static FinalResult SubmitExam(long examAttemptID)
        {
            OfficialExamFinalResult saved = clsOfficialExamSessionData.SubmitExam(examAttemptID);
            return new FinalResult
            {
                ExamAttemptID = saved.ExamAttemptID,
                TestAppointmentID = saved.TestAppointmentID,
                TestID = saved.TestID,
                QuestionCount = saved.QuestionCount,
                RequiredCorrectAnswers = saved.RequiredCorrectAnswers,
                CorrectAnswers = saved.CorrectAnswers,
                Passed = saved.Passed,
                Status = saved.Status,
                FinishedAtUtc = DateTime.SpecifyKind(saved.FinishedAtUtc, DateTimeKind.Utc)
            };
        }
    }
}
