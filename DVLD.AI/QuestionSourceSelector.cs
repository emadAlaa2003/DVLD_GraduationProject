using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DVLD.AI
{
    public class QuestionSourceCandidate
    {
        public TextChunk Chunk { get; set; }

        public double QualityScore { get; set; }

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
                        candidate.QualityScore)
                    .ToList();


            if (candidates.Count == 0)
            {
                throw new InvalidOperationException(
                    "No suitable chunks were found for question generation.");
            }


            // إزالة النصوص المتطابقة أو شبه المتطابقة
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
                result.IsSuitable = false;

                result.RejectionReason =
                    "Chunk is empty.";

                return result;
            }


            string text =
                chunk.Text.Trim();


            if (text.Length <
                MinimumTextLength)
            {
                result.IsSuitable = false;

                result.RejectionReason =
                    "Text is too short.";

                return result;
            }


            if (ContainsExcludedContent(
                    text))
            {
                result.IsSuitable = false;

                result.RejectionReason =
                    "Chunk contains index, references, URLs or metadata.";

                return result;
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
                result.IsSuitable = false;

                result.RejectionReason =
                    "Chunk does not contain enough readable text.";

                return result;
            }


            if (statistics.ArabicRatio <
                MinimumArabicRatio)
            {
                result.IsSuitable = false;

                result.RejectionReason =
                    "Chunk contains too little Arabic content.";

                return result;
            }


            if (statistics.NoiseRatio >
                MaximumNoiseRatio)
            {
                result.IsSuitable = false;

                result.RejectionReason =
                    "Chunk contains too much noisy text.";

                return result;
            }


            result.QualityScore =
                CalculateQualityScore(
                    text,
                    statistics);


            result.IsSuitable =
                result.QualityScore >= 45;


            if (!result.IsSuitable)
            {
                result.RejectionReason =
                    "Quality score is too low.";
            }


            return result;
        }


        // =====================================================
        // حساب Quality Score
        // =====================================================

        private static double CalculateQualityScore(
            string text,
            TextStatistics statistics)
        {
            double score = 0;


            // ---------------------------------------------
            // 1. طول النص
            // الحجم المتوسط أفضل لتوليد سؤال واضح
            // ---------------------------------------------

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


            // ---------------------------------------------
            // 2. وجود محتوى عربي حقيقي
            // ---------------------------------------------

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


            // ---------------------------------------------
            // 3. الجمل وعلامات الترقيم
            // وجود جمل واضحة يزيد احتمال صلاحية النص
            // ---------------------------------------------

            if (statistics.SentenceMarkerCount >= 3)
            {
                score += 15;
            }
            else if (statistics.SentenceMarkerCount >= 1)
            {
                score += 10;
            }


            // ---------------------------------------------
            // 4. عقوبة النص المشوه
            // ---------------------------------------------

            score -=
                statistics.NoiseRatio * 30;


            // ---------------------------------------------
            // 5. Arabic Presentation Forms
            // نسبة عالية قد تعني استخراج PDF مشوه
            // ---------------------------------------------

            score -=
                statistics.PresentationFormRatio * 25;


            // ---------------------------------------------
            // 6. عدد كبير جدًا من الأرقام غالبًا جدول
            // أو فهرس
            // ---------------------------------------------

            if (statistics.DigitRatio > 0.20)
            {
                score -= 15;
            }
            else if (statistics.DigitRatio > 0.10)
            {
                score -= 7;
            }


            // ---------------------------------------------
            // حصر النتيجة بين 0 و 100
            // ---------------------------------------------

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


            foreach (char character in text)
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
            // من كل منطقة نأخذ أفضل Chunk جودة.
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


                QuestionSourceCandidate bestCandidate =
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
                            candidate.QualityScore)
                        .FirstOrDefault();


                // إذا المنطقة لا تحتوي مرشح مناسب،
                // نسمح بنفس الصفحة ولكن Chunk مختلف.
                if (bestCandidate == null)
                {
                    bestCandidate =
                        candidates
                            .Where(candidate =>
                                candidate.Chunk.PageNumber >=
                                    startPage &&

                                candidate.Chunk.PageNumber <=
                                    endPage &&

                                !usedChunkIndexes.Contains(
                                    candidate.Chunk.ChunkIndex))
                            .OrderByDescending(candidate =>
                                candidate.QualityScore)
                            .FirstOrDefault();
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


            // إذا بعض المناطق لم تعطِ نتائج،
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
                else if (
                    candidate.QualityScore >
                    unique[key].QualityScore)
                {
                    unique[key] =
                        candidate;
                }
            }


            return unique
                .Values
                .OrderByDescending(candidate =>
                    candidate.QualityScore)
                .ToList();
        }


        private static string BuildDuplicateKey(
            string text)
        {
            StringBuilder builder =
                new StringBuilder();


            foreach (char character in text)
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
                        marker.ToLowerInvariant()))
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