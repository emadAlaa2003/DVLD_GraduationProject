using DVLD_DataAccess;

namespace DVLD_Buisness
{
    public static class clsQuestionBank
    {
        public static int AddNewQuestion(
            string questionText,
            string questionType,
            string optionA,
            string optionB,
            string optionC,
            string optionD,
            string correctOption,
            string explanation,
            int? sourceDocumentID,
            int? sourcePageNumber)
        {
            return clsQuestionBankData.AddNewQuestion(
                questionText,
                questionType,
                optionA,
                optionB,
                optionC,
                optionD,
                correctOption,
                explanation,
                sourceDocumentID,
                sourcePageNumber);
        }
    }
}