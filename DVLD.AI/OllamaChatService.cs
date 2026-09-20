using System.Net.Http.Json;
using System.Text.Json;

namespace DVLD.AI
{
    public static class OllamaChatService
    {
        private static readonly HttpClient _httpClient =
            new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };

        private const string OllamaChatUrl =
            "http://localhost:11434/api/chat";

        private const string ChatModel =
            "qwen3:1.7b";

        public static async Task<string> GenerateAnswerAsync(
            string question,
            IReadOnlyList<string> contextChunks)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                throw new ArgumentException(
                    "Question is required.",
                    nameof(question));
            }

            if (contextChunks == null ||
                contextChunks.Count == 0)
            {
                throw new ArgumentException(
                    "At least one context chunk is required.",
                    nameof(contextChunks));
            }

            string context =
                string.Join(
                    "\n\n---\n\n",
                    contextChunks);

            string systemPrompt =
                """
                أنت مساعد متخصص في السلامة المرورية.

                أجب عن سؤال المستخدم اعتماداً فقط على المعلومات
                الموجودة في السياق المرفق.

                القواعد:
                - لا تخترع معلومات غير موجودة في السياق.
                - إذا لم تجد إجابة واضحة في السياق، قل إن المعلومات
                  المتوفرة لا تكفي للإجابة.
                - أجب باللغة العربية بشكل واضح ومختصر.
                - لا تذكر أنك نموذج ذكاء اصطناعي.
                """;

            string userPrompt =
                $"""
                السياق:

                {context}

                السؤال:

                {question}

                أجب اعتماداً على السياق فقط.
                """;

            var request = new
            {
                model = ChatModel,

                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = systemPrompt
                    },

                    new
                    {
                        role = "user",
                        content = userPrompt
                    }
                },

                stream = false,

                think = false,

                options = new
                {
                    num_ctx = 2048,
                    num_predict = 250
                }
            };

            using HttpResponseMessage response =
                await _httpClient.PostAsJsonAsync(
                    OllamaChatUrl,
                    request);

            response.EnsureSuccessStatusCode();

            string json =
                await response.Content.ReadAsStringAsync();

            using JsonDocument document =
                JsonDocument.Parse(json);

            JsonElement root =
                document.RootElement;

            string answer =
                root
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

            if (string.IsNullOrWhiteSpace(answer))
            {
                throw new InvalidOperationException(
                    "Ollama returned an empty answer.");
            }

            return answer.Trim();
        }
    }
}