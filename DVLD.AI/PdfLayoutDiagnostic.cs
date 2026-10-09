using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace DVLD.AI
{
    public class PdfLayoutDiagnosticResult
    {
        public int PageNumber { get; set; }

        public int LetterCount { get; set; }

        public int WordCount { get; set; }

        public string RawLetterSequence { get; set; } =
            string.Empty;

        public List<PdfWordDiagnostic> Words { get; set; } =
            new();
    }


    public class PdfWordDiagnostic
    {
        public int Index { get; set; }

        public string Text { get; set; } =
            string.Empty;

        public double Left { get; set; }

        public double Right { get; set; }

        public double Bottom { get; set; }

        public double Top { get; set; }
    }


    public static class PdfLayoutDiagnostic
    {
        public static PdfLayoutDiagnosticResult AnalyzePage(
            string filePath,
            int pageNumber,
            int maximumWords = 250)
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


            if (pageNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(pageNumber));
            }


            using PdfDocument document =
                PdfDocument.Open(filePath);


            if (pageNumber >
                document.NumberOfPages)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(pageNumber));
            }


            var page =
                document.GetPage(pageNumber);


            /*
             * المرحلة الأولى:
             * ترتيب الحروف الخام كما يخزنها PDF.
             */
            string rawLetterSequence =
                string.Concat(
                    page.Letters
                        .Select(letter =>
                            letter.Value))
                    .Normalize(
                        NormalizationForm.FormKC);


            /*
             * لا نحتاج إرجاع صفحة كاملة ضخمة
             * أثناء التشخيص.
             */
            if (rawLetterSequence.Length >
                4000)
            {
                rawLetterSequence =
                    rawLetterSequence[..4000];
            }


            /*
             * المرحلة الثانية:
             * الكلمات كما يبنيها
             * NearestNeighbourWordExtractor.
             */
            var words =
                page.GetWords(
                        NearestNeighbourWordExtractor
                            .Instance)
                    .ToList();


            List<PdfWordDiagnostic> diagnosticWords =
                words
                    .Take(maximumWords)
                    .Select(
                        (word, index) =>
                            new PdfWordDiagnostic
                            {
                                Index =
                                    index + 1,

                                Text =
                                    (word.Text ??
                                     string.Empty)
                                    .Normalize(
                                        NormalizationForm.FormKC),

                                Left =
                                    word.BoundingBox
                                        .BottomLeft.X,

                                Right =
                                    word.BoundingBox
                                        .BottomRight.X,

                                Bottom =
                                    word.BoundingBox
                                        .BottomLeft.Y,

                                Top =
                                    word.BoundingBox
                                        .TopLeft.Y
                            })
                    .ToList();


            return new PdfLayoutDiagnosticResult
            {
                PageNumber =
                    pageNumber,

                LetterCount =
                    page.Letters.Count,

                WordCount =
                    words.Count,

                RawLetterSequence =
                    rawLetterSequence,

                Words =
                    diagnosticWords
            };
        }
    }
}