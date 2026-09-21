using System.Net.Http.Json;
using System.Text.Json;

namespace DVLD.AI
{
    public class QuestionGroundingValidationResult
    {
        public bool IsGrounded { get; set; }

        public bool IsAnswerSupported { get; set; }

        public bool IsExplanationSupported { get; set; }

        public bool IsClearAndWellWritten { get; set; }

        public bool HasSingleCorrectAnswer { get; set; }

        public int GroundingScore { get; set; }

        public int LanguageQualityScore { get; set; }

        public string Reason { get; set; } =
            string.Empty;


        public bool IsAccepted =>
            IsGrounded &&
            IsAnswerSupported &&
            IsExplanationSupported &&
            IsClearAndWellWritten &&
            HasSingleCorrectAnswer &&
            GroundingScore >= 80 &&
            LanguageQualityScore >= 70;
    }


    public static class OllamaQuestionGroundingValidatorService
    {
        private static readonly HttpClient _httpClient =
            new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };


        private const string OllamaChatUrl =
            "http://localhost:11434/api/chat";


        private const string ModelName =
            "qwen3:1.7b";


        public static async Task<QuestionGroundingValidationResult>
            ValidateAsync(
                string sourceText,
                GeneratedQuestion question)
        {
            if (string.IsNullOrWhiteSpace(sourceText))
            {
                throw new ArgumentException(
                    "Source text is required.",
                    nameof(sourceText));
            }


            if (question == null)
            {
                throw new ArgumentNullException(
                    nameof(question));
            }


            string correctAnswerText =
                GetCorrectAnswerText(
                    question);


            string systemPrompt =
                """
                أنت مدقق جودة مستقل لأسئلة امتحان قيادة.

                مهمتك ليست إنشاء سؤال جديد.

                مهمتك تقييم السؤال المقدم مقارنة بالنص المصدر فقط.

                قواعد إلزامية:

                1. لا تستخدم أي معرفة من خارج النص المصدر.
                2. لا تفترض صحة أي معلومة غير موجودة بوضوح في المصدر.
                3. يجب أن يكون السؤال قابلاً للإجابة من المصدر نفسه.
                4. يجب أن تكون الإجابة المحددة مدعومة بوضوح من المصدر.
                5. يجب أن يكون التفسير مدعوماً من المصدر.
                6. بالنسبة للاختيار من متعدد:
                   يجب أن يوجد جواب صحيح واحد فقط بصورة واضحة.
                7. ارفض السؤال إذا كان:
                   - عاماً بشكل مبالغ فيه.
                   - غامضاً.
                   - ركيك الصياغة.
                   - يحتوي معلومات مخترعة.
                   - يحتاج معرفة خارجية للإجابة.
                   - خياراته غير منطقية أو لغتها غير سليمة.
                8. لا تعطي درجة مرتفعة لمجرد أن الموضوع قريب
                   من النص. يجب أن تكون العلاقة مباشرة وواضحة.
                9. GroundingScore من 0 إلى 100 ويقيس مدى اعتماد
                   السؤال والإجابة والتفسير على النص المصدر.
                10. LanguageQualityScore من 0 إلى 100 ويقيس
                    وضوح وسلامة صياغة السؤال والخيارات بالعربية.
                11. كن صارماً. السؤال الضعيف يجب رفضه.
                12. أرجع JSON فقط بدون Markdown.

                الشكل المطلوب:

                {
                  "isGrounded": true,
                  "isAnswerSupported": true,
                  "isExplanationSupported": true,
                  "isClearAndWellWritten": true,
                  "hasSingleCorrectAnswer": true,
                  "groundingScore": 0,
                  "languageQualityScore": 0,
                  "reason": ""
                }
                """;


            string userPrompt =
                $"""
                --- بداية النص المصدر ---

                {sourceText}

                --- نهاية النص المصدر ---


                --- السؤال المراد تقييمه ---

                نوع السؤال:
                {question.QuestionType}

                نص السؤال:
                {question.QuestionText}

                الخيار A:
                {question.OptionA}

                الخيار B:
                {question.OptionB}

                الخيار C:
                {question.OptionC}

                الخيار D:
                {question.OptionD}

                CorrectOption:
                {question.CorrectOption}

                نص الإجابة الصحيحة:
                {correctAnswerText}

                Explanation:
                {question.Explanation}

                --- نهاية السؤال ---


                قيّم السؤال اعتماداً على النص المصدر فقط.

                مهم جداً:
                إذا كانت الإجابة الصحيحة غير مثبتة بوضوح
                في المصدر، اجعل isAnswerSupported = false.

                إذا احتاج السؤال معرفة عامة أو معلومات
                من خارج المصدر، اجعل isGrounded = false.

                إذا كانت صياغة السؤال أو الخيارات ركيكة
                أو غير طبيعية بالعربية، اجعل
                isClearAndWellWritten = false.

                أرجع JSON فقط.
                """;


            var request =
                new
                {
                    model =
                        ModelName,

                    messages =
                        new[]
                        {
                            new
                            {
                                role = "system",
                                content =
                                    systemPrompt
                            },

                            new
                            {
                                role = "user",
                                content =
                                    userPrompt
                            }
                        },

                    stream =
                        false,

                    think =
                        false,

                    format =
                        "json",

                    options =
                        new
                        {
                            num_ctx = 3072,

                            num_predict = 400,

                            temperature = 0.1
                        }
                };


            using HttpResponseMessage response =
                await _httpClient.PostAsJsonAsync(
                    OllamaChatUrl,
                    request);


            response.EnsureSuccessStatusCode();


            string responseJson =
                await response.Content
                    .ReadAsStringAsync();


            using JsonDocument responseDocument =
                JsonDocument.Parse(
                    responseJson);


            string content =
                responseDocument
                    .RootElement
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();


            if (string.IsNullOrWhiteSpace(
                    content))
            {
                throw new InvalidOperationException(
                    "Ollama returned an empty grounding validation result.");
            }


            QuestionGroundingValidationResult result =
                JsonSerializer
                    .Deserialize<QuestionGroundingValidationResult>(
                        content,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive =
                                true
                        });


            if (result == null)
            {
                throw new InvalidOperationException(
                    "Ollama returned an invalid grounding validation result.");
            }


            result.GroundingScore =
                Math.Clamp(
                    result.GroundingScore,
                    0,
                    100);


            result.LanguageQualityScore =
                Math.Clamp(
                    result.LanguageQualityScore,
                    0,
                    100);


            result.Reason =
                result.Reason?.Trim() ??
                string.Empty;


            return result;
        }


        private static string GetCorrectAnswerText(
            GeneratedQuestion question)
        {
            return question.CorrectOption?
                .Trim()
                .ToUpperInvariant() switch
            {
                "A" => question.OptionA,
                "B" => question.OptionB,
                "C" => question.OptionC,
                "D" => question.OptionD,

                _ => string.Empty
            };
        }
    }
}
