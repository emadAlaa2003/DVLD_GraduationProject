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


        public static async Task<float[]> GenerateEmbeddingAsync(
            string text)
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
                    Model = ModelName,
                    Input = text
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
        }


        private class OllamaEmbedResponse
        {
            [JsonPropertyName("embeddings")]
            public List<float[]> Embeddings { get; set; } =
                new List<float[]>();
        }
    }
}