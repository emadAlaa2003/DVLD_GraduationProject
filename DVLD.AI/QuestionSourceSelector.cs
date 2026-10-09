using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DVLD.AI
{
    public class QuestionSourceCandidate
    {
        public TextChunk Chunk { get; set; }

        // جودة النص من ناحية القراءة والاستخراج.
        public double QualityScore { get; set; }

        // قيمة المحتوى من ناحية قابليته لإنشاء سؤال مفيد.
        // هذا التقييم عام وغير مرتبط بكتاب أو كلمات مرورية محددة.
        public double ExamValueScore { get; set; }

        public double ArabicRatio { get; set; }

        public double NoiseRatio { get; set; }

        public bool IsSuitable { get; set; }

        public string RejectionReason { get; set; } =
            string.Empty;
    }


    public static class QuestionSourceSelector
    {
        private const int MinimumTextLength = 250;

        private const int MinimumLetterCount = 80;

        private const double MinimumArabicRatio = 0.10;

        private const double MaximumNoiseRatio = 0.45;

        // لا يكفي أن يكون النص نظيفاً؛ يجب أن يحتوي
        // على معلومة ذات قيمة تعليمية/اختبارية.
        private const double MinimumExamValueScore = 20;


        // =====================================================
        // الدالة الرئيسية
        // =====================================================

        public static List<QuestionSourceCandidate> SelectBestSources(
            IReadOnlyList<TextChunk> chunks,
            int totalPages,
            int requiredCount)
        {
            if (chunks == null)
            {
                throw new ArgumentNullException(
                    nameof(chunks));
            }


            if (requiredCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requiredCount));
            }


            if (totalPages <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(totalPages));
            }


            List<QuestionSourceCandidate> candidates =
                chunks
                    .Select(EvaluateChunk)
                    .Where(candidate =>
                        candidate.IsSuitable)
                    .OrderByDescending(candidate =>
                        candidate.ExamValueScore)
                    .ThenByDescending(candidate =>
                        candidate.QualityScore)
                    .ToList();


            if (candidates.Count == 0)
            {
                throw new InvalidOperationException(
                    "No suitable question-worthy chunks were found.");
            }


            // إزالة النصوص المتطابقة أو شبه المتطابقة.
            candidates =
                RemoveDuplicateCandidates(
                    candidates);


            if (requiredCount >
                candidates.Count)
            {
                throw new InvalidOperationException(
                    $"Only {candidates.Count} suitable chunks " +
                    $"are available, but {requiredCount} " +
                    $"questions were requested.");
            }


            return SelectDistributedCandidates(
                candidates,
                totalPages,
                requiredCount);
        }


        // =====================================================
        // تقييم Chunk واحد
        // =====================================================

        public static QuestionSourceCandidate EvaluateChunk(
            TextChunk chunk)
        {
            QuestionSourceCandidate result =
                new QuestionSourceCandidate
                {
                    Chunk = chunk
                };


            if (chunk == null ||
                string.IsNullOrWhiteSpace(
                    chunk.Text))
            {
                return Reject(
                    result,
                    "Chunk is empty.");
            }


            string text =
                chunk.Text.Trim();


            if (text.Length <
                MinimumTextLength)
            {
                return Reject(
                    result,
                    "Text is too short.");
            }


            if (ContainsExcludedContent(
                    text))
            {
                return Reject(
                    result,
                    "Chunk contains index, references, URLs or metadata.");
            }


            TextStatistics statistics =
                AnalyzeText(
                    text);


            result.ArabicRatio =
                statistics.ArabicRatio;

            result.NoiseRatio =
                statistics.NoiseRatio;


            if (statistics.LetterCount <
                MinimumLetterCount)
            {
                return Reject(
                    result,
                    "Chunk does not contain enough readable text.");
            }


            if (statistics.ArabicRatio <
                MinimumArabicRatio)
            {
                return Reject(
                    result,
                    "Chunk contains too little Arabic content.");
            }


            if (statistics.NoiseRatio >
                MaximumNoiseRatio)
            {
                return Reject(
                    result,
                    "Chunk contains too much noisy text.");
            }


            result.QualityScore =
                CalculateQualityScore(
                    text,
                    statistics);


            if (result.QualityScore < 45)
            {
                return Reject(
                    result,
                    "Text quality score is too low.");
            }


            // التقييم الجديد:
            // هل النص يحتوي قاعدة/شرط/تعريف/إجراء/رقم/حد/استثناء...
            // بدل اختيار أول نص نظيف في الكتاب فقط.
            result.ExamValueScore =
                CalculateExamValueScore(
                    text);


            if (result.ExamValueScore <
                MinimumExamValueScore)
            {
                return Reject(
                    result,
                    "Chunk is readable but does not contain enough question-worthy information.");
            }


            result.IsSuitable = true;

            return result;
        }


        private static QuestionSourceCandidate Reject(
            QuestionSourceCandidate candidate,
            string reason)
        {
            candidate.IsSuitable = false;
            candidate.RejectionReason = reason;

            return candidate;
        }


        // =====================================================
        // تقييم القيمة الاختبارية بشكل عام
        // =====================================================
        //
        // ملاحظة:
        // لا توجد هنا كلمات خاصة بكتاب المرور أو بصفحة محددة.
        // نعتمد على أنماط لغوية عامة تظهر في الكتب التعليمية،
        // اللوائح، الأدلة، السياسات والإجراءات:
        //
        // - متطلب / إلزام / منع
        // - شرط
        // - تعريف
        // - إجراء أو تسلسل
        // - رقم أو حد أدنى/أقصى
        // - استثناء أو سبب/نتيجة
        // =====================================================

        private static double CalculateExamValueScore(
            string text)
        {
            string normalized =
                NormalizeForMatching(
                    text);


            double score = 0;


            // 1) قواعد ومتطلبات ومنع.
            int requirementMatches =
                CountMarkerGroups(
                    normalized,
                    new[]
                    {
                        "يجب",
                        "يتعين",
                        "يلزم",
                        "يشترط",
                        "لا يجوز",
                        "لا يسمح",
                        "يمنع",
                        "يحظر",
                        "ممنوع",
                        "ينبغي",
                        "من الضروري",
                        "must",
                        "shall",
                        "required",
                        "prohibited",
                        "must not"
                    });

            score +=
                Math.Min(
                    requirementMatches * 14,
                    28);


            // 2) شروط وحالات تطبيق.
            int conditionMatches =
                CountMarkerGroups(
                    normalized,
                    new[]
                    {
                        "اذا",
                        "في حال",
                        "عندما",
                        "عند ",
                        "بشرط",
                        "ما لم",
                        "في حالة",
                        "if ",
                        "when ",
                        "unless",
                        "provided that"
                    });

            score +=
                Math.Min(
                    conditionMatches * 10,
                    20);


            // 3) تعريفات واضحة.
            int definitionMatches =
                CountMarkerGroups(
                    normalized,
                    new[]
                    {
                        "يقصد ب",
                        "يعني",
                        "تعني",
                        "يعرف ب",
                        "تعرف ب",
                        "عبارة عن",
                        "refers to",
                        "means ",
                        "defined as"
                    });

            score +=
                Math.Min(
                    definitionMatches * 14,
                    24);


            // 4) إجراءات أو تسلسل خطوات.
            int procedureMatches =
                CountMarkerGroups(
                    normalized,
                    new[]
                    {
                        "اولا",
                        "ثانيا",
                        "ثالثا",
                        "ثم ",
                        "بعد ذلك",
                        "قبل ",
                        "بعد ",
                        "يتم ",
                        "الخطوة",
                        "الخطوات",
                        "first",
                        "second",
                        "then ",
                        "before ",
                        "after ",
                        "step "
                    });

            score +=
                Math.Min(
                    procedureMatches * 7,
                    21);


            // 5) حدود أو مقارنات كمية.
            int thresholdMatches =
                CountMarkerGroups(
                    normalized,
                    new[]
                    {
                        "على الاقل",
                        "على الاكثر",
                        "لا يقل",
                        "لا تزيد",
                        "لا يزيد",
                        "اقل من",
                        "اكثر من",
                        "حد ادنى",
                        "حد اقصى",
                        "minimum",
                        "maximum",
                        "at least",
                        "no more than",
                        "less than",
                        "more than"
                    });

            score +=
                Math.Min(
                    thresholdMatches * 12,
                    24);


            // 6) وجود رقم داخل نص طبيعي غالباً يعطي
            // معلومة أكثر تحديداً وقابلية للسؤال.
            bool containsNumber =
                Regex.IsMatch(
                    text,
                    @"\p{Nd}");

            if (containsNumber)
            {
                score += 10;
            }


            // 7) استثناءات / سبب ونتيجة.
            int reasoningMatches =
                CountMarkerGroups(
                    normalized,
                    new[]
                    {
                        "باستثناء",
                        "الا اذا",
                        "عدا",
                        "بسبب",
                        "لان",
                        "لذلك",
                        "نتيجة",
                        "except",
                        "because",
                        "therefore",
                        "as a result"
                    });

            score +=
                Math.Min(
                    reasoningMatches * 8,
                    16);


            // 8) كثافة الجمل المفيدة:
            // لا نسمح لنص من عناوين قصيرة فقط أن ينافس
            // فقرة تعليمية حقيقية.
            int informativeSentenceCount =
                CountInformativeSentences(
                    text);

            score +=
                Math.Min(
                    informativeSentenceCount * 3,
                    12);


            return Math.Clamp(
                score,
                0,
                100);
        }


        private static int CountMarkerGroups(
            string normalizedText,
            IEnumerable<string> markers)
        {
            int count = 0;


            foreach (string marker
                     in markers)
            {
                string normalizedMarker =
                    NormalizeForMatching(
                        marker);


                if (normalizedText.Contains(
                        normalizedMarker,
                        StringComparison.Ordinal))
                {
                    count++;
                }
            }


            return count;
        }


        private static int CountInformativeSentences(
            string text)
        {
            string[] parts =
                Regex.Split(
                    text,
                    @"[\.!\?؟؛:\r\n]+");


            int count = 0;


            foreach (string part
                     in parts)
            {
                string sentence =
                    part.Trim();


                if (sentence.Length < 35)
                {
                    continue;
                }


                int wordCount =
                    Regex.Matches(
                        sentence,
                        @"[\p{L}\p{Nd}]+")
                        .Count;


                if (wordCount >= 7)
                {
                    count++;
                }
            }


            return count;
        }


        private static string NormalizeForMatching(
            string text)
        {
            if (string.IsNullOrWhiteSpace(
                    text))
            {
                return string.Empty;
            }


            string normalized =
                text
                    .Normalize(
                        NormalizationForm.FormKC)
                    .ToLowerInvariant()
                    .Replace("أ", "ا")
                    .Replace("إ", "ا")
                    .Replace("آ", "ا")
                    .Replace("ى", "ي")
                    .Replace("ؤ", "و")
                    .Replace("ئ", "ي")
                    .Replace("ـ", "");


            normalized =
                Regex.Replace(
                    normalized,
                    @"\s+",
                    " ");


            return normalized.Trim();
        }


        // =====================================================
        // حساب جودة النص
        // =====================================================

        private static double CalculateQualityScore(
            string text,
            TextStatistics statistics)
        {
            double score = 0;


            // 1. طول النص
            if (text.Length >= 450 &&
                text.Length <= 1500)
            {
                score += 25;
            }
            else if (text.Length >= 300 &&
                     text.Length <= 2000)
            {
                score += 18;
            }
            else
            {
                score += 10;
            }


            // 2. وجود محتوى عربي حقيقي
            if (statistics.ArabicRatio >= 0.50)
            {
                score += 30;
            }
            else if (statistics.ArabicRatio >= 0.30)
            {
                score += 24;
            }
            else if (statistics.ArabicRatio >= 0.20)
            {
                score += 18;
            }
            else
            {
                score += 10;
            }


            // 3. وجود جمل واضحة
            if (statistics.SentenceMarkerCount >= 3)
            {
                score += 15;
            }
            else if (statistics.SentenceMarkerCount >= 1)
            {
                score += 10;
            }


            // 4. عقوبة النص المشوه
            score -=
                statistics.NoiseRatio * 30;


            // 5. Arabic Presentation Forms
            score -=
                statistics.PresentationFormRatio * 25;


            // 6. عدد كبير جداً من الأرقام غالباً جدول أو فهرس.
            if (statistics.DigitRatio > 0.20)
            {
                score -= 15;
            }
            else if (statistics.DigitRatio > 0.10)
            {
                score -= 7;
            }


            return Math.Clamp(
                score,
                0,
                100);
        }


        // =====================================================
        // تحليل النص
        // =====================================================

        private static TextStatistics AnalyzeText(
            string text)
        {
            int letterCount = 0;
            int arabicLetterCount = 0;
            int digitCount = 0;
            int noiseCount = 0;
            int presentationFormCount = 0;
            int sentenceMarkerCount = 0;


            foreach (char character
                     in text)
            {
                if (char.IsLetter(character))
                {
                    letterCount++;


                    if (IsArabicLetter(
                            character))
                    {
                        arabicLetterCount++;
                    }


                    if (IsArabicPresentationForm(
                            character))
                    {
                        presentationFormCount++;
                    }
                }


                if (char.IsDigit(character))
                {
                    digitCount++;
                }


                if (IsSentenceMarker(
                        character))
                {
                    sentenceMarkerCount++;
                }


                if (!char.IsLetterOrDigit(character) &&
                    !char.IsWhiteSpace(character) &&
                    !IsNormalPunctuation(character))
                {
                    noiseCount++;
                }
            }


            int safeLength =
                Math.Max(
                    text.Length,
                    1);


            int safeLetterCount =
                Math.Max(
                    letterCount,
                    1);


            return new TextStatistics
            {
                LetterCount =
                    letterCount,

                ArabicRatio =
                    (double)arabicLetterCount /
                    safeLetterCount,

                DigitRatio =
                    (double)digitCount /
                    safeLength,

                NoiseRatio =
                    (double)noiseCount /
                    safeLength,

                PresentationFormRatio =
                    (double)presentationFormCount /
                    safeLetterCount,

                SentenceMarkerCount =
                    sentenceMarkerCount
            };
        }


        // =====================================================
        // توزيع المصادر على كامل الكتاب
        // =====================================================

        private static List<QuestionSourceCandidate>
            SelectDistributedCandidates(
                List<QuestionSourceCandidate> candidates,
                int totalPages,
                int requiredCount)
        {
            List<QuestionSourceCandidate> selected =
                new List<QuestionSourceCandidate>();


            HashSet<int> usedChunkIndexes =
                new HashSet<int>();


            HashSet<int> usedPages =
                new HashSet<int>();


            // نقسم الكتاب إلى مناطق حسب عدد الأسئلة المطلوب.
            // من كل منطقة نأخذ المرشح الأعلى قيمة اختبارية،
            // ثم الأعلى جودة نصية.
            for (int sectionIndex = 0;
                 sectionIndex < requiredCount;
                 sectionIndex++)
            {
                double startRatio =
                    (double)sectionIndex /
                    requiredCount;

                double endRatio =
                    (double)(sectionIndex + 1) /
                    requiredCount;


                int startPage =
                    Math.Max(
                        1,
                        (int)Math.Floor(
                            totalPages *
                            startRatio) + 1);


                int endPage =
                    Math.Min(
                        totalPages,
                        (int)Math.Ceiling(
                            totalPages *
                            endRatio));


                List<QuestionSourceCandidate> sectionCandidates =
                    candidates
                        .Where(candidate =>
                            candidate.Chunk.PageNumber >=
                                startPage &&

                            candidate.Chunk.PageNumber <=
                                endPage &&

                            !usedChunkIndexes.Contains(
                                candidate.Chunk.ChunkIndex) &&

                            !usedPages.Contains(
                                candidate.Chunk.PageNumber))
                        .OrderByDescending(candidate =>
                            candidate.ExamValueScore)
                        .ThenByDescending(candidate =>
                            candidate.QualityScore)
                        .ToList();


                QuestionSourceCandidate bestCandidate =
                    SelectWeightedCandidate(
                        sectionCandidates);


                // إذا المنطقة لا تحتوي مرشحاً مناسباً،
                // نسمح بنفس الصفحة ولكن Chunk مختلف.
                if (bestCandidate == null)
                {
                    sectionCandidates =
                        candidates
                            .Where(candidate =>
                                candidate.Chunk.PageNumber >=
                                    startPage &&

                                candidate.Chunk.PageNumber <=
                                    endPage &&

                                !usedChunkIndexes.Contains(
                                    candidate.Chunk.ChunkIndex))
                            .OrderByDescending(candidate =>
                                candidate.ExamValueScore)
                            .ThenByDescending(candidate =>
                                candidate.QualityScore)
                            .ToList();


                    bestCandidate =
                        SelectWeightedCandidate(
                            sectionCandidates);
                }


                if (bestCandidate != null)
                {
                    selected.Add(
                        bestCandidate);


                    usedChunkIndexes.Add(
                        bestCandidate
                            .Chunk
                            .ChunkIndex);


                    usedPages.Add(
                        bestCandidate
                            .Chunk
                            .PageNumber);
                }
            }


            // إذا بعض المناطق لم تعط نتائج،
            // نكمل من أفضل المصادر المتبقية.
            if (selected.Count <
                requiredCount)
            {
                IEnumerable<QuestionSourceCandidate> remaining =
                    candidates
                        .Where(candidate =>
                            !usedChunkIndexes.Contains(
                                candidate
                                    .Chunk
                                    .ChunkIndex))
                        .OrderByDescending(candidate =>
                            candidate.ExamValueScore)
                        .ThenByDescending(candidate =>
                            candidate.QualityScore);


                foreach (QuestionSourceCandidate candidate
                         in remaining)
                {
                    if (selected.Count >=
                        requiredCount)
                    {
                        break;
                    }


                    selected.Add(
                        candidate);


                    usedChunkIndexes.Add(
                        candidate
                            .Chunk
                            .ChunkIndex);
                }
            }


            return selected
                .OrderBy(candidate =>
                    candidate.Chunk.PageNumber)
                .ToList();
        }


        // =====================================================
        // اختيار متنوع من أفضل المصادر
        // =====================================================
        //
        // لا نختار Chunk عشوائياً من كامل الكتاب.
        // نأخذ فقط أفضل مجموعة من المرشحين ثم نستخدم
        // Weighted Random بحيث تبقى الجودة هي العامل الأكبر،
        // مع تغيير المصدر بين الطلبات لتقليل التكرار.
        // =====================================================

        private static QuestionSourceCandidate
            SelectWeightedCandidate(
                List<QuestionSourceCandidate> orderedCandidates)
        {
            if (orderedCandidates == null ||
                orderedCandidates.Count == 0)
            {
                return null;
            }


            const int topPoolSize = 12;


            List<QuestionSourceCandidate> pool =
                orderedCandidates
                    .Take(
                        Math.Min(
                            topPoolSize,
                            orderedCandidates.Count))
                    .ToList();


            double bestExamValue =
                pool.Max(candidate =>
                    candidate.ExamValueScore);


            // لا نسمح للعشوائية أن تسحب مصدراً أضعف بكثير.
            // نبقي فقط المرشحين القريبين من أعلى قيمة اختبارية.
            pool =
                pool
                    .Where(candidate =>
                        candidate.ExamValueScore >=
                            bestExamValue - 12)
                    .ToList();


            if (pool.Count == 1)
            {
                return pool[0];
            }


            double totalWeight =
                pool.Sum(candidate =>
                    Math.Max(
                        1,
                        (candidate.ExamValueScore * 2.0) +
                        candidate.QualityScore));


            double target =
                Random.Shared.NextDouble() *
                totalWeight;


            double accumulated =
                0;


            foreach (QuestionSourceCandidate candidate
                     in pool)
            {
                accumulated +=
                    Math.Max(
                        1,
                        (candidate.ExamValueScore * 2.0) +
                        candidate.QualityScore);


                if (target <= accumulated)
                {
                    return candidate;
                }
            }


            return pool.Last();
        }


        // =====================================================
        // إزالة النصوص المكررة
        // =====================================================

        private static List<QuestionSourceCandidate>
            RemoveDuplicateCandidates(
                List<QuestionSourceCandidate> candidates)
        {
            Dictionary<string, QuestionSourceCandidate> unique =
                new Dictionary<string, QuestionSourceCandidate>();


            foreach (QuestionSourceCandidate candidate
                     in candidates)
            {
                string key =
                    BuildDuplicateKey(
                        candidate.Chunk.Text);


                if (!unique.ContainsKey(key))
                {
                    unique.Add(
                        key,
                        candidate);
                }
                else
                {
                    QuestionSourceCandidate existing =
                        unique[key];


                    bool candidateIsBetter =
                        candidate.ExamValueScore >
                        existing.ExamValueScore ||

                        (
                            candidate.ExamValueScore ==
                            existing.ExamValueScore &&

                            candidate.QualityScore >
                            existing.QualityScore
                        );


                    if (candidateIsBetter)
                    {
                        unique[key] =
                            candidate;
                    }
                }
            }


            return unique
                .Values
                .OrderByDescending(candidate =>
                    candidate.ExamValueScore)
                .ThenByDescending(candidate =>
                    candidate.QualityScore)
                .ToList();
        }


        private static string BuildDuplicateKey(
            string text)
        {
            StringBuilder builder =
                new StringBuilder();


            foreach (char character
                     in text)
            {
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(
                        char.ToLowerInvariant(
                            character));
                }


                if (builder.Length >= 250)
                {
                    break;
                }
            }


            return builder.ToString();
        }


        // =====================================================
        // Filters
        // =====================================================

        private static bool ContainsExcludedContent(
            string text)
        {
            string lowered =
                text.ToLowerInvariant();


            // فقط أنواع محتوى عامة لا تصلح عادةً لتوليد أسئلة
            // ولا توجد هنا استثناءات مبنية على الكتاب الحالي.
            string[] excludedMarkers =
            {
                "table of contents",
                "bibliography",
                "references",
                "reference list",
                "acknowledgements",
                "http://",
                "https://",
                "www.",
                "isbn",
                "المحتويات",
                "فهرس المحتويات",
                "الفهرس",
                "المراجع",
                "المصادر والمراجع"
            };


            foreach (string marker
                     in excludedMarkers)
            {
                if (lowered.Contains(
                        marker.ToLowerInvariant(),
                        StringComparison.Ordinal))
                {
                    return true;
                }
            }


            return false;
        }


        private static bool IsArabicLetter(
            char character)
        {
            return
                (character >= '\u0600' &&
                 character <= '\u06FF') ||

                (character >= '\u0750' &&
                 character <= '\u077F') ||

                (character >= '\u08A0' &&
                 character <= '\u08FF') ||

                IsArabicPresentationForm(
                    character);
        }


        private static bool IsArabicPresentationForm(
            char character)
        {
            return
                (character >= '\uFB50' &&
                 character <= '\uFDFF') ||

                (character >= '\uFE70' &&
                 character <= '\uFEFF');
        }


        private static bool IsSentenceMarker(
            char character)
        {
            return
                character == '.' ||
                character == '!' ||
                character == '?' ||
                character == '؟' ||
                character == '؛' ||
                character == ':';
        }


        private static bool IsNormalPunctuation(
            char character)
        {
            return
                character == '.' ||
                character == ',' ||
                character == '،' ||
                character == ';' ||
                character == '؛' ||
                character == ':' ||
                character == '?' ||
                character == '؟' ||
                character == '!' ||
                character == '-' ||
                character == '_' ||
                character == '(' ||
                character == ')' ||
                character == '[' ||
                character == ']' ||
                character == '{' ||
                character == '}' ||
                character == '/' ||
                character == '\\' ||
                character == '"' ||
                character == '\'' ||
                character == '%' ||
                character == '+';
        }


        private class TextStatistics
        {
            public int LetterCount { get; set; }

            public double ArabicRatio { get; set; }

            public double DigitRatio { get; set; }

            public double NoiseRatio { get; set; }

            public double PresentationFormRatio { get; set; }

            public int SentenceMarkerCount { get; set; }
        }
    }
}
