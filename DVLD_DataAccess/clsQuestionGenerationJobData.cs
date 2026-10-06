using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace DVLD_DataAccess
{
    public class clsQuestionGenerationJobData
    {
        public static int AddNewJob(
            int DocumentID,
            int MultipleChoiceCount,
            int TrueFalseCount)
        {
            int QuestionGenerationJobID = -1;

            using (SqlConnection connection =
                   new SqlConnection(
                       clsDataAccessSettings.ConnectionString))
            {
                string query = @"
                    INSERT INTO QuestionGenerationJobs
                    (
                        DocumentID,
                        MultipleChoiceCount,
                        TrueFalseCount,
                        Status,
                        RequestedAt
                    )
                    VALUES
                    (
                        @DocumentID,
                        @MultipleChoiceCount,
                        @TrueFalseCount,
                        'Pending',
                        SYSDATETIME()
                    );

                    SELECT SCOPE_IDENTITY();";


                using (SqlCommand command =
                       new SqlCommand(
                           query,
                           connection))
                {
                    command.Parameters.Add(
                        "@DocumentID",
                        SqlDbType.Int).Value =
                        DocumentID;

                    command.Parameters.Add(
                        "@MultipleChoiceCount",
                        SqlDbType.Int).Value =
                        MultipleChoiceCount;

                    command.Parameters.Add(
                        "@TrueFalseCount",
                        SqlDbType.Int).Value =
                        TrueFalseCount;


                    connection.Open();

                    object result =
                        command.ExecuteScalar();


                    if (result != null &&
                        result != DBNull.Value)
                    {
                        QuestionGenerationJobID =
                            Convert.ToInt32(result);
                    }
                }
            }


            return QuestionGenerationJobID;
        }


        public static bool MarkProcessing(
            int QuestionGenerationJobID)
        {
            using (SqlConnection connection =
                   new SqlConnection(
                       clsDataAccessSettings.ConnectionString))
            {
                string query = @"
                    UPDATE QuestionGenerationJobs
                    SET
                        Status = 'Processing',
                        StartedAt = SYSDATETIME(),
                        FinishedAt = NULL,
                        ErrorMessage = NULL
                    WHERE QuestionGenerationJobID =
                        @QuestionGenerationJobID;";


                using (SqlCommand command =
                       new SqlCommand(
                           query,
                           connection))
                {
                    command.Parameters.Add(
                        "@QuestionGenerationJobID",
                        SqlDbType.Int).Value =
                        QuestionGenerationJobID;


                    connection.Open();

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }


        public static bool MarkCompleted(
            int QuestionGenerationJobID,
            int GeneratedQuestionsCount)
        {
            using (SqlConnection connection =
                   new SqlConnection(
                       clsDataAccessSettings.ConnectionString))
            {
                string query = @"
                    UPDATE QuestionGenerationJobs
                    SET
                        Status = 'Completed',
                        FinishedAt = SYSDATETIME(),
                        GeneratedQuestionsCount =
                            @GeneratedQuestionsCount,
                        ErrorMessage = NULL
                    WHERE QuestionGenerationJobID =
                        @QuestionGenerationJobID;";


                using (SqlCommand command =
                       new SqlCommand(
                           query,
                           connection))
                {
                    command.Parameters.Add(
                        "@QuestionGenerationJobID",
                        SqlDbType.Int).Value =
                        QuestionGenerationJobID;

                    command.Parameters.Add(
                        "@GeneratedQuestionsCount",
                        SqlDbType.Int).Value =
                        GeneratedQuestionsCount;


                    connection.Open();

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }


        public static bool MarkFailed(
            int QuestionGenerationJobID,
            string ErrorMessage)
        {
            using (SqlConnection connection =
                   new SqlConnection(
                       clsDataAccessSettings.ConnectionString))
            {
                string query = @"
                    UPDATE QuestionGenerationJobs
                    SET
                        Status = 'Failed',
                        FinishedAt = SYSDATETIME(),
                        ErrorMessage = @ErrorMessage
                    WHERE QuestionGenerationJobID =
                        @QuestionGenerationJobID;";


                using (SqlCommand command =
                       new SqlCommand(
                           query,
                           connection))
                {
                    command.Parameters.Add(
                        "@QuestionGenerationJobID",
                        SqlDbType.Int).Value =
                        QuestionGenerationJobID;

                    command.Parameters.Add(
                        "@ErrorMessage",
                        SqlDbType.NVarChar,
                        2000).Value =
                        string.IsNullOrWhiteSpace(
                            ErrorMessage)
                            ? "Unknown generation error."
                            : ErrorMessage;


                    connection.Open();

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }


        public static bool GetJobInfoByID(
            int QuestionGenerationJobID,
            ref int DocumentID,
            ref int MultipleChoiceCount,
            ref int TrueFalseCount,
            ref string Status,
            ref DateTime RequestedAt,
            ref DateTime? StartedAt,
            ref DateTime? FinishedAt,
            ref int? GeneratedQuestionsCount,
            ref string ErrorMessage)
        {
            bool isFound = false;


            using (SqlConnection connection =
                   new SqlConnection(
                       clsDataAccessSettings.ConnectionString))
            {
                string query = @"
                    SELECT
                        DocumentID,
                        MultipleChoiceCount,
                        TrueFalseCount,
                        Status,
                        RequestedAt,
                        StartedAt,
                        FinishedAt,
                        GeneratedQuestionsCount,
                        ErrorMessage
                    FROM QuestionGenerationJobs
                    WHERE QuestionGenerationJobID =
                        @QuestionGenerationJobID;";


                using (SqlCommand command =
                       new SqlCommand(
                           query,
                           connection))
                {
                    command.Parameters.Add(
                        "@QuestionGenerationJobID",
                        SqlDbType.Int).Value =
                        QuestionGenerationJobID;


                    connection.Open();


                    using (SqlDataReader reader =
                           command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            isFound = true;


                            DocumentID =
                                Convert.ToInt32(
                                    reader["DocumentID"]);

                            MultipleChoiceCount =
                                Convert.ToInt32(
                                    reader["MultipleChoiceCount"]);

                            TrueFalseCount =
                                Convert.ToInt32(
                                    reader["TrueFalseCount"]);

                            Status =
                                reader["Status"]
                                    .ToString();

                            RequestedAt =
                                Convert.ToDateTime(
                                    reader["RequestedAt"]);


                            StartedAt =
                                reader["StartedAt"] ==
                                DBNull.Value
                                    ? (DateTime?)null
                                    : Convert.ToDateTime(
                                        reader["StartedAt"]);


                            FinishedAt =
                                reader["FinishedAt"] ==
                                DBNull.Value
                                    ? (DateTime?)null
                                    : Convert.ToDateTime(
                                        reader["FinishedAt"]);


                            GeneratedQuestionsCount =
                                reader[
                                    "GeneratedQuestionsCount"] ==
                                DBNull.Value
                                    ? (int?)null
                                    : Convert.ToInt32(
                                        reader[
                                            "GeneratedQuestionsCount"]);


                            ErrorMessage =
                                reader["ErrorMessage"] ==
                                DBNull.Value
                                    ? null
                                    : reader["ErrorMessage"]
                                        .ToString();
                        }
                    }
                }
            }


            return isFound;
        }
    }
}