using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DVLD_DataAccess
{
    public class clsKnowledgeDocumentData
    {
        public static int AddNewDocument(
            string OriginalFileName,
            string StoredFileName,
            string FilePath,
            long FileSizeBytes)
        {
            int DocumentID = -1;

            using (SqlConnection connection =
                   new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = @"
                    INSERT INTO KnowledgeDocuments
                    (
                        OriginalFileName,
                        StoredFileName,
                        FilePath,
                        FileSizeBytes
                    )
                    VALUES
                    (
                        @OriginalFileName,
                        @StoredFileName,
                        @FilePath,
                        @FileSizeBytes
                    );

                    SELECT SCOPE_IDENTITY();";

                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue(
                        "@OriginalFileName", OriginalFileName);

                    command.Parameters.AddWithValue(
                        "@StoredFileName", StoredFileName);

                    command.Parameters.AddWithValue(
                        "@FilePath", FilePath);

                    command.Parameters.AddWithValue(
                        "@FileSizeBytes", FileSizeBytes);

                    try
                    {
                        connection.Open();

                        object result = command.ExecuteScalar();

                        if (result != null &&
                            int.TryParse(result.ToString(), out int insertedID))
                        {
                            DocumentID = insertedID;
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }

            return DocumentID;
        }


        public static DataTable GetAllDocuments()
        {
            DataTable dt = new DataTable();

            using (SqlConnection connection =
                   new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = @"
                    SELECT
                        DocumentID,
                        OriginalFileName,
                        StoredFileName,
                        FilePath,
                        FileSizeBytes,
                        ProcessingStatus,
                        TotalPages,
                        ChunkCount,
                        UploadedAt,
                        ProcessedAt,
                        ErrorMessage,
                        IsActive
                    FROM KnowledgeDocuments
                    ORDER BY UploadedAt DESC;";

                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    try
                    {
                        connection.Open();

                        using (SqlDataReader reader =
                               command.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }

            return dt;
        }
        public static bool UpdateAfterTextExtraction(
    int DocumentID,
    int TotalPages)
        {
            using (SqlConnection connection =
                   new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = @"
            UPDATE KnowledgeDocuments
            SET
                TotalPages = @TotalPages,
                ProcessingStatus = 'Processing',
                ErrorMessage = NULL
            WHERE DocumentID = @DocumentID;";

                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue(
                        "@DocumentID", DocumentID);

                    command.Parameters.AddWithValue(
                        "@TotalPages", TotalPages);

                    try
                    {
                        connection.Open();

                        return command.ExecuteNonQuery() > 0;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }
        public static bool MarkProcessingFailed(
    int DocumentID,
    string ErrorMessage)
        {
            using (SqlConnection connection =
                   new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = @"
            UPDATE KnowledgeDocuments
            SET
                ProcessingStatus = 'Failed',
                ErrorMessage = @ErrorMessage
            WHERE DocumentID = @DocumentID;";

                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue(
                        "@DocumentID", DocumentID);

                    command.Parameters.AddWithValue(
                        "@ErrorMessage",
                        string.IsNullOrWhiteSpace(ErrorMessage)
                            ? "Unknown processing error."
                            : ErrorMessage);

                    try
                    {
                        connection.Open();

                        return command.ExecuteNonQuery() > 0;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }
        public static bool UpdateChunkCount(
    int DocumentID,
    int ChunkCount)
        {
            using (SqlConnection connection =
                   new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = @"
            UPDATE KnowledgeDocuments
            SET ChunkCount = @ChunkCount
            WHERE DocumentID = @DocumentID;";

                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue(
                        "@DocumentID", DocumentID);

                    command.Parameters.AddWithValue(
                        "@ChunkCount", ChunkCount);

                    try
                    {
                        connection.Open();

                        return command.ExecuteNonQuery() > 0;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }
        public static bool MarkProcessingCompleted(
    int DocumentID,
    int ChunkCount)
        {
            using (SqlConnection connection =
                new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = @"
            UPDATE KnowledgeDocuments
            SET
                ProcessingStatus = 'Ready',
                ChunkCount = @ChunkCount,
                ProcessedAt = SYSDATETIME(),
                ErrorMessage = NULL
            WHERE DocumentID = @DocumentID;";

                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue(
                        "@DocumentID", DocumentID);

                    command.Parameters.AddWithValue(
                        "@ChunkCount", ChunkCount);

                    try
                    {
                        connection.Open();

                        return command.ExecuteNonQuery() > 0;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }
        public static string GetFilePathByDocumentID(
    int DocumentID)
        {
            using (SqlConnection connection =
                   new SqlConnection(
                       clsDataAccessSettings.ConnectionString))
            {
                string query = @"
            SELECT FilePath
            FROM KnowledgeDocuments
            WHERE DocumentID = @DocumentID
              AND IsActive = 1;";


                using (SqlCommand command =
                       new SqlCommand(
                           query,
                           connection))
                {
                    command.Parameters.AddWithValue(
                        "@DocumentID",
                        DocumentID);


                    connection.Open();


                    object result =
                        command.ExecuteScalar();


                    if (result == null ||
                        result == DBNull.Value)
                    {
                        return null;
                    }


                    return result.ToString();
                }
            }
        }

    }

}