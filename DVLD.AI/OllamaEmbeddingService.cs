using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace DVLD.AI
{
    public static class OllamaEmbeddingService
    {
        private const string BaseUrl =
            "http://localhost:11434";

        private const string ModelName =
            "qwen3-embedding:0.6b";

        private const string RetrievalInstruction =
            "Given a web search query, retrieve relevant passages that answer the query";


        // تستخدم للـ Chunks / Documents
        // هنا نترك الموديل في الذاكرة لأننا قد نعالج
        // عدداً كبيراً من الـ Chunks وراء بعض.
        public static async Task<float[]> GenerateEmbeddingAsync(
            string text)
        {
            return await GenerateEmbeddingInternalAsync(
                text,
                unloadAfterRequest: false);
        }


        // تستخدم للأسئلة فقط.
        // بعد إنشاء Query Embedding نفرغ موديل الـ Embedding
        // من الذاكرة حتى نتيح مساحة لموديل الـ Chat.
        public static async Task<float[]> GenerateQueryEmbeddingAsync(
            string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                throw new ArgumentException(
                    "Question is required.",
                    nameof(question));
            }

            string instructedQuery =
                $"Instruct: {RetrievalInstruction}\n" +
                $"Query: {question}";

            return await GenerateEmbeddingInternalAsync(
                instructedQuery,
                unloadAfterRequest: true);
        }


        private static async Task<float[]>
            GenerateEmbeddingInternalAsync(
                string text,
                bool unloadAfterRequest)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException(
                    "Text is required.",
                    nameof(text));
            }


            using HttpClient client =
                new HttpClient();


            OllamaEmbedRequest request =
                new OllamaEmbedRequest
                {
                    Model =
                        ModelName,

                    Input =
                        text,

                    // 0 = Ollama يفرغ الموديل من الذاكرة
                    // مباشرة بعد انتهاء الطلب.
                    // null = نستخدم السلوك الطبيعي لـ Ollama.
                    KeepAlive =
                        unloadAfterRequest
                            ? 0
                            : null
                };


            HttpResponseMessage response =
                await client.PostAsJsonAsync(
                    $"{BaseUrl}/api/embed",
                    request);


            response.EnsureSuccessStatusCode();


            OllamaEmbedResponse? result =
                await response.Content
                    .ReadFromJsonAsync<OllamaEmbedResponse>();


            if (result == null ||
                result.Embeddings == null ||
                result.Embeddings.Count == 0)
            {
                throw new InvalidOperationException(
                    "Ollama did not return an embedding.");
            }


            return result.Embeddings[0];
        }


        private class OllamaEmbedRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } =
                string.Empty;


            [JsonPropertyName("input")]
            public string Input { get; set; } =
                string.Empty;


            [JsonPropertyName("keep_alive")]
            [JsonIgnore(
                Condition =
                    JsonIgnoreCondition.WhenWritingNull)]
            public int? KeepAlive { get; set; }
        }


        private class OllamaEmbedResponse
        {
            [JsonPropertyName("embeddings")]
            public List<float[]> Embeddings { get; set; } =
                new List<float[]>();
        }
    }
}