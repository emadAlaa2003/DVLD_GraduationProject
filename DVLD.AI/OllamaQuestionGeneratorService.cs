using System.Net.Http.Json;
using System.Text.Json;

namespace DVLD.AI
{
    public class GeneratedQuestion
    {
        public string QuestionText { get; set; } =
            string.Empty;

        public string QuestionType { get; set; } =
            string.Empty;

        public string OptionA { get; set; } =
            string.Empty;

        public string OptionB { get; set; } =
            string.Empty;

        public string OptionC { get; set; } =
            string.Empty;

        public string OptionD { get; set; } =
            string.Empty;

        public string CorrectOption { get; set; } =
            string.Empty;

        public string Explanation { get; set; } =
            string.Empty;
    }


    public static class OllamaQuestionGeneratorService
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


        public static async Task<GeneratedQuestion>
            GenerateQuestionAsync(
                string sourceText,
                string questionType)
        {
            if (string.IsNullOrWhiteSpace(sourceText))
            {
                throw new ArgumentException(
                    "Source text is required.",
                    nameof(sourceText));
            }


            if (questionType != "MultipleChoice" &&
                questionType != "TrueFalse")
            {
                throw new ArgumentException(
                    "Question type must be MultipleChoice or TrueFalse.",
                    nameof(questionType));
            }


            string systemPrompt =
                BuildSystemPrompt(
                    questionType);


            string userPrompt =
                $"""
                هذا هو النص المصدر الوحيد المسموح لك بالاعتماد عليه:

                --- بداية المصدر ---

                {sourceText}

                --- نهاية المصدر ---

                أنشئ الآن سؤالاً امتحانياً واحداً فقط
                اعتماداً على معلومة واضحة ومباشرة من المصدر.

                لا تشرح المطلوب.
                لا تكرر التعليمات.
                لا تذكر نوع السؤال داخل نص السؤال.
                لا تنشئ سؤالاً عن طريقة إنشاء الأسئلة.
                أرجع JSON فقط.
                """;


            var request = new
            {
                model = ModelName,

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

                format = "json",

                options = new
                {
                    num_ctx = 2048,
                    num_predict = 600,
                    temperature = 0.3
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


            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException(
                    "Ollama returned an empty question.");
            }


            GeneratedQuestion question =
                JsonSerializer.Deserialize<GeneratedQuestion>(
                    content,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });


            ValidateGeneratedQuestion(
                question,
                questionType);


            return question;
        }


        private static string BuildSystemPrompt(
            string questionType)
        {
            if (questionType == "TrueFalse")
            {
                return
                    """
                    أنت مولد أسئلة لامتحان قيادة باللغة العربية.

                    مهمتك إنشاء سؤال صح أو خطأ واحد فقط
                    من النص المصدر الذي سيرسله المستخدم.

                    شروط إلزامية:

                    1. السؤال يجب أن يكون باللغة العربية.
                    2. السؤال يجب أن يعتمد على معلومة موجودة
                       بوضوح في النص المصدر.
                    3. لا تستخدم معلومات من ذاكرتك أو من خارج المصدر.
                    4. لا تكرر أي تعليمات موجودة في الطلب.
                    5. لا تكتب:
                       "نوع السؤال المطلوب"
                       أو "TrueFalse"
                       أو "MultipleChoice"
                       داخل QuestionText.
                    6. QuestionText يجب أن يكون عبارة أو سؤالاً
                       معرفياً حقيقياً عن السلامة المرورية.
                    7. OptionA يجب أن تكون "صح".
                    8. OptionB يجب أن تكون "خطأ".
                    9. OptionC و OptionD يجب أن تكونا فارغتين.
                    10. CorrectOption يجب أن تكون A أو B فقط.
                    11. Explanation يجب أن يشرح سبب الإجابة
                        اعتماداً على المصدر فقط.
                    12. لا تكتب Markdown.
                    13. أرجع JSON فقط.

                    الشكل المطلوب:

                    {
                      "questionText": "",
                      "questionType": "TrueFalse",
                      "optionA": "صح",
                      "optionB": "خطأ",
                      "optionC": "",
                      "optionD": "",
                      "correctOption": "",
                      "explanation": ""
                    }
                    """;
            }


            return
                """
                أنت مولد أسئلة لامتحان قيادة باللغة العربية.

                مهمتك إنشاء سؤال اختيار من متعدد واحد فقط
                من النص المصدر الذي سيرسله المستخدم.

                شروط إلزامية:

                1. السؤال يجب أن يكون باللغة العربية.
                2. السؤال يجب أن يعتمد على معلومة موجودة
                   بوضوح في النص المصدر.
                3. لا تستخدم معلومات من ذاكرتك أو من خارج المصدر.
                4. لا تكرر أي تعليمات موجودة في الطلب.
                5. لا تكتب:
                   "نوع السؤال المطلوب"
                   أو "TrueFalse"
                   أو "MultipleChoice"
                   داخل QuestionText.
                6. QuestionText يجب أن يكون سؤالاً حقيقياً
                   عن السلامة المرورية أو القيادة.
                7. أنشئ أربعة خيارات A و B و C و D.
                8. يجب أن تكون الخيارات الأربعة مختلفة.
                9. يوجد جواب صحيح واحد فقط.
                10. CorrectOption يجب أن تكون
                    A أو B أو C أو D فقط.
                11. Explanation يجب أن يشرح الإجابة الصحيحة
                    اعتماداً على المصدر فقط.
                12. لا تكتب Markdown.
                13. أرجع JSON فقط.

                الشكل المطلوب:

                {
                  "questionText": "",
                  "questionType": "MultipleChoice",
                  "optionA": "",
                  "optionB": "",
                  "optionC": "",
                  "optionD": "",
                  "correctOption": "",
                  "explanation": ""
                }
                """;
        }


