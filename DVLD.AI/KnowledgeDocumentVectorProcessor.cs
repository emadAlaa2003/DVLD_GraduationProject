namespace DVLD.AI
{
    public static class KnowledgeDocumentVectorProcessor
    {
        public static async Task ProcessAsync(
            int documentID,
            List<TextChunk> chunks)
        {
            if (documentID <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(documentID));
            }

            if (chunks == null)
            {
                throw new ArgumentNullException(
                    nameof(chunks));
            }

            if (chunks.Count == 0)
            {
                throw new InvalidOperationException(
                    "The document does not contain any chunks.");
            }


            foreach (TextChunk chunk in chunks)
            {
                float[] embedding =
                    await OllamaEmbeddingService
                        .GenerateEmbeddingAsync(
                            chunk.Text);


                await QdrantKnowledgeStore
                    .UpsertChunkAsync(
                        documentID,
                        chunk,
                        embedding);
            }
        }
    }
}