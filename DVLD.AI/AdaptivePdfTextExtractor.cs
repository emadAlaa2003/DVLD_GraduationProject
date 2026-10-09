using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace DVLD.AI
{
    public static class AdaptivePdfTextExtractor
    {
        private const double MinimumUsefulScore = 40;

        private const double PreferredMethodMargin = 5;


        public static PdfExtractionResult Extract(
            string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "File path is required.",
                    nameof(filePath));
            }


            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "PDF file was not found.",
                    filePath);
            }


            PdfExtractionResult result =
                new PdfExtractionResult();


            using PdfDocument document =
                PdfDocument.Open(
                    filePath);


            result.TotalPages =
                document.NumberOfPages;


            foreach (Page page
                     in document.GetPages())
            {
                string selectedText =
                    ExtractBestPageText(
                        page);


                result.Pages.Add(
                    new PdfPageText
                    {
                        PageNumber =
                            page.Number,

                        Text =
                            selectedText
                    });
            }


            return result;
        }


        public static string ExtractBestPageText(
            Page page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(
                    nameof(page));
            }


            // ==========================================
            // الطريقة الأولى:
            // الاستخراج الطبيعي من PdfPig
            // ==========================================

            string normalText =
                string.Empty;


            try
            {
                normalText =
                    ContentOrderTextExtractor
                        .GetText(
                            page);

                normalText =
                    NormalizeText(
                        normalText);
            }
            catch
            {
                normalText =
                    string.Empty;
            }


            // ==========================================
            // الطريقة الثانية:
            // إعادة بناء RTL من إحداثيات الحروف
            // ==========================================

            string rtlText =
                string.Empty;


            try
            {
                rtlText =
                    PdfRtlTextReconstructor
                        .Reconstruct(
                            page);

                rtlText =
                    NormalizeText(
                        rtlText);
            }
            catch
            {
                rtlText =
                    string.Empty;
            }


            // ==========================================
            // تقييم جودة الطريقتين
            // ==========================================

            PdfTextQualityResult normalQuality =
                PdfTextQualityEvaluator
                    .Evaluate(
                        normalText);


            PdfTextQualityResult rtlQuality =
                PdfTextQualityEvaluator
                    .Evaluate(
                        rtlText);


            // ==========================================
            // حالة 1:
            // واحدة فقط صالحة
            // ==========================================

            if (rtlQuality.IsUsable &&
                !normalQuality.IsUsable)
            {
                return rtlText;
            }


            if (normalQuality.IsUsable &&
                !rtlQuality.IsUsable)
            {
                return normalText;
            }


            // ==========================================
            // حالة 2:
            // الطريقتان صالحتان
            //
            // لا نختار RTL لمجرد أنه أعلى بنقطة
            // أو نقطتين.
            //
            // لازم يكون الفرق واضحاً.
            // ==========================================

            if (normalQuality.IsUsable &&
                rtlQuality.IsUsable)
            {
                if (rtlQuality.Score >=
                    normalQuality.Score +
                    PreferredMethodMargin)
                {
                    return rtlText;
                }


                /*
                 * إذا النتائج متقاربة،
                 * نحافظ على الاستخراج الطبيعي.
                 *
                 * هذا مهم لملفات PDF الطبيعية
                 * والملفات الإنجليزية.
                 */
                return normalText;
            }


            // ==========================================
            // حالة 3:
            // ولا واحدة وصلت لمستوى IsUsable
            //
            // نأخذ الأعلى إذا كان على الأقل
            // يحتوي نصاً مفيداً.
            // ==========================================

            if (rtlQuality.Score >
                    normalQuality.Score &&
                rtlQuality.Score >=
                    MinimumUsefulScore &&
                !string.IsNullOrWhiteSpace(
                    rtlText))
            {
                return rtlText;
            }


            if (normalQuality.Score >=
                    MinimumUsefulScore &&
                !string.IsNullOrWhiteSpace(
                    normalText))
            {
                return normalText;
            }


            // ==========================================
            // حالة 4:
            // الجودة ضعيفة للطريقتين.
            //
            // لا نخترع نصاً ولا نعمل Reverse أعمى.
            //
            // نرجع الأفضل الموجود مؤقتاً،
            // وسيتم التعامل مع الملفات الرديئة
            // أو Scanner لاحقاً من خلال OCR /
            // Document Quality Validation.
            // ==========================================

            if (rtlQuality.Score >
                normalQuality.Score)
            {
                return rtlText;
            }


            return normalText;
        }


        private static string NormalizeText(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }


            string normalized =
                text.Normalize(
                    NormalizationForm.FormKC);


            // إزالة التطويل العربي
            normalized =
                normalized.Replace(
                    "\u0640",
                    string.Empty);


            // توحيد أنواع المسافات
            normalized =
                normalized
                    .Replace(
                        '\u00A0',
                        ' ')
                    .Replace(
                        '\u2007',
                        ' ')
                    .Replace(
                        '\u202F',
                        ' ');


            string[] lines =
                normalized.Split(
                    new[]
                    {
                        "\r\n",
                        "\r",
                        "\n"
                    },
                    StringSplitOptions.None);


            List<string> cleanedLines =
                new List<string>();


            foreach (string line
                     in lines)
            {
                string cleanedLine =
                    Regex.Replace(
                        line,
                        @"[ \t]+",
                        " ")
                    .Trim();


                if (!string.IsNullOrWhiteSpace(
                        cleanedLine))
                {
                    cleanedLines.Add(
                        cleanedLine);
                }
            }


            return string.Join(
                Environment.NewLine,
                cleanedLines);
        }
    }
}