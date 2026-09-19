using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace DVLD.AI
{
    public static class QdrantConnection
    {
        private const string Host =
            "localhost";

        private const int GrpcPort =
            6334;


        public const string KnowledgeCollectionName =
            "dvld_knowledge";

        public const ulong EmbeddingSize =
            1024;


        public static QdrantClient CreateClient()
        {
            return new QdrantClient(
                Host,
                GrpcPort);
        }


        public static async Task<bool> TestConnectionAsync()
        {
            QdrantClient client =
                CreateClient();

            try
            {
                await client.ListCollectionsAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }


        public static async Task CreateKnowledgeCollectionAsync()
        {
            QdrantClient client =
                CreateClient();

            await client.CreateCollectionAsync(
                collectionName:
                    KnowledgeCollectionName,

                vectorsConfig:
                    new VectorParams
                    {
                        Size = EmbeddingSize,
                        Distance = Distance.Cosine
                    });
        }

    }
}