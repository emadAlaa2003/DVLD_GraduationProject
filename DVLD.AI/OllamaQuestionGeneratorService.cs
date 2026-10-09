using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

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

        // Ollama يرجع رقم مقطع الدليل فقط.
        // التطبيق نفسه ينسخ النص الأصلي حرفياً من المصدر.
        public int SourceEvidenceIndex { get; set; } = -1;

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


            List<string> evidenceSegments =
                BuildEvidenceSegments(
                    sourceText);


            if (evidenceSegments.Count == 0)
            {
                throw new InvalidOperationException(
                    "Source chunk does not contain a complete evidence segment suitable for question generation.");
            }


            string numberedSource =
                BuildNumberedSource(
                    evidenceSegments);


            string systemPrompt =
                BuildSystemPrompt(
                    questionType);


            string userPrompt =
                $"""
                هذا هو النص المصدر الوحيد المسموح لك بالاعتماد عليه.
                تم تقسيمه إلى مقاطع دليل مرقمة.

                --- بداية المصدر ---

                {numberedSource}

                --- نهاية المصدر ---

                أنشئ سؤالاً امتحانياً واحداً فقط من حقيقة أو قاعدة
                أو إجراء أو رقم أو تعليمات واضحة ومحددة في أحد المقاطع.

                مهم جداً:
                - اختر مقطع دليل واحداً يثبت الإجابة مباشرة.
                - لا تنسخ نص الدليل بنفسك.
                - أرجع فقط رقم المقطع في SourceEvidenceIndex.
                - يجب أن يكون SourceEvidenceIndex رقماً صحيحاً موجوداً
                  بين المقاطع المعروضة أعلاه.
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
                    num_predict = 320,
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


            if (question == null)
            {
                throw new InvalidOperationException(
                    "Ollama returned an invalid question.");
            }


            ResolveSourceEvidence(
                question,
                evidenceSegments);


            ValidateGeneratedQuestion(
                question,
                questionType);


            question.Explanation =
                BuildDeterministicExplanation(
                    question);


            return question;
        }


        // =====================================================
        // الدليل: Ollama يختار رقم المقطع فقط
        // والتطبيق ينسخ المقطع الأصلي حرفياً
        // =====================================================

        private static List<string> BuildEvidenceSegments(
            string sourceText)
        {
            List<string> segments =
                new List<string>();


            // نحمي النقطة الواقعة بين رقمين قبل تقسيم الجمل.
            // مثال: 6.0 أو 3.5 يجب أن تبقى داخل نفس المقطع
            // وألا تُعامل كنهاية جملة.
            const string numericDotToken =
                "__DVLD_NUMERIC_DOT__";


            string protectedSource =
                Regex.Replace(
                    sourceText,
                    @"(?<=\p{Nd})\.(?=\p{Nd})",
                    numericDotToken);


            MatchCollection matches =
                Regex.Matches(
                    protectedSource,
                    @"[^\.!\?؟؛\r\n]+[\.!\?؟؛]?");


            foreach (Match match
                     in matches)
            {
                string segment =
                    match.Value
                        .Replace(
                            numericDotToken,
                            ".")
                        .Trim();


                if (!IsSuitableEvidenceSegment(
                        segment))
                {
                    continue;
                }


                segments.Add(
                    segment);
            }


            return segments;
        }


        private static bool IsSuitableEvidenceSegment(
            string segment)
        {
            if (string.IsNullOrWhiteSpace(segment) ||
                segment.Length < 25)
            {
                return false;
            }


            MatchCollection arabicWords =
                Regex.Matches(
                    segment,
                    @"[\u0600-\u06FF]+");


            if (arabicWords.Count < 5)
            {
                return false;
            }


            // لا نسمح بمقطع ينتهي بمقدمة لقائمة مثل:
            // "يجب مراعاة الأمور التالية:"
            // حتى لو أضاف استخراج الـPDF نقطة أو فاصل بعدها.
            // هذا فحص عام وليس مرتبطاً بمحتوى كتاب معين.
            string normalizedEnding =
                Regex.Replace(
                    segment.Trim(),
                    @"\s+",
                    " ");


            if (Regex.IsMatch(
                    normalizedEnding,
                    @"[:：]\s*[\.\،,؛;!\?؟]*$"))
            {
                return false;
            }


            string lastArabicWord =
                arabicWords
                    .Cast<Match>()
                    .Last()
                    .Value;


            string[] invalidEndingWords =
            {
                "ال",
                "و",
                "في",
                "من",
                "إلى",
                "الى",
                "على",
                "عن",
                "أو",
                "او",
                "أن",
                "ان",
                "مع"
            };


            if (invalidEndingWords.Contains(
                    lastArabicWord))
            {
                return false;
            }


            return true;
        }


        private static string BuildNumberedSource(
            IReadOnlyList<string> evidenceSegments)
        {
            StringBuilder builder =
                new StringBuilder();


            for (int i = 0;
                 i < evidenceSegments.Count;
                 i++)
            {
                builder.Append("[E");
                builder.Append(i);
                builder.Append("] ");
                builder.AppendLine(
                    evidenceSegments[i]);
            }


            return builder.ToString();
        }


        private static void ResolveSourceEvidence(
            GeneratedQuestion question,
            IReadOnlyList<string> evidenceSegments)
        {
            if (question.SourceEvidenceIndex < 0 ||
                question.SourceEvidenceIndex >=
                    evidenceSegments.Count)
            {
                throw new InvalidOperationException(
                    $"AI returned an invalid SourceEvidenceIndex. " +
                    $"Index=[{question.SourceEvidenceIndex}], " +
                    $"AvailableSegments=[{evidenceSegments.Count}].");
            }


            question.SourceEvidence =
                evidenceSegments[
                    question.SourceEvidenceIndex];
        }


        private static string BuildSystemPrompt(
            string questionType)
        {
            if (questionType == "TrueFalse")
            {
                return
                    """
                    أنت مولد أسئلة احترافي باللغة العربية.

                    أنشئ عبارة صح أو خطأ واحدة فقط من المصدر.

                    الشروط الإلزامية:

                    1. استخدم فقط معلومة واضحة ومباشرة من أحد مقاطع المصدر.
                    2. اجعل العبارة محددة وليست عامة أو فضفاضة.
                    3. QuestionText يجب أن يكون عبارة تقريرية يمكن الحكم
                       عليها بصح أو خطأ، وليس سؤالاً استفهامياً.
                    4. لا تبدأ QuestionText بكلمات مثل:
                       هل، ما، ماذا، كيف، لماذا، متى، أين، من.
                    5. لا تنه QuestionText بعلامة استفهام.
                    6. يجب أن تكون العربية طبيعية وسليمة.
                    7. لا تستخدم معرفة خارج المصدر.
                    8. OptionA = "صح".
                    9. OptionB = "خطأ".
                    10. OptionC و OptionD فارغان.
                    11. CorrectOption يجب أن تكون A أو B فقط.
                    12. SourceEvidenceIndex يجب أن يكون رقم مقطع واحد
                        من المقاطع [E0] [E1] ... يثبت الإجابة مباشرة.
                    13. لا تكتب Explanation؛ التطبيق سيبنيه من الدليل.
                    14. لا تكتب SourceEvidence بنفسك.
                    15. راجع QuestionText قبل الإرجاع:
                        يجب أن تكون عبارة تقريرية وليست سؤالاً.
                    16. لا تذكر أرقام المقاطع مثل [E0] أو [E1]
                        داخل QuestionText.
                    17. لا تكتب Markdown.
                    18. أرجع JSON فقط.

                    الشكل المطلوب:

                    {
                      "questionText": "",
                      "questionType": "TrueFalse",
                      "optionA": "صح",
                      "optionB": "خطأ",
                      "optionC": "",
                      "optionD": "",
                      "correctOption": "",
                      "sourceEvidenceIndex": 0
                    }
                    """;
            }


            return
                """
                أنت مولد أسئلة احترافي باللغة العربية.

                أنشئ سؤال اختيار من متعدد واحداً فقط من المصدر.

                الشروط الإلزامية:

                1. استخدم حقيقة أو قاعدة أو إجراء أو رقم أو تعليمات
                   واضحة ومحددة من أحد مقاطع المصدر.
                2. السؤال يجب أن يكون واضحاً ومستقلاً ومفيداً.
                3. لا تنشئ سؤالاً عاماً إذا كان بالإمكان إنشاء
                   سؤال أكثر تحديداً.
                4. يجب أن تكون العربية طبيعية وسليمة.
                5. أنشئ أربعة خيارات A و B و C و D.
                6. يجب أن تكون الخيارات الأربعة مختلفة وواضحة.
                7. يجب أن تكون الخيارات من نفس النوع اللغوي والمنطقي.
                8. كل خيار يجب أن يكون بالعربية ولا يحتوي حروفاً
                   من لغة أخرى.
                9. يوجد جواب صحيح واحد فقط.
                10. CorrectOption يجب أن تكون A أو B أو C أو D فقط.
                11. الإجابة الصحيحة يجب أن تكون مثبتة مباشرة بالمصدر.
                12. الخيارات الخاطئة يجب أن تكون معقولة لغوياً،
                    لكنها غير صحيحة حسب المعلومة المحددة.
                13. SourceEvidenceIndex يجب أن يكون رقم مقطع واحد
                    من المقاطع [E0] [E1] ... يثبت الإجابة مباشرة.
                14. لا تكتب Explanation؛ التطبيق سيبنيه من الدليل.
                15. لا تكتب SourceEvidence بنفسك.
                16. لا تذكر أرقام المقاطع مثل [E0] أو [E1]
                    داخل QuestionText.
                17. لا تستخدم معرفة خارج المصدر.
                18. لا تكتب Markdown.
                19. أرجع JSON فقط.

                الشكل المطلوب:

                {
                  "questionText": "",
                  "questionType": "MultipleChoice",
                  "optionA": "",
                  "optionB": "",
                  "optionC": "",
                  "optionD": "",
                  "correctOption": "",
                  "sourceEvidenceIndex": 0
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
                SanitizeExplanation(
                    question.Explanation);

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
                    "AI did not resolve sufficient source evidence.");
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


        private static string BuildDeterministicExplanation(
            GeneratedQuestion question)
        {
            string evidence =
                question.SourceEvidence?.Trim() ??
                string.Empty;


            if (question.QuestionType == "TrueFalse")
            {
                string answerText =
                    question.CorrectOption == "A"
                        ? "صح"
                        : "خطأ";


                return
                    $"الإجابة الصحيحة هي {answerText}. " +
                    $"الدليل: {evidence}";
            }


            string correctAnswerText =
                question.CorrectOption switch
                {
                    "A" => question.OptionA,
                    "B" => question.OptionB,
                    "C" => question.OptionC,
                    "D" => question.OptionD,
                    _ => string.Empty
                };


            return
                $"الإجابة الصحيحة هي {correctAnswerText}. " +
                $"الدليل: {evidence}";
        }


        private static string SanitizeExplanation(
            string explanation)
        {
            if (string.IsNullOrWhiteSpace(
                    explanation))
            {
                return string.Empty;
            }


            string cleaned =
                explanation.Trim();


            // إزالة أي مراجع داخلية لمقاطع الدليل مثل [E7].
            cleaned =
                Regex.Replace(
                    cleaned,
                    @"\[(?:E|e)\d+\]",
                    string.Empty);


            // إذا صاغ النموذج جملة تشير إلى رقم المقطع الداخلي،
            // نحذف العبارة التقنية نفسها حتى لا تظهر للمستخدم.
            cleaned =
                Regex.Replace(
                    cleaned,
                    @"\s*(?:كما\s+)?(?:يوضح|يوضّح|ورد\s+في|مذكور\s+في)?\s*(?:المصدر|مصدر|المقطع|مقطع)\s*(?:رقم)?\s*(?=[\.\،\,؛;:]|$)",
                    string.Empty,
                    RegexOptions.IgnoreCase);


            cleaned =
                Regex.Replace(
                    cleaned,
                    @"\s+",
                    " ");


            cleaned =
                Regex.Replace(
                    cleaned,
                    @"\s+([\.\،\,؛;:])",
                    "$1");


            cleaned =
                cleaned.Trim();


            return cleaned;
        }


        private static bool ContainsMetaInstructions(
            string text)
        {
            if (Regex.IsMatch(
                    text,
                    @"\[(?:E|e)\d+\]"))
            {
                return true;
            }


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
                "SourceEvidenceIndex",
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
