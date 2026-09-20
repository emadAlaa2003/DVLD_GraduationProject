namespace DVLD.AI
{
    public class TextChunk
    {
        public int PageNumber { get; set; }

        public int ChunkIndex { get; set; }

        public string Text { get; set; } = string.Empty;
    }


    public static class TextChunker
    {
        public static List<TextChunk> CreateChunks(
            PdfExtractionResult extractionResult,
            int chunkSize = 1200,
            int overlap = 200)
        {
            if (extractionResult == null)
            {
                throw new ArgumentNullException(
                    nameof(extractionResult));
            }

            if (chunkSize <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(chunkSize));
            }

            if (overlap < 0 || overlap >= chunkSize)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(overlap));
            }


            List<TextChunk> chunks =
                new List<TextChunk>();

            int chunkIndex = 1;


            foreach (PdfPageText page in extractionResult.Pages)
            {
                string text =
                    CleanText(page.Text);

                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }


                int start = 0;

                while (start < text.Length)
                {
                    int length =
                        Math.Min(
                            chunkSize,
                            text.Length - start);


                    string chunkText =
                        text.Substring(
                            start,
                            length).Trim();


                    if (!string.IsNullOrWhiteSpace(chunkText))
                    {
                        chunks.Add(
                            new TextChunk
                            {
                                PageNumber =
                                    page.PageNumber,

                                ChunkIndex =
                                    chunkIndex,

                                Text =
                                    chunkText
                            });

                        chunkIndex++;
                    }


                    if (start + length >= text.Length)
                    {
                        break;
                    }


                    start +=
                        chunkSize - overlap;
                }
            }


            return chunks;
        }


        private static string CleanText(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }


            text =
                text.Replace(
                    "\r",
                    " ");

            text =
                text.Replace(
                    "\n",
                    " ");


            while (text.Contains("  "))
            {
                text =
                    text.Replace(
                        "  ",
                        " ");
            }


            return text.Trim();
        }
    }
}