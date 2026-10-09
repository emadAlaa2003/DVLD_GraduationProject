using Qdrant.Client.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DVLD.AI
{
    public class RankedSearchResult
    {
        public ScoredPoint Point { get; set; } = null!;

        public float SemanticScore { get; set; }

        public float KeywordScore { get; set; }

        public float FinalScore { get; set; }
    }


    public static class KnowledgeSearchReranker
    {
        public static List<RankedSearchResult> Rerank(
            string question,
            IReadOnlyList<ScoredPoint> results,
            int take = 5)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                throw new ArgumentException(
                    "Question is required.",
                    nameof(question));
            }

            if (results == null)
            {
                throw new ArgumentNullException(
                    nameof(results));
            }


            List<string> queryWords =
                GetImportantWords(question);


            List<RankedSearchResult> rankedResults =
                new List<RankedSearchResult>();


            foreach (ScoredPoint result in results)
            {
                string text = string.Empty;

                if (result.Payload.ContainsKey("text"))
                {
                    text =
                        result.Payload["text"]
                            .StringValue ?? string.Empty;
                }


                float keywordScore =
                    CalculateKeywordScore(
                        queryWords,
                        text);


                float semanticScore =
                    result.Score;


                float finalScore =
                    (semanticScore * 0.70f) +
                    (keywordScore * 0.30f);


                rankedResults.Add(
                    new RankedSearchResult
                    {
                        Point =
                            result,

                        SemanticScore =
                            semanticScore,

                        KeywordScore =
                            keywordScore,

                        FinalScore =
                            finalScore
                    });
            }


            return rankedResults
                .OrderByDescending(
                    result => result.FinalScore)
                .Take(take)
                .ToList();
        }


        private static float CalculateKeywordScore(
            List<string> queryWords,
            string text)
        {
            if (queryWords.Count == 0 ||
                string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }


            string normalizedText =
                NormalizeArabic(text);


            HashSet<string> textWords =
                normalizedText
                    .Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries)
                    .Select(word =>
                        NormalizeWord(word))
                    .Where(word =>
                        !string.IsNullOrWhiteSpace(word))
                    .ToHashSet();


            int matchedWords = 0;


            foreach (string word in queryWords)
            {
                if (textWords.Contains(word))
                {
                    matchedWords++;
                }
            }


            return (float)matchedWords /
                   queryWords.Count;
        }


        private static List<string> GetImportantWords(
            string text)
        {
            string normalized =
                NormalizeArabic(text);


            HashSet<string> stopWords =
                new HashSet<string>
                {
                    "ما",
                    "ماذا",
                    "من",
                    "في",
                    "على",
                    "عن",
                    "الى",
                    "هل",
                    "هو",
                    "هي",
                    "هذا",
                    "هذه",
                    "ذلك",
                    "تلك",
                    "كيف",
                    "لماذا",
                    "متى",
                    "اين"
                };


            return normalized
                .Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries)

                .Select(word =>
                    NormalizeWord(word))

                .Where(word =>
                    word.Length >= 3 &&
                    !stopWords.Contains(word))

                .Distinct()
                .ToList();
        }


        private static string NormalizeArabic(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }


            string result =
                text.ToLowerInvariant();


            // إزالة التشكيل
            result =
                Regex.Replace(
                    result,
                    "[\u064B-\u065F\u0670]",
                    "");


            // توحيد بعض أشكال الحروف العربية
            result =
                result
                    .Replace('أ', 'ا')
                    .Replace('إ', 'ا')
                    .Replace('آ', 'ا')
                    .Replace('ى', 'ي');


            // إزالة الرموز وعلامات الترقيم
            result =
                Regex.Replace(
                    result,
                    @"[^\p{L}\p{N}\s]",
                    " ");


            // إزالة المسافات الزائدة
            result =
                Regex.Replace(
                    result,
                    @"\s+",
                    " ");


            return result.Trim();
        }


        private static string NormalizeWord(
            string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return string.Empty;
            }


            string result =
                word.Trim();


            // مثال:
            // المطبات  → مطبات
            // السرعة   → سرعة
            // الطرق    → طرق
            if (result.StartsWith("ال") &&
                result.Length > 4)
            {
                result =
                    result.Substring(2);
            }


            return result;
        }
    }
}