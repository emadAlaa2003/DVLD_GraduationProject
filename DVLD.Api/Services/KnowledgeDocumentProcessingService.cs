using DVLD.AI;
using DVLD_Buisness;

namespace DVLD.Api.Services
{
    public static class KnowledgeDocumentProcessingService
    {
        public static async Task ProcessAsync(
            int documentID,
            string filePath)
        {
            try
            {
                PdfExtractionResult extractionResult =
                    PdfTextExtractor.Extract(filePath);

                clsKnowledgeDocument.UpdateAfterTextExtraction(
                    documentID,
                    extractionResult.TotalPages);

                List<TextChunk> chunks =
                    TextChunker.CreateChunks(extractionResult)
                        .Where(chunk =>
                            !string.IsNullOrWhiteSpace(chunk.Text))
                        .ToList();

                if (chunks.Count == 0)
                {
                    throw new InvalidOperationException(
                        "No text chunks were created.");
                }

                clsKnowledgeDocument.UpdateChunkCount(
                    documentID,
                    chunks.Count);

                await KnowledgeDocumentVectorProcessor.ProcessAsync(
                    documentID,
                    chunks);

                clsKnowledgeDocument.MarkProcessingCompleted(
                    documentID,
                    chunks.Count);
            }
            catch (Exception ex)
            {
                clsKnowledgeDocument.MarkProcessingFailed(
                    documentID,
                    ex.Message);

                throw;
            }
        }
    }
}