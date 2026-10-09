using System;
using System.Data;
using DVLD_DataAccess;
using System.Collections.Generic;
namespace DVLD_Buisness
{
    public class clsKnowledgeDocument
    {
        public int DocumentID { get; private set; }

        public string OriginalFileName { get; private set; }

        public string StoredFileName { get; private set; }

        public string FilePath { get; private set; }

        public long FileSizeBytes { get; private set; }

        public string ProcessingStatus { get; private set; }

        public int? TotalPages { get; private set; }

        public int ChunkCount { get; private set; }

        public DateTime UploadedAt { get; private set; }

        public DateTime? ProcessedAt { get; private set; }

        public string ErrorMessage { get; private set; }

        public bool IsActive { get; private set; }


        private clsKnowledgeDocument(
            int documentID,
            string originalFileName,
            string storedFileName,
            string filePath,
            long fileSizeBytes,
            string processingStatus,
            int? totalPages,
            int chunkCount,
            DateTime uploadedAt,
            DateTime? processedAt,
            string errorMessage,
            bool isActive)
        {
            DocumentID = documentID;

            OriginalFileName = originalFileName;

            StoredFileName = storedFileName;

            FilePath = filePath;

            FileSizeBytes = fileSizeBytes;

            ProcessingStatus = processingStatus;

            TotalPages = totalPages;

            ChunkCount = chunkCount;

            UploadedAt = uploadedAt;

            ProcessedAt = processedAt;

            ErrorMessage = errorMessage;

            IsActive = isActive;
        }


        public static clsKnowledgeDocument Find(
            int documentID)
        {
            string originalFileName = "";
            string storedFileName = "";
            string filePath = "";
            long fileSizeBytes = 0;
            string processingStatus = "";
            int? totalPages = null;
            int chunkCount = 0;
            DateTime uploadedAt = DateTime.MinValue;
            DateTime? processedAt = null;
            string errorMessage = null;
            bool isActive = false;


            bool isFound =
                clsKnowledgeDocumentData
                    .GetDocumentInfoByID(
                        documentID,
                        ref originalFileName,
                        ref storedFileName,
                        ref filePath,
                        ref fileSizeBytes,
                        ref processingStatus,
                        ref totalPages,
                        ref chunkCount,
                        ref uploadedAt,
                        ref processedAt,
                        ref errorMessage,
                        ref isActive);


            if (!isFound)
                return null;


            return new clsKnowledgeDocument(
                documentID,
                originalFileName,
                storedFileName,
                filePath,
                fileSizeBytes,
                processingStatus,
                totalPages,
                chunkCount,
                uploadedAt,
                processedAt,
                errorMessage,
                isActive);
        }


        public static int AddNewDocument(
            string OriginalFileName,
            string StoredFileName,
            string FilePath,
            long FileSizeBytes)
        {
            return clsKnowledgeDocumentData.AddNewDocument(
                OriginalFileName,
                StoredFileName,
                FilePath,
                FileSizeBytes);
        }


        public static DataTable GetAllDocuments()
        {
            return clsKnowledgeDocumentData
                .GetAllDocuments();
        }


        public static bool UpdateAfterTextExtraction(
            int DocumentID,
            int TotalPages)
        {
            return clsKnowledgeDocumentData
                .UpdateAfterTextExtraction(
                    DocumentID,
                    TotalPages);
        }


        public static bool MarkProcessingFailed(
            int DocumentID,
            string ErrorMessage)
        {
            return clsKnowledgeDocumentData
                .MarkProcessingFailed(
                    DocumentID,
                    ErrorMessage);
        }


        public static bool UpdateChunkCount(
            int DocumentID,
            int ChunkCount)
        {
            return clsKnowledgeDocumentData
                .UpdateChunkCount(
                    DocumentID,
                    ChunkCount);
        }


        public static bool MarkProcessingCompleted(
            int DocumentID,
            int ChunkCount)
        {
            return clsKnowledgeDocumentData
                .MarkProcessingCompleted(
                    DocumentID,
                    ChunkCount);
        }


        public static string GetFilePathByDocumentID(
            int DocumentID)
        {
            return clsKnowledgeDocumentData
                .GetFilePathByDocumentID(
                    DocumentID);
        }
        public bool Activate()
        {
            return _SetActive(true);
        }


        public bool Deactivate()
        {
            return _SetActive(false);
        }


        private bool _SetActive(bool isActive)
        {
            if (DocumentID <= 0)
                return false;

            if (IsActive == isActive)
                return true;

            bool updated =
                clsKnowledgeDocumentData.SetDocumentActive(
                    DocumentID,
                    isActive);

            if (updated)
            {
                IsActive = isActive;
            }

            return updated;
        }
        public static HashSet<int> GetActiveReadyDocumentIDs()
        {
            return clsKnowledgeDocumentData
                .GetActiveReadyDocumentIDs();
        }
    }
}