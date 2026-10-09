using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace DVLD_DataAccess
{
    public class clsKnowledgeDocumentData
    {
        public static bool GetDocumentInfoByID(
    int documentID,
    ref string originalFileName,
    ref string storedFileName,
    ref string filePath,
    ref long fileSizeBytes,
    ref string processingStatus,
    ref int? totalPages,
    ref int chunkCount,
    ref DateTime uploadedAt,
    ref DateTime? processedAt,
    ref string errorMessage,
    ref bool isActive)
        {
            bool isFound = false;

            using (SqlConnection connection =
                   new SqlConnection(
                       clsDataAccessSettings.ConnectionString))
            {
                string query = @"
            SELECT
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
            WHERE DocumentID = @DocumentID;";

                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    command.Parameters.Add(
                        "@DocumentID",
                        SqlDbType.Int).Value = documentID;

                    connection.Open();

                    using (SqlDataReader reader =
                           command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            isFound = true;

                            originalFileName =
                                reader["OriginalFileName"].ToString();

                            storedFileName =
                                reader["StoredFileName"].ToString();

                            filePath =
                                reader["FilePath"].ToString();

                            fileSizeBytes =
                                Convert.ToInt64(
                                    reader["FileSizeBytes"]);

                            processingStatus =
                                reader["ProcessingStatus"].ToString();

                            totalPages =
                                reader["TotalPages"] == DBNull.Value
                                    ? (int?)null
                                    : Convert.ToInt32(
                                        reader["TotalPages"]);

                            chunkCount =
                                Convert.ToInt32(
                                    reader["ChunkCount"]);

                            uploadedAt =
                                Convert.ToDateTime(
                                    reader["UploadedAt"]);

                            processedAt =
                                reader["ProcessedAt"] == DBNull.Value
                                    ? (DateTime?)null
                                    : Convert.ToDateTime(
                                        reader["ProcessedAt"]);

                            errorMessage =
                                reader["ErrorMessage"] == DBNull.Value
                                    ? null
                                    : reader["ErrorMessage"].ToString();

                            isActive =
                                Convert.ToBoolean(
                                    reader["IsActive"]);
                        }
                    }
                }
            }

            return isFound;
        }
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
        public static bool SetDocumentActive(
    int documentID,
    bool isActive)
        {
            if (documentID <= 0)
                return false;

            using (SqlConnection connection =
                   new SqlConnection(
                       clsDataAccessSettings.ConnectionString))
            {
                string query = @"
            UPDATE KnowledgeDocuments
            SET IsActive = @IsActive
            WHERE DocumentID = @DocumentID;";

                using (SqlCommand command =
                       new SqlCommand(
                           query,
                           connection))
                {
                    command.Parameters.Add(
                        "@DocumentID",
                        SqlDbType.Int).Value =
                        documentID;

                    command.Parameters.Add(
                        "@IsActive",
                        SqlDbType.Bit).Value =
                        isActive;

                    connection.Open();

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }
        public static HashSet<int> GetActiveReadyDocumentIDs()
        {
            HashSet<int> documentIDs =
                new HashSet<int>();

            using (SqlConnection connection =
                   new SqlConnection(
                       clsDataAccessSettings.ConnectionString))
            {
                string query = @"
            SELECT DocumentID
            FROM KnowledgeDocuments
            WHERE IsActive = 1
              AND ProcessingStatus = 'Ready';";

                using (SqlCommand command =
                       new SqlCommand(
                           query,
                           connection))
                {
                    connection.Open();

                    using (SqlDataReader reader =
                           command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            documentIDs.Add(
                                Convert.ToInt32(
                                    reader["DocumentID"]));
                        }
                    }
                }
            }

            return documentIDs;
        }
    }

}