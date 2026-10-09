using Qdrant.Client;
using Qdrant.Client.Grpc;
using static Qdrant.Client.Grpc.Conditions;

namespace DVLD.AI
{
    public static class QdrantKnowledgeStore
    {
        public static async Task UpsertChunkAsync(
            int documentID,
            TextChunk chunk,
            float[] embedding)
        {
            if (chunk == null)
            {
                throw new ArgumentNullException(
                    nameof(chunk));
            }

            if (embedding == null ||
                embedding.Length !=
                    (int)QdrantConnection.EmbeddingSize)
            {
                throw new ArgumentException(
                    "Embedding must contain exactly 1024 dimensions.",
                    nameof(embedding));
            }


            QdrantClient client =
                QdrantConnection.CreateClient();


            ulong pointID =
                ((ulong)(uint)documentID << 32) |
                (uint)chunk.ChunkIndex;


            PointStruct point =
                new PointStruct
                {
                    Id = pointID,

                    Vectors = embedding,

                    Payload =
                    {
                        ["document_id"] =
                            documentID,

                        ["page_number"] =
                            chunk.PageNumber,

                        ["chunk_index"] =
                            chunk.ChunkIndex,

                        ["text"] =
                            chunk.Text
                    }
                };


            await client.UpsertAsync(
                QdrantConnection.KnowledgeCollectionName,
                new List<PointStruct>
                {
                    point
                });
        }


        public static async Task<IReadOnlyList<ScoredPoint>>
            SearchAsync(
                float[] queryEmbedding,
                ulong limit = 5)
        {
            if (queryEmbedding == null ||
                queryEmbedding.Length !=
                    (int)QdrantConnection.EmbeddingSize)
            {
                throw new ArgumentException(
                    "Query embedding must contain exactly 1024 dimensions.",
                    nameof(queryEmbedding));
            }


            QdrantClient client =
                QdrantConnection.CreateClient();


            IReadOnlyList<ScoredPoint> results =
                await client.SearchAsync(
                    QdrantConnection.KnowledgeCollectionName,
                    queryEmbedding,
                    limit: limit);


            return results;
        }


        public static async Task<List<TextChunk>>
            GetDocumentChunksAsync(
                int documentID)
        {
            if (documentID <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(documentID));
            }


            QdrantClient client =
                QdrantConnection.CreateClient();


            List<TextChunk> chunks =
                new List<TextChunk>();


            PointId? offset =
                null;


            do
            {
                ScrollResponse response =
                    await client.ScrollAsync(
                        collectionName:
                            QdrantConnection.KnowledgeCollectionName,

                        filter:
                            Match(
                                "document_id",
                                documentID),

                        limit:
                            100,

                        offset:
                            offset);


                foreach (RetrievedPoint point
                         in response.Result)
                {
                    if (!point.Payload.ContainsKey(
                            "text") ||
                        !point.Payload.ContainsKey(
                            "page_number") ||
                        !point.Payload.ContainsKey(
                            "chunk_index"))
                    {
                        continue;
                    }


                    chunks.Add(
                        new TextChunk
                        {
                            Text =
                                point.Payload["text"]
                                    .StringValue,

                            PageNumber =
                                (int)point.Payload["page_number"]
                                    .IntegerValue,

                            ChunkIndex =
                                (int)point.Payload["chunk_index"]
                                    .IntegerValue
                        });
                }


                offset =
                    response.NextPageOffset;

            }
            while (offset != null);


            return chunks
                .OrderBy(chunk =>
                    chunk.ChunkIndex)
                .ToList();
        }


        public static async Task DeleteDocumentChunksAsync(
            int documentID)
        {
            if (documentID <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(documentID));
            }


            QdrantClient client =
                QdrantConnection.CreateClient();


            await client.DeleteAsync(
                collectionName:
                    QdrantConnection.KnowledgeCollectionName,

                filter:
                    Match(
                        "document_id",
                        documentID));
        }
    }
}
