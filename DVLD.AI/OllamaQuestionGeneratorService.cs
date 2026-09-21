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

        // نص قصير من المصدر نفسه، منسوخ حرفياً،
        // يثبت المعلومة التي بُني عليها السؤال.
        public string SourceEvidence { get; set; } =
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

                أنشئ سؤالاً امتحانياً واحداً فقط من حقيقة أو قاعدة
                أو إجراء أو رقم أو تعليمات واضحة ومحددة في المصدر.

                مهم جداً:
                - SourceEvidence يجب أن يكون مقتطفاً قصيراً من النص
                  المصدر نفسه، منسوخاً حرفياً دون إعادة صياغة.
                - SourceEvidence يجب أن يحتوي على 5 كلمات عربية على الأقل،
                  وأن يكون عبارة مكتملة تحمل المعلومة نفسها، وليس مجرد
                  عنوان قسم أو عنوان فرعي أو نصاً مقطوعاً.
                - لا تنشئ سؤالاً عاماً إذا كان المصدر يحتوي معلومة
                  أكثر تحديداً.
                - لا تستخدم معرفة خارج المصدر.
                - لا تذكر "النص" أو "المصدر" داخل السؤال.
                - لا تكرر التعليمات.
                - أرجع JSON فقط.
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
                    num_predict = 650,
                    temperature = 0.2
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
                    أنت مولد أسئلة احترافي لامتحان قيادة باللغة العربية.

                    أنشئ سؤال صح أو خطأ واحداً فقط من النص المصدر.

                    الشروط الإلزامية:

                    1. استخدم فقط معلومة واضحة ومباشرة في المصدر.
                    2. اجعل العبارة محددة وليست عامة أو فضفاضة.
                    3. QuestionText يجب أن يكون عبارة تقريرية يمكن الحكم
                       عليها بصح أو خطأ، وليس سؤالاً استفهامياً.
                    4. لا تبدأ QuestionText بكلمات مثل:
                       هل، ما، ماذا، كيف، لماذا، متى، أين، من.
                    5. لا تنه QuestionText بعلامة استفهام.
                    6. يجب أن تكون العربية طبيعية وسليمة.
                    7. لا تستخدم معرفة خارج المصدر.
                    8. لا تذكر "المصدر" أو "النص" داخل السؤال.
                    9. OptionA = "صح".
                    10. OptionB = "خطأ".
                    11. OptionC و OptionD فارغان.
                    12. CorrectOption يجب أن تكون A أو B فقط.
                    13. Explanation يشرح سبب الإجابة باختصار.
                    14. SourceEvidence يجب أن يكون مقتطفاً حرفياً
                        من المصدر يثبت الحقيقة الأساسية مباشرة.
                    15. SourceEvidence يجب أن يحتوي على 5 كلمات عربية
                        على الأقل وأن يكون عبارة مكتملة، وليس عنوان قسم
                        أو عنواناً فرعياً أو نصاً مقطوعاً.
                    16. لا تكتب Markdown.
                    17. أرجع JSON فقط.

                    الشكل المطلوب:

                    {
                      "questionText": "",
                      "questionType": "TrueFalse",
                      "optionA": "صح",
                      "optionB": "خطأ",
                      "optionC": "",
                      "optionD": "",
                      "correctOption": "",
                      "explanation": "",
                      "sourceEvidence": ""
                    }
                    """;
            }


            return
                """
                أنت مولد أسئلة احترافي لامتحان قيادة باللغة العربية.

                أنشئ سؤال اختيار من متعدد واحداً فقط من النص المصدر.

                الشروط الإلزامية:

                1. استخدم حقيقة أو قاعدة أو إجراء أو رقم أو تعليمات
                   واضحة ومحددة في المصدر.
                2. لا تنشئ سؤالاً عاماً مثل:
                   "ما الممارسة المرورية الآمنة؟"
                   إذا كان بالإمكان إنشاء سؤال أكثر تحديداً.
                3. السؤال يجب أن يكون واضحاً ومستقلاً ومفيداً للمتدرب.
                4. يجب أن تكون العربية طبيعية وسليمة.
                5. أنشئ أربعة خيارات A و B و C و D.
                6. يجب أن تكون الخيارات الأربعة مختلفة وواضحة.
                7. يجب أن تكون الخيارات من نفس النوع اللغوي والمنطقي.
                8. كل خيار يجب أن يكون بالعربية، ولا يحتوي حروفاً
                   لاتينية أو صينية أو أي حروف من لغة أخرى.
                9. امنع الخيارات الركيكة أو غير الطبيعية أو المتداخلة.
                10. يوجد جواب صحيح واحد فقط.
                11. CorrectOption يجب أن تكون A أو B أو C أو D فقط.
                12. الإجابة الصحيحة يجب أن تكون مثبتة مباشرة بالمصدر.
                13. الخيارات الخاطئة يجب أن تكون معقولة لغوياً،
                    لكنها غير صحيحة حسب المعلومة المحددة.
                14. Explanation يشرح الإجابة الصحيحة باختصار.
                15. SourceEvidence يجب أن يكون مقتطفاً حرفياً
                    من المصدر يثبت الإجابة الصحيحة مباشرة.
                16. SourceEvidence يجب أن يحتوي على 5 كلمات عربية
                    على الأقل وأن يكون عبارة مكتملة، وليس عنوان قسم
                    أو عنواناً فرعياً أو نصاً مقطوعاً.
                17. لا تستخدم معرفة خارج المصدر.
                18. لا تذكر "المصدر" أو "النص" داخل السؤال.
                19. لا تكتب Markdown.
                20. أرجع JSON فقط.

                الشكل المطلوب:

                {
                  "questionText": "",
                  "questionType": "MultipleChoice",
                  "optionA": "",
                  "optionB": "",
                  "optionC": "",
                  "optionD": "",
                  "correctOption": "",
                  "explanation": "",
                  "sourceEvidence": ""
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
                NormalizeCorrectOption(
                    question.CorrectOption);

            question.Explanation =
                question.Explanation?.Trim() ??
                string.Empty;

            question.SourceEvidence =
                question.SourceEvidence?.Trim() ??
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


            if (string.IsNullOrWhiteSpace(
                    question.SourceEvidence) ||
                question.SourceEvidence.Length < 15)
            {
                throw new InvalidOperationException(
                    "AI did not return sufficient source evidence.");
            }


            if (expectedQuestionType ==
                "TrueFalse")
            {
                if (LooksLikeQuestion(
                        question.QuestionText))
                {
                    throw new InvalidOperationException(
                        "AI returned a question instead of a TrueFalse statement.");
                }


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


            if (ContainsForeignLetters(question.OptionA) ||
                ContainsForeignLetters(question.OptionB) ||
                ContainsForeignLetters(question.OptionC) ||
                ContainsForeignLetters(question.OptionD))
            {
                throw new InvalidOperationException(
                    "AI returned answer choices containing non-Arabic letters.");
            }


            HashSet<string> uniqueOptions =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    NormalizeOption(question.OptionA),
                    NormalizeOption(question.OptionB),
                    NormalizeOption(question.OptionC),
                    NormalizeOption(question.OptionD)
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
                    $"AI returned an invalid correct option. " +
                    $"CorrectOption=[{question.CorrectOption}]");
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


        private static bool LooksLikeQuestion(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }


            string trimmed =
                text.Trim();


            if (trimmed.EndsWith("؟") ||
                trimmed.EndsWith("?"))
            {
                return true;
            }


            string[] questionStarters =
            {
                "هل ",
                "ما ",
                "ماذا ",
                "كيف ",
                "لماذا ",
                "متى ",
                "أين ",
                "اين ",
                "من ",
                "أي ",
                "اي "
            };


            foreach (string starter
                     in questionStarters)
            {
                if (trimmed.StartsWith(
                        starter,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }


            return false;
        }


        private static bool ContainsForeignLetters(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }


            foreach (char character
                     in text)
            {
                if (!char.IsLetter(character))
                {
                    continue;
                }


                bool isArabic =
                    (character >= '\u0600' &&
                     character <= '\u06FF') ||

                    (character >= '\u0750' &&
                     character <= '\u077F') ||

                    (character >= '\u08A0' &&
                     character <= '\u08FF');


                if (!isArabic)
                {
                    return true;
                }
            }


            return false;
        }


        private static string NormalizeCorrectOption(
            string correctOption)
        {
            if (string.IsNullOrWhiteSpace(correctOption))
            {
                return string.Empty;
            }


            string normalized =
                correctOption
                    .Trim()
                    .ToUpperInvariant()
                    .Replace(" ", string.Empty)
                    .Replace("_", string.Empty)
                    .Replace("-", string.Empty)
                    .Replace(":", string.Empty);


            if (normalized == "A" ||
                normalized == "B" ||
                normalized == "C" ||
                normalized == "D")
            {
                return normalized;
            }


            if (normalized.StartsWith(
                    "OPTION",
                    StringComparison.Ordinal))
            {
                string optionLetter =
                    normalized.Substring(
                        "OPTION".Length);


                if (optionLetter == "A" ||
                    optionLetter == "B" ||
                    optionLetter == "C" ||
                    optionLetter == "D")
                {
                    return optionLetter;
                }
            }


            return normalized;
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
