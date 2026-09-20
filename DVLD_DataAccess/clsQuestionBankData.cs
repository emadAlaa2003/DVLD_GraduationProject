using System;
using System.Data;
using Microsoft.Data.SqlClient;
namespace DVLD_DataAccess
{
    public static class clsQuestionBankData
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
            int questionID = -1;

            using (SqlConnection connection =
                   new SqlConnection(
                       clsDataAccessSettings.ConnectionString))
            {
                string query = @"
                    INSERT INTO QuestionBank
                    (
                        QuestionText,
                        QuestionType,
                        OptionA,
                        OptionB,
                        OptionC,
                        OptionD,
                        CorrectOption,
                        Explanation,
                        SourceDocumentID,
                        SourcePageNumber
                    )
                    VALUES
                    (
                        @QuestionText,
                        @QuestionType,
                        @OptionA,
                        @OptionB,
                        @OptionC,
                        @OptionD,
                        @CorrectOption,
                        @Explanation,
                        @SourceDocumentID,
                        @SourcePageNumber
                    );

                    SELECT SCOPE_IDENTITY();";


                using (SqlCommand command =
                       new SqlCommand(
                           query,
                           connection))
                {
                    command.Parameters.Add(
                        "@QuestionText",
                        SqlDbType.NVarChar,
                        1000).Value =
                        questionText;


                    command.Parameters.Add(
                        "@QuestionType",
                        SqlDbType.NVarChar,
                        20).Value =
                        questionType;


                    command.Parameters.Add(
                        "@OptionA",
                        SqlDbType.NVarChar,
                        500).Value =
                        optionA;


                    command.Parameters.Add(
                        "@OptionB",
                        SqlDbType.NVarChar,
                        500).Value =
                        optionB;


                    command.Parameters.Add(
                        "@OptionC",
                        SqlDbType.NVarChar,
                        500).Value =
                        string.IsNullOrWhiteSpace(optionC)
                            ? (object)DBNull.Value
                            : optionC;


                    command.Parameters.Add(
                        "@OptionD",
                        SqlDbType.NVarChar,
                        500).Value =
                        string.IsNullOrWhiteSpace(optionD)
                            ? (object)DBNull.Value
                            : optionD;


                    command.Parameters.Add(
                        "@CorrectOption",
                        SqlDbType.Char,
                        1).Value =
                        correctOption;


                    command.Parameters.Add(
                        "@Explanation",
                        SqlDbType.NVarChar,
                        2000).Value =
                        string.IsNullOrWhiteSpace(explanation)
                            ? (object)DBNull.Value
                            : explanation;


                    command.Parameters.Add(
                        "@SourceDocumentID",
                        SqlDbType.Int).Value =
                        sourceDocumentID.HasValue
                            ? (object)sourceDocumentID.Value
                            : DBNull.Value;


                    command.Parameters.Add(
                        "@SourcePageNumber",
                        SqlDbType.Int).Value =
                        sourcePageNumber.HasValue
                            ? (object)sourcePageNumber.Value
                            : DBNull.Value;


                    connection.Open();

                    object result =
                        command.ExecuteScalar();

                    if (result != null &&
                        result != DBNull.Value)
                    {
                        questionID =
                            Convert.ToInt32(result);
                    }
                }
            }

            return questionID;
        }
    }
}