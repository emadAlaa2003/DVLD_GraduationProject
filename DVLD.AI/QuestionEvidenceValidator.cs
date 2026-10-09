using System.Text;
using System.Text.RegularExpressions;

namespace DVLD.AI
{
    public class QuestionEvidenceValidationResult
    {
        public bool IsValid { get; set; }

        public string FailureReason { get; set; } =
            string.Empty;
    }


    public static class QuestionEvidenceValidator
    {
        private const int MinimumEvidenceLength = 25;

        private const int MinimumArabicWordCount = 5;

        private const int MinimumInformativeWordCount = 3;


        private static readonly HashSet<string> ArabicStopWords =
            new HashSet<string>
            {
                "في",
                "من",
                "إلى",
                "الى",
                "على",
                "عن",
                "مع",
                "أن",
                "ان",
                "أو",
                "او",
                "ثم",
                "كما",
                "هو",
                "هي",
                "هذا",
                "هذه",
                "ذلك",
                "تلك",
                "التي",
                "الذي",
                "ما",
                "لا",
                "يتم",
                "يجب"
            };


        private static readonly HashSet<string> InvalidEndingWords =
            new HashSet<string>
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


        public static QuestionEvidenceValidationResult Validate(
            string sourceText,
            string sourceEvidence)
        {
            if (string.IsNullOrWhiteSpace(sourceText))
            {
                return Invalid(
                    "Source text is empty.");
            }


            if (string.IsNullOrWhiteSpace(sourceEvidence))
            {
                return Invalid(
                    "SourceEvidence is empty.");
            }


            string normalizedSource =
                NormalizeForComparison(
                    sourceText);

            string normalizedEvidence =
                NormalizeForComparison(
                    sourceEvidence);


            if (normalizedEvidence.Length <
                MinimumEvidenceLength)
            {
                return Invalid(
                    "SourceEvidence is too short.");
            }


            if (!normalizedSource.Contains(
                    normalizedEvidence,
                    StringComparison.Ordinal))
            {
                return Invalid(
                    "SourceEvidence was not found in the source chunk.");
            }


            List<string> arabicWords =
                ExtractArabicWords(
                    normalizedEvidence);


            if (arabicWords.Count <
                MinimumArabicWordCount)
            {
                return Invalid(
                    "SourceEvidence does not contain enough Arabic words.");
            }


            string lastWord =
                arabicWords.LastOrDefault() ??
                string.Empty;


            if (InvalidEndingWords.Contains(
                    lastWord))
            {
                return Invalid(
                    "SourceEvidence appears to end with an incomplete phrase.");
            }


            int informativeWordCount =
                arabicWords
                    .Where(word =>
                        word.Length >= 3 &&
                        !ArabicStopWords.Contains(word))
                    .Distinct()
                    .Count();


            if (informativeWordCount <
                MinimumInformativeWordCount)
            {
                return Invalid(
                    "SourceEvidence does not contain enough meaningful information.");
            }


            int arabicLetterCount =
                normalizedEvidence.Count(
                    character =>
                        IsArabicLetter(
                            character));


            double arabicRatio =
                (double)arabicLetterCount /
                normalizedEvidence.Length;


            if (arabicRatio < 0.45)
            {
                return Invalid(
                    "SourceEvidence contains too much non-Arabic or noisy content.");
            }


            return new QuestionEvidenceValidationResult
            {
                IsValid = true,
                FailureReason = string.Empty
            };
        }


        private static QuestionEvidenceValidationResult Invalid(
            string reason)
        {
            return new QuestionEvidenceValidationResult
            {
                IsValid = false,
                FailureReason = reason
            };
        }


        private static List<string> ExtractArabicWords(
            string text)
        {
            MatchCollection matches =
                Regex.Matches(
                    text,
                    @"[\u0600-\u06FF]+");


            return matches
                .Select(match =>
                    match.Value)
                .Where(word =>
                    !string.IsNullOrWhiteSpace(
                        word))
                .ToList();
        }


        private static bool IsArabicLetter(
            char character)
        {
            return
                (character >= '\u0621' &&
                 character <= '\u063A') ||

                (character >= '\u0641' &&
                 character <= '\u064A');
        }


        private static string NormalizeForComparison(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }


            string normalized =
                text.Normalize(
                    NormalizationForm.FormKC);


            normalized =
                normalized.Replace(
                    "\u0640",
                    string.Empty);


            normalized =
                normalized
                    .Replace('\u00A0', ' ')
                    .Replace('\u2007', ' ')
                    .Replace('\u202F', ' ');


            normalized =
                Regex.Replace(
                    normalized,
                    @"\s+",
                    " ");


            return normalized.Trim();
        }
    }
}