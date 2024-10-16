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
    using MongoDB.Bson;
    using Sprache;
    using System.Text.RegularExpressions;
    using UglyToad.PdfPig.Content;

    public static class PdfReader
    {
        //public static List<string> ReadPdf(string filePath)
        private static List<string> bulletPoints = new List<string>();

        public static List<(string SectionTitle, string Chunk)> ReadPdf(string filePath)

        {
            
            List<(string SectionTitle, string Chunk)> result = new List<(string SectionTitle, string Chunk)>();

            // Construct the full file path for the PDF in wwwroot/docs
            //filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "RAGDocuments", "ep100.pdf");

            try
            {
                using (var document = PdfDocument.Open(filePath))
                {
                    // Analyze document layout and get decorations for text blocks
                    //var docDecorations = DecorationTextBlockClassifier.Get(document.GetPages().ToList(),
                    //                   DefaultWordExtractor.Instance,
                    //                    DocstrumBoundingBoxes.Instance);
                    int pageIndex = 0;
                    string currentSection = "";    // To track the main section (e.g., "SECTION 100")
                    string currentSubsection = "";
                    string currentWithoutSection = "";  // To track the subsection (e.g., "SUBSECTION 111")
                    string currentHeading = "";
                    string currentSectionTitle = "";// To track headings (e.g., "Introduction")
                    //string buffer = "";
                    StringBuilder currentChunkText = new StringBuilder();

                    // Iterate through each page of the PDF document
                    foreach (var page in document.GetPages())
                    {

                        pageIndex++; // Increment page index

                        // Skip pages 1 to 4
                        if (pageIndex >= 1 && pageIndex <= 4)
                        {
                            continue; // Skip processing these pages
                        }
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

                        string mainSection = ""; // To track the main section (e.g., "GUIDE TO THE CODE")

                        // 4. Extract text, excluding headings and footers
                        foreach (var block in orderedTextBlocks)
                        {

                            var str = block.Text.Normalize(NormalizationForm.FormKC);
                            str = RemovePageNumbers(str); // Remove page numbers before further processing
                            // str = str.ReplaceLineEndings(" ");

                            // Preprocess: If string contains '\n', split into multiple lines
                            var lines = str.Contains("\n") ? str.Split('\n') : new string[] { str };

                            StringBuilder combinedText = new StringBuilder();

                            // Process each line (either from split or original)
                            foreach (var line in lines)
                            {
                                string trimmedLine = line.Trim();

                                // Detect and remove bullet points (adjust for your needs)
                                if (trimmedLine.StartsWith("•") || trimmedLine.StartsWith("-") || trimmedLine.StartsWith("o"))
                                    //|| IsNumberedBullet(trimmedLine.Trim()))
                                //if (Regex.IsMatch(trimmedLine, @"^(\u2022|\u25CB|\u25CF|\u25E6|o|-|•|\d+\.)"))
                                {
                                    // Option 1: Remove the bullet symbol and add the rest of the text
                                    AddBulletPoint(trimmedLine); // Add each bullet point to the list
                                    continue;
                                }

                                // Later, when finalizing the chunk
                                if (bulletPoints.Count > 0)
                                {
                                    // Append the processed bullet points to the current chunk text
                                    currentChunkText.Append(FinalizeBulletPoints());
                                }

                                // Check if the block is a section title (e.g., "SECTION 100 COMPLYING WITH THE CODE")
                                if (!string.IsNullOrWhiteSpace(trimmedLine) && (Regex.IsMatch(trimmedLine, @"^(SECTION|PART)\s+\d+")))
                                {
                                    // Finalize the current chunk before updating the section
                                    if (!string.IsNullOrWhiteSpace(currentHeading) || currentChunkText.Length > 0)
                                    {
                                        AddChunk(result, currentSection, currentWithoutSection, currentSubsection, currentSectionTitle, currentHeading, currentChunkText.ToString());
                                    }

                                    // Now update the section after saving the previous chunk
                                    currentSection = trimmedLine;
                                    currentSubsection = ""; // Clear any existing subsection
                                    currentHeading = ""; // Reset heading for the new section
                                    currentChunkText.Clear(); // Clear chunk text for new section

                                }
                                // Check if the block is a section title (e.g., "COMPLYING WITH THE CODE")
                                else if (!string.IsNullOrWhiteSpace(trimmedLine) && (Regex.IsMatch(trimmedLine, @"^[A-Z\s\-]+$")))
                                {
                                    // Finalize the current chunk before updating the section
                                    if (!string.IsNullOrWhiteSpace(currentHeading) || currentChunkText.Length > 0)
                                    {
                                        AddChunk(result, currentSection, currentWithoutSection, currentSubsection, currentSectionTitle, currentHeading, currentChunkText.ToString());
                                    }

                                    // Now update the section after saving the previous chunk
                                    currentWithoutSection = trimmedLine;
                                    currentSubsection = ""; // Clear any existing subsection
                                    currentHeading = ""; // Reset heading for the new section
                                    currentChunkText.Clear(); // Clear chunk text for new section

                                }

                                // Check if the block is a subsection title (e.g., "SUBSECTION 111 INTEGRITY")
                                else if (!string.IsNullOrEmpty(trimmedLine) && Regex.IsMatch(trimmedLine, @"^SUBSECTION\s+\d+"))
                                {

                                    // Finalize the current chunk before updating the subsection
                                    if (!string.IsNullOrWhiteSpace(currentHeading) || currentChunkText.Length > 0)
                                    {
                                        AddChunk(result, currentSection, currentWithoutSection, currentSubsection, currentSectionTitle, currentHeading, currentChunkText.ToString());
                                    }

                                    // Now update the subsection after saving the previous chunk
                                    currentSubsection = trimmedLine;
                                    currentSectionTitle = ""; // Clear section title for the new subsection
                                    currentHeading = ""; // Reset heading for the new subsection
                                    currentChunkText.Clear(); // Clear chunk text for new subsection

                                }

                                // Initialize the flag as false for every new block
                                bool chunkSaved = false; // This resets the flag for each new block being processed

                                // Check if the block is a heading
                                if (IsHeading(trimmedLine))
                                {
                                    if (!string.IsNullOrEmpty(trimmedLine) && Regex.IsMatch(trimmedLine, @"^[A-Z][A-Za-z\s\-]{0,50}$"))
                                    {
                                        if (!trimmedLine.Equals(currentSection, StringComparison.OrdinalIgnoreCase) &&
                                        !trimmedLine.Equals(currentWithoutSection, StringComparison.OrdinalIgnoreCase))
                                        {
                                            // If a chunk is being processed, save it before starting a new heading
                                            if (!string.IsNullOrWhiteSpace(currentSectionTitle) || currentChunkText.Length > 0)
                                            {
                                                AddChunk(result, currentSection, currentWithoutSection, currentSubsection, currentSectionTitle, currentHeading, currentChunkText.ToString());
                                                chunkSaved = true; // Set the flag as true
                                            }

                                            currentSectionTitle = trimmedLine;  // Update the section title
                                        }
                                    }

                                    // If a chunk is being processed, save it before starting a new heading
                                    //if (!string.IsNullOrWhiteSpace(currentHeading) || currentChunkText.Length > 0)
                                    if (!chunkSaved && (!string.IsNullOrWhiteSpace(currentHeading) || currentChunkText.Length > 0))
                                    {
                                        AddChunk(result, currentSection, currentWithoutSection, currentSubsection, currentSectionTitle, currentHeading, currentChunkText.ToString());
                                    }

                                    // Start a new chunk with the heading
                                    currentHeading = trimmedLine;
                                    currentChunkText.Clear();
                                }
                                else
                                {
                                    // Append the text to the current chunk
                                    currentChunkText.Append($" {trimmedLine}");
                                }
                            }
                        }
                        //pageIndex++; // Move to the next page

                        // After processing all pages, add any remaining chunk
                        if (!string.IsNullOrWhiteSpace(currentHeading) || currentChunkText.Length > 0)
                        {
                            AddChunk(result, currentSection, currentWithoutSection, currentSubsection, currentSectionTitle, currentHeading, currentChunkText.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening PDF file: {ex.Message}");
            }

            return result;
        }


        // Helper method to detect headings
        private static bool IsHeading(string text)
        {
            text = text.Trim();

            // Check for patterns like "Section 100", "Subsection 110", "Part 1"
            if (System.Text.RegularExpressions.Regex.IsMatch(text, @"^(SECTION|PART|SUBSECTION)\s+\d+|^[A-Z][A-Za-z\s\-]{0,50}$"))
            {
                return true;
            }

            // Check for uppercase titles with less than 10 words (general headings)
            if (text == text.ToUpper() && text.Length > 0 && text.Split(' ').Length < 10)
            {
                return true;
            }

            // Add any other heading detection logic as needed
            return false;
        }

        //Base version
        // Helper method to add a chunk of text to the result after semantic chunking
        /*
        private static void AddChunk(List<(string Heading, string Text)> result, string heading, string text, string sectionTitle)
        {
            text = text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            // Perform semantic chunking by splitting text on paragraph or sentence boundaries
            string[] semanticChunks = text.Split(new string[] { "\n\n", ". " }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var semanticChunk in semanticChunks)
            {
                // Combine the heading with each semantic chunk
                //string chunk = $"{heading} {semanticChunk}".Trim();
                //result.Add(chunk);

                // Combine the section title, heading, and each semantic chunk
                result.Add((sectionTitle, $"{heading} {semanticChunk}".Trim()));
            }
        }
        */

        //working version section 100 - introduction

        private static void AddChunk(List<(string SectionTitle, string Chunk)> result, string section, string undersection, string subsection, string currentSectionTitle, string heading, string text)
        {
            text = text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            string combinedSectionTitle;

            if (!string.IsNullOrEmpty(subsection))
            {
                combinedSectionTitle = subsection;
            }
            else if (!string.IsNullOrEmpty(section))
            {
                combinedSectionTitle = string.IsNullOrEmpty(undersection)
                    ? $"{section} - {currentSectionTitle}"
                    : $"{section} - {undersection} - {currentSectionTitle}";
            }
            else
            {
                // Fallback case: set the currentSectionTitle if section and subsection are empty
                combinedSectionTitle = currentSectionTitle;
            }

            // Perform semantic chunking by splitting text on paragraph or sentence boundaries
            string[] semanticChunks = text.Split(new string[] { "\n\n", ". " }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var semanticChunk in semanticChunks)
            {
                string chunk = $"{heading} {semanticChunk}".Trim();
                result.Add((combinedSectionTitle, chunk));
            }
        }


        private static void AddBulletPoint(string text)
        {
            // Remove bullet symbols and leading spaces
            string result = text.TrimStart(new char[] { '•', '-', ' ', 'o' }).Trim();

            // Add bullet point to the list
            bulletPoints.Add(result);
        }
        private static string FinalizeBulletPoints()
        {
            StringBuilder finalText = new StringBuilder();

            // Process all bullet points except the last one
            for (int i = 0; i < bulletPoints.Count; i++)
            {
                string bullet = bulletPoints[i];

                // If it's not the last bullet point, replace full stop with a comma
                if (i < bulletPoints.Count - 1 && bullet.EndsWith("."))
                {
                    bullet = bullet.TrimEnd('.') + ",";
                }

                finalText.Append(bullet + " ");
            }

            // Clear the list after processing
            bulletPoints.Clear();

            return finalText.ToString().Trim();
        }

        private static string RemovePageNumbers(string text)
        {
            // Regular expression to match a line containing only digits (e.g., page numbers like "14", "15", etc.)
            string pattern = @"^\d+\s*$";

            // Split the text into lines
            var lines = text.Split('\n');

            // Rebuild the text excluding lines that match the page number pattern
            StringBuilder cleanedText = new StringBuilder();

            foreach (var line in lines)
            {
                string trimmedLine = line.Trim();

                // If the line matches the page number pattern, skip it
                if (!Regex.IsMatch(trimmedLine, pattern))
                {
                    cleanedText.AppendLine(line); // Add the line to the result if it's not a page number
                }
            }

            return cleanedText.ToString().Trim();
        }

        private static bool IsNumberedBullet(string text)
        {
            // This will now match "1.", "2.", etc., but not "110.2 A1".
            return System.Text.RegularExpressions.Regex.IsMatch(text, @"^\d+\.$");
        }

    }

}
