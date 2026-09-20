using System.Text.RegularExpressions;

namespace DVLD.AI
{
    public class PdfTextQualityResult
    {
        public double Score { get; set; }

        public double ArabicRatio { get; set; }

        public double LetterRatio { get; set; }

        public double NoiseRatio { get; set; }

        public double ReversedArabicPenalty { get; set; }

        public bool IsUsable { get; set; }
    }


    public static class PdfTextQualityEvaluator
    {
        public static PdfTextQualityResult Evaluate(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new PdfTextQualityResult
                {
                    Score = 0,
                    IsUsable = false
                };
            }


            int totalCharacters =
                Math.Max(
                    text.Length,
                    1);

            int letters = 0;
            int arabicLetters = 0;
            int noiseCharacters = 0;


            foreach (char character in text)
            {
                if (char.IsLetter(character))
                {
                    letters++;

                    if (IsArabicCharacter(
                            character))
                    {
                        arabicLetters++;
                    }
                }


                if (!char.IsLetterOrDigit(character) &&
                    !char.IsWhiteSpace(character) &&
                    !IsNormalPunctuation(character))
                {
                    noiseCharacters++;
                }
            }


            int safeLetters =
                Math.Max(
                    letters,
                    1);


            double arabicRatio =
                (double)arabicLetters /
                safeLetters;


            double letterRatio =
                (double)letters /
                totalCharacters;


            double noiseRatio =
                (double)noiseCharacters /
                totalCharacters;


            double score = 0;


            // ---------------------------------------------
            // 1. نسبة الحروف المقروءة
            // ---------------------------------------------

            if (letterRatio >= 0.65)
            {
                score += 30;
            }
            else if (letterRatio >= 0.45)
            {
                score += 22;
            }
            else if (letterRatio >= 0.25)
            {
                score += 12;
            }


            // ---------------------------------------------
            // 2. النص العربي
            // لا نعاقب النص الإنجليزي،
            // لكن إذا الصفحة عربية يكون وجود
            // نسبة جيدة من العربي نقطة إيجابية.
            // ---------------------------------------------

            if (arabicRatio >= 0.60)
            {
                score += 30;
            }
            else if (arabicRatio >= 0.35)
            {
                score += 24;
            }
            else if (arabicRatio >= 0.15)
            {
                score += 15;
            }
            else
            {
                score += 8;
            }


            // ---------------------------------------------
            // 3. وجود كلمات عربية شائعة باتجاه صحيح
            // ---------------------------------------------

            int naturalArabicHits =
                CountNaturalArabicWords(
                    text);


            score +=
                Math.Min(
                    naturalArabicHits * 3,
                    20);


            // ---------------------------------------------
            // 4. عقوبة الكلمات العربية الشائعة المقلوبة
            // ---------------------------------------------

            int reversedArabicHits =
                CountReversedArabicWords(
                    text);


            double reversedPenalty =
                Math.Min(
                    reversedArabicHits * 8,
                    35);


            score -=
                reversedPenalty;


            // ---------------------------------------------
            // 5. عقوبة الضوضاء
            // ---------------------------------------------

            score -=
                noiseRatio * 40;


            // ---------------------------------------------
            // 6. وجود جمل كاملة ومحتوى حقيقي
            // ---------------------------------------------

            if (text.Length >= 300)
            {
                score += 10;
            }
            else if (text.Length >= 100)
            {
                score += 5;
            }


            score =
                Math.Clamp(
                    score,
                    0,
                    100);


            return new PdfTextQualityResult
            {
                Score =
                    score,

                ArabicRatio =
                    arabicRatio,

                LetterRatio =
                    letterRatio,

                NoiseRatio =
                    noiseRatio,

                ReversedArabicPenalty =
                    reversedPenalty,

                IsUsable =
                    score >= 40 &&
                    letterRatio >= 0.20
            };
        }


        private static int CountNaturalArabicWords(
            string text)
        {
            string[] commonWords =
            {
                "الطريق",
                "المرور",
                "السائق",
                "المركبات",
                "المركبة",
                "السرعة",
                "المسافة",
                "العمل",
                "المنطقة",
                "الإشارة",
                "الحوادث",
                "السلامة",
                "يجب",
                "يمكن",
                "عند",
                "على",
                "من",
                "إلى"
            };


            int count = 0;


            foreach (string word
                     in commonWords)
            {
                count +=
                    Regex.Matches(
                        text,
                        Regex.Escape(word))
                    .Count;
            }


            return count;
        }


        private static int CountReversedArabicWords(
            string text)
        {
            string[] reversedWords =
            {
                "قيرطلا",
                "رورملا",
                "قئاسلا",
                "تابكرملا",
                "ةبكرملا",
                "ةعرسلا",
                "ةفاسملا",
                "لمعلا",
                "ةقطنملا",
                "ةراشإلا",
                "ثداوحلا",
                "ةمالسلا",
                "بجي",
                "نكمي",
                "دنع",
                "ىلع",
                "نم",
                "ىلإ"
            };


            int count = 0;


            foreach (string word
                     in reversedWords)
            {
                count +=
                    Regex.Matches(
                        text,
                        Regex.Escape(word))
                    .Count;
            }


            return count;
        }


        private static bool IsArabicCharacter(
            char character)
        {
            return
                (character >= '\u0600' &&
                 character <= '\u06FF') ||

                (character >= '\u0750' &&
                 character <= '\u077F') ||

                (character >= '\u08A0' &&
                 character <= '\u08FF') ||

                (character >= '\uFB50' &&
                 character <= '\uFDFF') ||

                (character >= '\uFE70' &&
                 character <= '\uFEFF');
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
                character == '+' ||
                character == '=' ||
                character == '<' ||
                character == '>';
        }
    }
}