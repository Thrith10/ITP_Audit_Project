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
                    string currentSSQMRefSection = ""; //Service Providers (Ref: Para. 16(v), 32(h))
                    //string buffer = "";
                    StringBuilder currentChunkText = new StringBuilder();

                    // Iterate through each page of the PDF document
                    foreach (var page in document.GetPages())
                    {

                        pageIndex++; // Increment page index

                        // Skip pages 1 to 4
                        if (pageIndex <= 2)
                        //if (pageIndex < 9)
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

                        string previousLine = null; // Store the previous line for checking

                        // 4. Extract text, excluding headings and footers
                        foreach (var block in orderedTextBlocks)
                        {

                            var str = block.Text.Normalize(NormalizationForm.FormKC);

                            str = RemovePageNumbers(str); // Remove page numbers before further processing
                            // str = str.ReplaceLineEndings(" ");

                            //
                            if (string.Equals(str, "Singapore Standard on Review Engagements (SSRE) 2400 (Revised), Engagements to Review Historical Financial Statements\r\nSingapore Standard on Assurance Engagements (SSAE) 3000 (Revised), Assurance Engagements Other than Audits or\r\nReviews of Historical Financial Information\r\nThe Institute of Singapore Chartered Accountants’ Ethics Pronouncement (EP) 100 (Revised on 14 August 2020) Code of\r\nProfessional Conduct and Ethics (ISCA Code)"))
                            {
                                continue;
                            }

                            // Preprocess: If string contains '\n', split into multiple lines
                            var lines = str.Contains("\n") ? str.Split('\n') : new string[] { str };

                            // Define a regex pattern to capture the pattern like A10. or A1.
                            var regexPattern = @"^([A-Z]\d{1,3}\.)\s*(.*)";

                            // Create a list to hold split results
                            List<string> processedLines = new List<string>();

                            StringBuilder combinedText = new StringBuilder();

                            // Process each line (either from split or original)
                            foreach (var line in lines)
                            {
                                string trimmedLine = line.Trim();

                                // Check if the line matches the pattern(A10.followed by text)

                                var match = Regex.Match(trimmedLine, regexPattern);
                                if (match.Success)
                                {
                                    // Add the A10. part as a separate entry
                                    processedLines.Add(match.Groups[1].Value);  // e.g., "A10."

                                    // Add the rest of the text as another entry
                                    processedLines.Add(match.Groups[2].Value);  // e.g., "The firm identifies deficiencies..."
                                }
                                else
                                {
                                    // If no match, add the original trimmedLine as it is
                                    processedLines.Add(trimmedLine);
                                }

                                // Now, `processedLines` contains the split values and can be processed further
                                foreach (var processedLine in processedLines)
                                {
                                    // Now `line` will contain either the first part (A10.) or the remaining part
                                    trimmedLine = processedLine;  // Assign each split line to trimmedLine for further processing

                                    // Continue with further processing as needed...

                                    // Skip lines that match header/footer patterns
                                    if (IsHeaderOrFooter(trimmedLine))
                                    {
                                        continue; // Skip processing for headers/footers
                                    }

                                    // Check if the line starts with "s. 62" and apply FixSpacingAfterS
                                    if (Regex.IsMatch(trimmedLine, @"^s\.\s+62"))
                                    {
                                        trimmedLine = FixSpacingAfterS(trimmedLine);
                                    }

                                    // If the previous line was "APPENDIX", check if the current line is a single letter
                                    if (previousLine != null && previousLine.Equals("APPENDIX", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (Regex.IsMatch(trimmedLine, @"^[A-Z]$"))
                                        {
                                            // Combine "APPENDIX" with the current line (which is the single letter)
                                            trimmedLine = previousLine + " " + trimmedLine;

                                            // Reset the previousLine to null since we've processed the combination
                                            previousLine = null;
                                        }
                                        else
                                        {
                                            // Process the "APPENDIX" line (since the current line isn't a letter)
                                            previousLine = null;  // Reset because the current line didn't combine with "APPENDIX"
                                        }
                                    }

                                    // If the line is "APPENDIX", set it to be checked in the next iteration
                                    if (trimmedLine.Equals("APPENDIX", StringComparison.OrdinalIgnoreCase))
                                    {
                                        previousLine = "APPENDIX";
                                        continue; // Skip further processing for now, wait for the next iteration to handle combining
                                    }

                                    // If the previous line contains "(Ref:" and is incomplete, combine it with the current line
                                    if (previousLine != null && (previousLine.Contains("(Ref:") || previousLine.Contains("(Ref.") && (!previousLine.Contains(")") || previousLine.EndsWith("and"))))
                                    {
                                        // Combine the previous line with the current line
                                        trimmedLine = CombineIncompleteReference(previousLine, trimmedLine);

                                        // Reset previousLine after combining
                                        previousLine = null;
                                    }

                                    if (previousLine != null && previousLine.StartsWith("Information That") && previousLine.EndsWith("or"))
                                    {
                                        // Combine the previous line with the current line
                                        trimmedLine = CombineIncompleteReference(previousLine, trimmedLine);

                                        // Reset previousLine after combining
                                        previousLine = null;
                                    }

                                    if (previousLine != null && previousLine.StartsWith("Whether Consultation") && previousLine.EndsWith("of"))
                                    {
                                        // Combine the previous line with the current line
                                        trimmedLine = CombineIncompleteReference(previousLine, trimmedLine);

                                        // Reset previousLine after combining
                                        previousLine = null;
                                    }

                                    // If the current line contains "(Ref:" and is incomplete, store it in previousLine
                                    if ((trimmedLine.Contains("(Ref:") || trimmedLine.Contains("(Ref."))
                                        && (!trimmedLine.Contains(")") || trimmedLine.EndsWith("and")))
                                    {
                                        previousLine = trimmedLine; // Store this line for combining with the next line
                                        continue; // Skip further processing of this line for now
                                    }

                                    //Specific to Page 47 SSQM1
                                    if(trimmedLine.StartsWith("Information That") && trimmedLine.EndsWith("or"))
                                    {
                                        previousLine = trimmedLine; // Store this line for combining with the next line
                                        continue; // Skip further processing of this line for now
                                    }

                                    //Specific to Page 17 SSQM2
                                    if (trimmedLine.StartsWith("Whether Consultation") && trimmedLine.EndsWith("of"))
                                    {
                                        previousLine = trimmedLine; // Store this line for combining with the next line
                                        continue; // Skip further processing of this line for now
                                    }

                                    // Check if the previous line starts with "ANNEX:" and current line is uppercase (likely part of the title)
                                    if (previousLine != null && previousLine.StartsWith("ANNEX:", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (Regex.IsMatch(trimmedLine, @"^[A-Z\s]+$")) // Check if the current line is uppercase and likely part of the title
                                        {
                                            // Combine the previous line with the current line
                                            trimmedLine = previousLine + " " + trimmedLine;

                                            // Reset the previousLine to null since we've processed the combination
                                            previousLine = null;
                                        }
                                        else
                                        {
                                            // Process the "ANNEX:" line (since the current line isn't part of the title)
                                            previousLine = null;  // Reset because the current line didn't combine with "ANNEX:"
                                        }
                                    }

                                    // If the line starts with "ANNEX:", set it to be checked in the next iteration
                                    //if (trimmedLine.StartsWith("ANNEX:", StringComparison.OrdinalIgnoreCase))
                                    if (trimmedLine.StartsWith("ANNEX:", StringComparison.OrdinalIgnoreCase) &&
                                    !trimmedLine.EndsWith("OFFENCES", StringComparison.OrdinalIgnoreCase))
                                    {
                                        previousLine = trimmedLine; // Store the line to combine it with the next
                                        continue; // Skip further processing for now, wait for the next iteration to handle combining
                                    }

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
                                    //if (!string.IsNullOrWhiteSpace(trimmedLine) && (Regex.IsMatch(trimmedLine, @"^(SECTION|PART)\s+\d+")))
                                    //if (!string.IsNullOrWhiteSpace(trimmedLine) && (Regex.IsMatch(trimmedLine, @"^(SECTION|PART|APPENDIX|SUPPLEMENT)\s+(\d+|[A-Z])")))
                                    if (!string.IsNullOrWhiteSpace(trimmedLine) &&
                                    (Regex.IsMatch(trimmedLine, @"^(SECTION|PART|APPENDIX|SUPPLEMENT|ANNEX):?\s+(\d+|[A-Z].*)")))

                                    {
                                        // Finalize the current chunk before updating the section
                                        if (!string.IsNullOrWhiteSpace(currentHeading) || currentChunkText.Length > 0)
                                        {
                                            AddChunk(result, currentSection, currentWithoutSection, currentSubsection, currentSectionTitle, currentSSQMRefSection, 
                                                currentHeading, currentChunkText.ToString());
                                        }

                                        // Now update the section after saving the previous chunk
                                        currentSection = trimmedLine;
                                        currentWithoutSection = "";
                                        currentSectionTitle = "";
                                        currentSubsection = ""; // Clear any existing subsection
                                        currentHeading = ""; // Reset heading for the new section
                                        currentSSQMRefSection = "";
                                        currentChunkText.Clear(); // Clear chunk text for new section

                                    }
                                    // Check if the block is a section title (e.g., "COMPLYING WITH THE CODE")
                                    else if (!string.IsNullOrWhiteSpace(trimmedLine) && (Regex.IsMatch(trimmedLine, @"^[A-Z\s\-]+$")))
                                    {
                                        // Finalize the current chunk before updating the section
                                        if (!string.IsNullOrWhiteSpace(currentHeading) || currentChunkText.Length > 0)
                                        {
                                            AddChunk(result, currentSection, currentWithoutSection, currentSubsection, currentSectionTitle, currentSSQMRefSection, 
                                                currentHeading, currentChunkText.ToString());
                                        }

                                        // Now update the section after saving the previous chunk
                                        currentWithoutSection = trimmedLine;
                                        currentSectionTitle = "";
                                        currentSubsection = ""; // Clear any existing subsection
                                        currentHeading = ""; // Reset heading for the new section
                                        currentChunkText.Clear(); // Clear chunk text for new section

                                    }

                                    // Check if the block is a subsection title (e.g., "SUBSECTION 111 INTEGRITY")
                                    //else if (!string.IsNullOrEmpty(trimmedLine) && Regex.IsMatch(trimmedLine, @"^SUBSECTION\s+\d+"))
                                    else if (!string.IsNullOrEmpty(trimmedLine) &&
                                    (Regex.IsMatch(trimmedLine, @"^SUBSECTION\s+\d+") ||
                                    Regex.IsMatch(trimmedLine, @"(offences?\s+under\s+s\.\d+[A-Za-z]?)", RegexOptions.IgnoreCase)))
                                    {


                                        // Finalize the current chunk before updating the subsection
                                        if (!string.IsNullOrWhiteSpace(currentHeading) || currentChunkText.Length > 0)
                                        {
                                            AddChunk(result, currentSection, currentWithoutSection, currentSubsection, currentSectionTitle, currentSSQMRefSection, 
                                                currentHeading, currentChunkText.ToString());
                                        }

                                        // Now update the subsection after saving the previous chunk
                                        currentSubsection = trimmedLine;
                                        currentSectionTitle = ""; // Clear section title for the new subsection
                                        currentHeading = ""; // Reset heading for the new subsection
                                        currentChunkText.Clear(); // Clear chunk text for new subsection

                                    //Check for SSQM Ref Para sections
                                    }else if(!string.IsNullOrEmpty(trimmedLine) && Regex.IsMatch(trimmedLine, @"^([A-Za-z\s,–'’()\-]+)\s*\(Ref[.:]\s+Para[.:]\s*\d+[a-z]?(–\d+[a-z]?)?(\([a-z]+\)(–\([a-z]+\))?)?(\([ivx]+\))?(,\s*\d+[a-z]?(–\d+[a-z]?)?(\([a-z]+\))?(\([ivx]+\))?)*\s*( and \d+[a-z]?(–\d+[a-z]?)?(\([a-z]+\))?(\([ivx]+\))?)?\s*\)$"))
                                    {

                                        // Finalize the current chunk before updating the subsection
                                        if (!string.IsNullOrWhiteSpace(currentHeading) || currentChunkText.Length > 0)
                                        {
                                            AddChunk(result, currentSection, currentWithoutSection, currentSubsection, currentSectionTitle, currentSSQMRefSection, 
                                                currentHeading, currentChunkText.ToString());
                                        }

                                        currentSSQMRefSection = trimmedLine;
                                        currentHeading = ""; // Reset heading for the new subsection
                                        currentChunkText.Clear(); // Clear chunk text for new subsection

                                    }

                                    // Initialize the flag as false for every new block
                                    bool chunkSaved = false; // This resets the flag for each new block being processed

                                    // Check if the block is a heading
                                    if (IsHeading(trimmedLine))
                                    {
                                        //if (!string.IsNullOrEmpty(trimmedLine) && Regex.IsMatch(trimmedLine, @"^[A-Z][A-Za-z\s\-]{0,100}$"))
                                        //if (!string.IsNullOrEmpty(trimmedLine) && Regex.IsMatch(trimmedLine, @"^[A-Z][A-Za-z\s\-\.,]{0,100}$"))
                                        //if (!string.IsNullOrEmpty(trimmedLine) && Regex.IsMatch(trimmedLine, @"^[A-Z][A-Za-z\s\-\.,()]{0,100}$"))
                                        //if (!string.IsNullOrEmpty(trimmedLine) && Regex.IsMatch(trimmedLine, @"^(s\.\d+[A-Za-z]?\s+[A-Za-z\.,()\s\-]{0,100})$"))
                                        if (!string.IsNullOrEmpty(trimmedLine)
                                            //&& (IsSpecialCase(trimmedLine) || IsRegexMatch(trimmedLine))
                                            && IsRegexMatch(trimmedLine)
                                            && IsProperlyFormatted(trimmedLine))

                                        {
                                            if (!trimmedLine.Equals(currentSection, StringComparison.OrdinalIgnoreCase) &&
                                                !trimmedLine.Equals(currentWithoutSection, StringComparison.OrdinalIgnoreCase))
                                            {
                                                // If a chunk is being processed, save it before starting a new heading
                                                if (!string.IsNullOrWhiteSpace(currentSectionTitle) || currentChunkText.Length > 0)
                                                {
                                                    AddChunk(result, currentSection, currentWithoutSection, currentSubsection, currentSectionTitle,
                                                        currentSSQMRefSection, currentHeading, currentChunkText.ToString());
                                                    chunkSaved = true; // Set the flag as true
                                                }

                                                currentSectionTitle = trimmedLine;  // Update the section title
                                                currentSSQMRefSection = "";
                                            }

                                        }
                                        // If a chunk is being processed, save it before starting a new heading
                                        //if (!string.IsNullOrWhiteSpace(currentHeading) || currentChunkText.Length > 0)
                                        if (!chunkSaved && (!string.IsNullOrWhiteSpace(currentHeading) || currentChunkText.Length > 0))
                                        {
                                            AddChunk(result, currentSection, currentWithoutSection, currentSubsection, currentSectionTitle,
                                                currentSSQMRefSection, currentHeading, currentChunkText.ToString());
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
                                processedLines.Clear();
                            }
                        }

                        //pageIndex++; // Move to the next page

                        // After processing all pages, add any remaining chunk
                        if (!string.IsNullOrWhiteSpace(currentHeading) || currentChunkText.Length > 0)
                        {
                            AddChunk(result, currentSection, currentWithoutSection, currentSubsection, currentSectionTitle,
                                currentSSQMRefSection, currentHeading, currentChunkText.ToString());
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

            // Return false if the text is empty after trimming
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            // Check if the text ends with a ; and reject it
            if (text.Trim().EndsWith(";"))
            {
                return false;  // Reject if the string ends with a period
            }

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

            if (Regex.IsMatch(text, @"^(Establishing|Procedures|Factors|Client|Additional|The Firm’s|Acceptance)\b"))
            {
                return true;
            }

            if (Regex.IsMatch(text, @"^Money\s+Laundering\b"))
            {
                return true;
            }


            // Check for legal section references like "s.96", "s.96A", "s.62", etc.
            if (Regex.IsMatch(text, @"^s\.\d+[A-Za-z]?\s+[A-Za-z]", RegexOptions.IgnoreCase))
            {
                return true;
            }

            // Allow headings that follow the "X (Ref: Para. N)" pattern
            if (Regex.IsMatch(text, @"^[A-Za-z\s]+\(Ref: Para\.\s+\d+\)$"))
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

        private static void AddChunk(List<(string SectionTitle, string Chunk)> result, string section, string undersection, 
            string subsection, string currentSectionTitle, string currentSSQMRefSection,string heading, string text)
        {
            text = text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            string combinedSectionTitle;

            if (!string.IsNullOrEmpty(subsection))
            {
                //combinedSectionTitle = subsection;

                combinedSectionTitle = !string.IsNullOrEmpty(section) && !string.IsNullOrEmpty(currentSectionTitle)
                    ? $"{section} - {subsection} - {currentSectionTitle}"
                    : subsection;  // Include only subsection if section is empty
            }
            else if (!string.IsNullOrEmpty(section))
            {
                combinedSectionTitle = string.IsNullOrEmpty(undersection)
                    //? $"{section} - {currentSectionTitle}"
                    //? $"{section}{(!string.IsNullOrEmpty(currentSectionTitle) ? $" - {currentSectionTitle}" : "")}"
                    //: $"{section} - {undersection} - {currentSectionTitle}";
                    ? $"{section}{(!string.IsNullOrEmpty(currentSectionTitle) ? $" - {currentSectionTitle}" : "")}"
                    : $"{section} - {undersection}{(!string.IsNullOrEmpty(currentSectionTitle) ? $" - {currentSectionTitle}" : "")}";
            }
            else if (!string.IsNullOrEmpty(undersection))
            {
                combinedSectionTitle = !string.IsNullOrEmpty(currentSectionTitle)
                    ? $"{undersection} - {currentSectionTitle}"
                    : undersection;  // Include only undersection if currentSectionTitle is empty
            }
            else if (!string.IsNullOrEmpty(currentSectionTitle)) //For SSQM1 Ref headings
            {
                combinedSectionTitle = !string.IsNullOrEmpty(currentSectionTitle)
                    ? $"{currentSectionTitle} {(!string.IsNullOrEmpty(currentSSQMRefSection) ? $" - {currentSSQMRefSection}" : "")}"
                    : currentSectionTitle;  // Include only currentSectionTitle if currentSSQMRefSection is empty
            }
            else
            {
                // Fallback case: set the currentSectionTitle if section and subsection are empty
                combinedSectionTitle = currentSectionTitle;
            }

            /*
            // Perform semantic chunking by splitting text on paragraph or sentence boundaries
            string[] semanticChunks = text.Split(new string[] { "\n\n", ". " }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var semanticChunk in semanticChunks)
            {
                string chunk = $"{heading} {semanticChunk}".Trim();
                result.Add((combinedSectionTitle, chunk));
            }
            */

            // Temporary marker to replace the period in the references
            string marker = "<REF_PERIOD>";

            // Replace the period inside references to avoid splitting at that point
            /*
            string modifiedText = Regex.Replace(text, @"(Ref:\s+Para\.\s+A\d+(-\d+)?\))", match =>
            {
                return match.Value.Replace(".", marker);
            });

            */

            // Also prevent splitting if the reference is at the end of a sentence, e.g., after a period
            // We look for any sentence that ends with a reference like "some text. (Ref: Para. A12)"
            // Replace the period inside references that contain multiple parts (e.g., Para. A10, A159–A160)
            string modifiedText = Regex.Replace(text, @"(Ref:\s+Para\.)", match =>
            {
                return match.Value.Replace(".", marker);
            });

            modifiedText = Regex.Replace(modifiedText, @"\.(?=\s*\(Ref:)", match => marker);


            // Perform semantic chunking by splitting text on paragraph or sentence boundaries
            string[] semanticChunks = modifiedText.Split(new string[] { "\n\n", ". " }, StringSplitOptions.RemoveEmptyEntries);

            // After chunking, restore the original periods inside the references
            for (int i = 0; i < semanticChunks.Length; i++)
            {
                // Replace the temporary marker back with the original period
                semanticChunks[i] = semanticChunks[i].Replace(marker, ".");

                // Add the chunk to the result with the heading and combined section title
                string chunk = $"{heading} {semanticChunks[i]}".Trim();
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

        // Function to detect and skip common headers/footers like "EP 200"
        private static bool IsHeaderOrFooter(string text)
        {
            // Check for common header/footer patterns like "EP 200"
            if (Regex.IsMatch(text.Trim(), @"^EP\s+\d+$"))
            {
                return true;
            }

            // Check for footer patterns like "SSA 250 (Revised), paragraphs 13, 14, 15 and 17."
            if (Regex.IsMatch(text.Trim(), @"^SSA\s+\d+(\s*\(Revised\))?,\s*paragraph(s)?\s+\d+.*\.$"))
            {
                return true;
            }

            // Check for footnote numbering such as "8 SSA 250 (Revised), paragraphs 13, 14, 15 and 17."
            if (Regex.IsMatch(text.Trim(), @"^\d+\s+SSA\s+\d+(\s*\(Revised\))?,\s*paragraph(s)?\s+\d+.*\.$"))
            {
                return true;
            }

            // Add other footer detection logic as needed
            return false;
        }

        // Function to detect and fix spacing after "s."
        private static string FixSpacingAfterS(string text)
        {
            // Use Regex to find and replace spaces after "s."
            return Regex.Replace(text, @"s\.\s+(\d+[A-Za-z]?)", @"s.$1");
        }

        //Prevent paragraph sentences from entering SectionTitle
        private static bool IsProperlyFormatted(string text)
        {
            string[] words = text.Split(' ');
            string[] exceptions = { "of", "and", "the", "in", "on", "at", "to", "from", "by", "for", "or", "is", "this", "a", "with", "an" };  // Common small words

            // Check if the text ends with a period and reject it
            if (text.Trim().EndsWith("."))
            {
                return false;  // Reject if the string ends with a period
            }

            if (text.Equals("The"))
            {
                return false;
            }

            //Specific to SSQM1 Page 50
            if (text.Equals("Public sector considerations"))
            {
                return true;
            }

            //Specific to SSQM1 Page 59
            if (text.Equals("Quality Risks and Responses"))
            {
                return false;
            }

            //Specific to SSQM1 Page 59
            if (text.Equals("Nature of the Findings and Their Pervasiveness"))
            {
                return false;
            }

            //Specific to SSQM1 Page 59
            if (text.Equals("Extent of Monitoring Activity and Extent of Findings"))
            {
                return false;
            }

            // Check for the specific "s." followed by numbers and optional letters (e.g., "s.96A")
            if (Regex.IsMatch(text.Trim(), @"^s\.\d+[A-Za-z]?\s+[A-Za-z\s\-]+$"))
            {
                return true;  // Accept strings that start with "s.96", "s.96A", "s.62B", etc.
            }

            foreach (var word in words)
            {
                if (Array.Exists(exceptions, e => e.Equals(word, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;  // Skip common small words
                }

                if (char.IsLower(word[0]))
                {
                    return false;  // Reject if any non-exception word starts with a lowercase letter
                }
            }

            return true;
        }

        // Helper method to combine incomplete references
        private static string CombineIncompleteReference(string currentLine, string nextLine)
        {
            // Combine the current line with the next line
            string combinedLine = currentLine.TrimEnd() + " " + nextLine.TrimStart();

            // Check if the combined string contains the correct reference format
            if (IsValidReferenceFormat(combinedLine))
            {
                return combinedLine; // Return combined line if valid
            }

            // If the combined string doesn't match the valid format, return the original lines separately
            return currentLine + " " + nextLine; // You can decide how to handle this
        }

        // Helper method to check if the combined line has the correct reference format
        private static bool IsValidReferenceFormat(string text)
        {
            // Check for patterns like "Para. A10)", "Para. A10-A11)", "A10)", and "Para. A10, A159-A160)"
            return Regex.IsMatch(text, @"\b(Para\.\s+A\d+(\-\d+)?(,\s*A\d+(\-\d+)?)*\))");
        }

        //Methods for after IsHeading Checks for sectionTitle
        private static bool IsSpecialCase(string line)
        {

            return line.Equals("The Firm’s Risk Assessment Process (Ref: Para. 23)", StringComparison.OrdinalIgnoreCase)
                || line.Equals("Relevant Ethical Requirements(Ref: Para. 16(t), 29)", StringComparison.OrdinalIgnoreCase)
                || line.Equals("Resources (Ref: Para. 32)", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsRegexMatch(string line)
        {
            return Regex.IsMatch(line, @"^([A-Z][A-Za-z\s\-\.,()’']{0,100}|s\.\d+[A-Za-z]?\s+[A-Za-z\.,()\s\-]{0,100})$");
        }


    }

}