        private static void ValidateGeneratedQuestion(
            GeneratedQuestion question,
            string expectedQuestionType)
        {
            if (question == null)
            {
                throw new InvalidOperationException(
                    "Ollama returned an invalid question.");
            }


            question.QuestionText =
                question.QuestionText?.Trim() ??
                string.Empty;

            question.QuestionType =
                expectedQuestionType;

            question.OptionA =
                question.OptionA?.Trim() ??
                string.Empty;

            question.OptionB =
                question.OptionB?.Trim() ??
                string.Empty;

            question.OptionC =
                question.OptionC?.Trim() ??
                string.Empty;

            question.OptionD =
                question.OptionD?.Trim() ??
                string.Empty;

            question.CorrectOption =
                question.CorrectOption?
                    .Trim()
                    .ToUpperInvariant() ??
                string.Empty;

            question.Explanation =
                question.Explanation?.Trim() ??
                string.Empty;


            if (string.IsNullOrWhiteSpace(
                    question.QuestionText))
            {
                throw new InvalidOperationException(
                    "AI returned an empty question text.");
            }


            if (!ContainsArabic(
                    question.QuestionText))
            {
                throw new InvalidOperationException(
                    "AI returned a non-Arabic question.");
            }


            if (ContainsMetaInstructions(
                    question.QuestionText))
            {
                throw new InvalidOperationException(
                    "AI returned prompt instructions instead of a real question.");
            }


            if (expectedQuestionType ==
                "TrueFalse")
            {
                question.OptionA = "صح";
                question.OptionB = "خطأ";
                question.OptionC = string.Empty;
                question.OptionD = string.Empty;


                if (question.CorrectOption != "A" &&
                    question.CorrectOption != "B")
                {
                    throw new InvalidOperationException(
                        "AI returned an invalid TrueFalse correct option.");
                }


                return;
            }


            if (string.IsNullOrWhiteSpace(
                    question.OptionA) ||
                string.IsNullOrWhiteSpace(
                    question.OptionB) ||
                string.IsNullOrWhiteSpace(
                    question.OptionC) ||
                string.IsNullOrWhiteSpace(
                    question.OptionD))
            {
                throw new InvalidOperationException(
                    "AI returned incomplete multiple choice options.");
            }


            if (!ContainsArabic(question.OptionA) ||
                !ContainsArabic(question.OptionB) ||
                !ContainsArabic(question.OptionC) ||
                !ContainsArabic(question.OptionD))
            {
                throw new InvalidOperationException(
                    "AI returned non-Arabic answer choices.");
            }


            HashSet<string> uniqueOptions =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    NormalizeOption(
                        question.OptionA),

                    NormalizeOption(
                        question.OptionB),

                    NormalizeOption(
                        question.OptionC),

                    NormalizeOption(
                        question.OptionD)
                };


            if (uniqueOptions.Count != 4)
            {
                throw new InvalidOperationException(
                    "AI returned duplicate answer choices.");
            }


            if (question.CorrectOption != "A" &&
                question.CorrectOption != "B" &&
                question.CorrectOption != "C" &&
                question.CorrectOption != "D")
            {
                throw new InvalidOperationException(
                    "AI returned an invalid correct option.");
            }
        }


        private static bool ContainsMetaInstructions(
            string text)
        {
            string[] forbiddenTexts =
            {
                "نوع السؤال المطلوب",
                "TrueFalse",
                "MultipleChoice",
                "SingleChoice",
                "questionType",
                "correctOption",
                "optionA",
                "optionB",
                "optionC",
                "optionD",
                "أنشئ سؤال",
                "أرجع JSON"
            };


            foreach (string forbiddenText
                     in forbiddenTexts)
            {
                if (text.Contains(
                        forbiddenText,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }


            return false;
        }


        private static bool ContainsArabic(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }


            foreach (char character in text)
            {
                if ((character >= '\u0600' &&
                     character <= '\u06FF') ||

                    (character >= '\u0750' &&
                     character <= '\u077F') ||

                    (character >= '\u08A0' &&
                     character <= '\u08FF'))
                {
                    return true;
                }
            }


            return false;
        }


        private static string NormalizeOption(
            string option)
        {
            return option
                .Trim()
                .Replace(" ", string.Empty)
                .Replace(".", string.Empty)
                .Replace("،", string.Empty)
                .Replace(",", string.Empty);
        }
    }
}