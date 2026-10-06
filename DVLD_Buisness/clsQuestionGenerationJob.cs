using System;
using DVLD_DataAccess;

namespace DVLD_Buisness
{
    public class clsQuestionGenerationJob
    {
        public int QuestionGenerationJobID { get; private set; }

        public int DocumentID { get; private set; }

        public int MultipleChoiceCount { get; private set; }

        public int TrueFalseCount { get; private set; }

        public string Status { get; private set; }

        public DateTime RequestedAt { get; private set; }

        public DateTime? StartedAt { get; private set; }

        public DateTime? FinishedAt { get; private set; }

        public int? GeneratedQuestionsCount { get; private set; }

        public string ErrorMessage { get; private set; }


        private clsQuestionGenerationJob(
            int questionGenerationJobID,
            int documentID,
            int multipleChoiceCount,
            int trueFalseCount,
            string status,
            DateTime requestedAt,
            DateTime? startedAt,
            DateTime? finishedAt,
            int? generatedQuestionsCount,
            string errorMessage)
        {
            QuestionGenerationJobID =
                questionGenerationJobID;

            DocumentID =
                documentID;

            MultipleChoiceCount =
                multipleChoiceCount;

            TrueFalseCount =
                trueFalseCount;

            Status =
                status;

            RequestedAt =
                requestedAt;

            StartedAt =
                startedAt;

            FinishedAt =
                finishedAt;

            GeneratedQuestionsCount =
                generatedQuestionsCount;

            ErrorMessage =
                errorMessage;
        }


        public static clsQuestionGenerationJob Find(
            int QuestionGenerationJobID)
        {
            int documentID = 0;
            int multipleChoiceCount = 0;
            int trueFalseCount = 0;

            string status = "";

            DateTime requestedAt =
                DateTime.MinValue;

            DateTime? startedAt =
                null;

            DateTime? finishedAt =
                null;

            int? generatedQuestionsCount =
                null;

            string errorMessage =
                null;


            bool isFound =
                clsQuestionGenerationJobData
                    .GetJobInfoByID(
                        QuestionGenerationJobID,
                        ref documentID,
                        ref multipleChoiceCount,
                        ref trueFalseCount,
                        ref status,
                        ref requestedAt,
                        ref startedAt,
                        ref finishedAt,
                        ref generatedQuestionsCount,
                        ref errorMessage);


            if (!isFound)
                return null;


            return new clsQuestionGenerationJob(
                QuestionGenerationJobID,
                documentID,
                multipleChoiceCount,
                trueFalseCount,
                status,
                requestedAt,
                startedAt,
                finishedAt,
                generatedQuestionsCount,
                errorMessage);
        }


        public static int AddNewJob(
            int DocumentID,
            int MultipleChoiceCount,
            int TrueFalseCount)
        {
            return clsQuestionGenerationJobData
                .AddNewJob(
                    DocumentID,
                    MultipleChoiceCount,
                    TrueFalseCount);
        }


        public static bool MarkProcessing(
            int QuestionGenerationJobID)
        {
            return clsQuestionGenerationJobData
                .MarkProcessing(
                    QuestionGenerationJobID);
        }


        public static bool MarkCompleted(
            int QuestionGenerationJobID,
            int GeneratedQuestionsCount)
        {
            return clsQuestionGenerationJobData
                .MarkCompleted(
                    QuestionGenerationJobID,
                    GeneratedQuestionsCount);
        }


        public static bool MarkFailed(
            int QuestionGenerationJobID,
            string ErrorMessage)
        {
            return clsQuestionGenerationJobData
                .MarkFailed(
                    QuestionGenerationJobID,
                    ErrorMessage);
        }
    }
}