using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace DVLD.AI
{
    public class PdfExtractionResult
    {
        public int TotalPages { get; set; }

        public List<PdfPageText> Pages { get; set; } = new();
    }


    public class PdfPageText
    {
        public int PageNumber { get; set; }

        public string Text { get; set; } = string.Empty;
    }


    public static class PdfTextExtractor
    {
        public static PdfExtractionResult Extract(string filePath)
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

            PdfExtractionResult result = new PdfExtractionResult();

            using (PdfDocument document = PdfDocument.Open(filePath))
            {
                result.TotalPages = document.NumberOfPages;

                foreach (var page in document.GetPages())
                {
                    string text =
                        ContentOrderTextExtractor.GetText(page);

                    result.Pages.Add(new PdfPageText
                    {
                        PageNumber = page.Number,
                        Text = text
                    });
                }
            }

            return result;
        }
    }
}