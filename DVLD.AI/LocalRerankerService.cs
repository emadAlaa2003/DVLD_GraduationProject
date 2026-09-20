using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace DVLD.AI
{
    public class RerankerResult
    {
        public int Index { get; set; }

        public float Score { get; set; }
    }


    public static class LocalRerankerService
    {
        private const string BaseUrl =
            "http://localhost:8081";


        public static async Task<List<RerankerResult>> RerankAsync(
            string query,
            List<string> texts)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException(
                    "Query is required.",
                    nameof(query));
            }

            if (texts == null || texts.Count == 0)
            {
                throw new ArgumentException(
                    "Texts are required.",
                    nameof(texts));
            }


            using HttpClient client =
                new HttpClient();


            RerankRequest request =
                new RerankRequest
                {
                    Query = query,
                    Texts = texts
                };


            HttpResponseMessage response =
                await client.PostAsJsonAsync(
                    $"{BaseUrl}/rerank",
                    request);


            response.EnsureSuccessStatusCode();


            List<RerankResponseItem>? result =
                await response.Content
                    .ReadFromJsonAsync<List<RerankResponseItem>>();


            if (result == null)
            {
                throw new InvalidOperationException(
                    "Reranker did not return results.");
            }


            return result
                .Select(item =>
                    new RerankerResult
                    {
                        Index = item.Index,
                        Score = item.Score
                    })
                .OrderByDescending(item =>
                    item.Score)
                .ToList();
        }


        private class RerankRequest
        {
            [JsonPropertyName("query")]
            public string Query { get; set; } =
                string.Empty;


            [JsonPropertyName("texts")]
            public List<string> Texts { get; set; } =
                new List<string>();
        }


        private class RerankResponseItem
        {
            [JsonPropertyName("index")]
            public int Index { get; set; }


            [JsonPropertyName("score")]
            public float Score { get; set; }
        }
    }
}