namespace DVLD.AI
{
    public class PdfExtractionResult
    {
        public int TotalPages { get; set; }

        public List<PdfPageText> Pages { get; set; } =
            new();
    }


    public class PdfPageText
    {
        public int PageNumber { get; set; }

        public string Text { get; set; } =
            string.Empty;
    }


    public static class PdfTextExtractor
    {
        public static PdfExtractionResult Extract(
            string filePath)
        {
            return AdaptivePdfTextExtractor
                .Extract(
                    filePath);
        }
    }
}