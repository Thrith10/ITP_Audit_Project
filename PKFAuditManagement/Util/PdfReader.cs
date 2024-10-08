namespace PKFAuditManagement.Util
{
    using UglyToad.PdfPig;
    using UglyToad.PdfPig.DocumentLayoutAnalysis;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
    using UglyToad.PdfPig.Util;
    using System.Collections.Generic;
    using System.Text;

    public static class PdfReader
    {
        public static List<string> ReadPdf(string filePath)
        {
            List<string> result = new List<string>();

            // Construct the full file path for the PDF in wwwroot/docs
            //filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "RAGDocuments", "ep100.pdf");

            try
            {
                using (var document = PdfDocument.Open(filePath))
                {
                    // Analyze document layout and get decorations for text blocks
                    var docDecorations = DecorationTextBlockClassifier.Get(document.GetPages().ToList(),
                                        DefaultWordExtractor.Instance,
                                        DocstrumBoundingBoxes.Instance);
                    int pageIndex = 0;
                    string buffer = "";

                    // Iterate through each page of the PDF document
                    foreach (var page in document.GetPages())
                    {
                        // 0. Preprocessing
                        var letters = page.Letters; // Get the letters from the page

                        // 1. Extract words
                        var wordExtractor = NearestNeighbourWordExtractor.Instance;
                        var words = wordExtractor.GetWords(letters);

                        // 2. Segment the page into blocks of text
                        var pageSegmenter = DocstrumBoundingBoxes.Instance;
                        var textBlocks = pageSegmenter.GetBlocks(words);

                        // 3. Determine the reading order of the text blocks
                        var readingOrder = UnsupervisedReadingOrderDetector.Instance;
                        var orderedTextBlocks = readingOrder.Get(textBlocks);

                        // 4. Extract text, excluding headings and footers
                        foreach (var block in orderedTextBlocks)
                        {
                            var str = block.Text.Normalize(NormalizationForm.FormKC);
                            // Check if the block is not a decoration
                            if (!docDecorations[pageIndex].Any(x => x.BoundingBox.ToString() == block.BoundingBox.ToString()))
                            {
                                // If the block is likely a heading or title, buffer it
                                if (str.Split(' ').Length < 10)
                                {
                                    buffer += $" {str}";
                                }
                                else
                                {
                                    // Otherwise, add the text to the result list
                                    result.Add($"{buffer.ReplaceLineEndings(" ")} {str.ReplaceLineEndings(" ")}");
                                    buffer = ""; // Reset the buffer
                                }
                            }
                        }
                        pageIndex++; // Move to the next page
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening PDF file: {ex.Message}");
            }

            // Return the extracted text (or empty if there was an error)
            return result;
        }
    }

}
