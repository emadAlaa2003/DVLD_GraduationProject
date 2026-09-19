using System.Data;
using DVLD_DataAccess;

namespace DVLD_Buisness
{
    public class clsKnowledgeDocument
    {
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
            return clsKnowledgeDocumentData.GetAllDocuments();
        }
        public static bool UpdateAfterTextExtraction(
    int DocumentID,
    int TotalPages)
        {
            return clsKnowledgeDocumentData.UpdateAfterTextExtraction(
                DocumentID,
                TotalPages);
        }
        public static bool MarkProcessingFailed(
    int DocumentID,
    string ErrorMessage)
        {
            return clsKnowledgeDocumentData.MarkProcessingFailed(
                DocumentID,
                ErrorMessage);
        }
        public static bool UpdateChunkCount(
    int DocumentID,
    int ChunkCount)
        {
            return clsKnowledgeDocumentData.UpdateChunkCount(
                DocumentID,
                ChunkCount);
        }
        public static bool MarkProcessingCompleted(
    int DocumentID,
    int ChunkCount)
        {
            return clsKnowledgeDocumentData.MarkProcessingCompleted(
                DocumentID,
                ChunkCount);
        }
    }
}