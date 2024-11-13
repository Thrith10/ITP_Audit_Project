using DocumentFormat.OpenXml.Office2013.Word;
using MongoDB.Bson;
using MongoDB.Driver;
using PKFAuditManagement.Controllers;

namespace PKFAuditManagement.Services
{
    public interface IMongoDBService
    {
        Task<List<(string SectionTitle, string ParagraphText, string DocumentName)>> FindSimilarDocumentsAsync(double[] embedding, string userInput);
        Task<string> SaveParagraphsToMongoDBAsync(List<(string SectionTitle, string Chunk)> paragraphs, string documentName);
        Task<string> SaveSectionsToMongoDBAsync(List<BsonDocument> sections, string documentName);
    }

    public class MongoDBService : IMongoDBService
    {
        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly IEmbeddingService _embeddingService;
        private readonly ILogger<MongoDBService> _logger;
        public MongoDBService(string connectionString, IEmbeddingService embeddingService, ILogger<MongoDBService> logger)
        {
            // Configure embedding service
            _embeddingService = embeddingService;

            // Configure MongoDB client settings
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);

            // Create the MongoDB client
            _client = new MongoClient(settings);

            // Access the sample_mflix database
            _database = _client.GetDatabase("audit_documents");

            // Reference to the embedded_movies collection
            _collection = _database.GetCollection<BsonDocument>("docs_combined_v1");
            _logger = logger;
        }

        public async Task<string> SaveParagraphsToMongoDBAsync(List<(string SectionTitle, string Chunk)> paragraphs, string documentName)
        {
            // Insert each paragraph into the MongoDB collection
            for (var i = 0; i < paragraphs.Count; i++)
            {
                //var paragraph = paragraphs[i];

                var paragraphText = paragraphs[i].Chunk;
                var sectionTitle = paragraphs[i].SectionTitle;

                // Call embedding service to generate embeddings based on user input
                double[] embeddings = await _embeddingService.GenerateEmbeddingsAsync(paragraphText);

                // Failed to generate embeddings
                if (embeddings.Length == 0) // Check the length of the float array
                {
                    return "Failed to generate embeddings.";
                }

                // Convert the float array to a BsonArray for storage
                var embeddingArray = new BsonArray(embeddings.Select(e => new BsonDouble(e)));

                // Create a MongoDB document
                var document = new BsonDocument
                {
                    { "ParagraphIndex", i },
                    { "DocumentName", documentName },
                    { "SectionTitle", sectionTitle },  // Add section title metadata
                    { "ParagraphText", paragraphText },
                    { "plot_embedding", embeddingArray  }
                };

                // Asynchronously insert the document into the collection
                await _collection.InsertOneAsync(document);

                // Print the index of the inserted paragraph
                Console.Write($".{i}");
            }

            return "All paragraphs saved successfully."; // Return success message
        }

        /*
        public async Task<List<string>> FindSimilarDocumentsAsync(double[] embedding)
        {
            try
            {
                var pipeline = new BsonDocument[]
                {
                    new BsonDocument("$vectorSearch", new BsonDocument
                    {
                        { "queryVector", new BsonArray(embedding) },
                        { "path", "plot_embedding" }, // This field name has to match the one in the collection
                        { "numCandidates", 100 },
                        { "limit", 5 },
                        { "index", "AuditDoc" } // This index has to match the Atlas Search Index name in MongoDB.
                    })
                };

                var result = await _collection.AggregateAsync<BsonDocument>(pipeline);
                var documents = new List<string>();

                await result.ForEachAsync(doc =>
                {
                    // Each document in the collection has a column "ParagraphText" to store the information
                    var title = doc["ParagraphText"].ToString();
                    documents.Add(title);
                });

                return documents;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during vector search: {ex.Message}");
                return new List<string> { "Error during vector search" };
            }
        }
        */

        /*
        public async Task<List<(string SectionTitle, string ParagraphText, string DocumentName)>> FindSimilarDocumentsAsync(double[] embedding, string userInput)
        {
            _logger.LogInformation("Entered FindSimilarDocumentsAsync method.");

            try
            {
                var documents = new List<(string SectionTitle, string ParagraphText, string DocumentName)>();

                // First, try to find all paragraphs with the exact or partial match for the SectionTitle in MongoDB
                var filter = Builders<BsonDocument>.Filter.Regex("SectionTitle", new BsonRegularExpression(userInput, "i"));
                var directMatchResult = await _collection.Find(filter).ToListAsync();

                directMatchResult.ForEach(doc =>
                {
                    // Debug logging
                    _logger.LogInformation("Document contents:");
                    foreach (var element in doc.Elements)
                    {
                        _logger.LogInformation($"{element.Name}: {element.Value}");
                    }

                    var sectionTitle = doc["SectionTitle"].ToString();
                    var paragraphText = doc["ParagraphText"].ToString();
                    var testName = doc["SectionTitle"].ToString();

                    documents.Add((sectionTitle, paragraphText, testName));
                });
                

                // If we found direct matches, return all the paragraphs with the matching SectionTitle
                if (documents.Count > 0)
                {
                    return documents;  // Return all paragraphs with the matched SectionTitle
                }

                // If no exact matches are found, fallback to the vector search using embeddings
                var pipeline = new BsonDocument[]
                {
                    new BsonDocument("$vectorSearch", new BsonDocument
                    {
                        { "queryVector", new BsonArray(embedding) },
                        { "path", "plot_embedding" },
                        { "numCandidates", 200 },
                        { "limit", 10 },
                        { "index", "AuditDoc" }
                    })
                };

                var result = await _collection.AggregateAsync<BsonDocument>(pipeline);

                await result.ForEachAsync(doc =>
                {
                    _logger.LogInformation("Document contents:");
                    foreach (var element in doc.Elements)
                    {
                        _logger.LogInformation($"{element.Name}: {element.Value}");
                    }
                    var sectionTitle = doc["SectionTitle"].ToString();
                    var paragraphText = doc["ParagraphText"].ToString();
                    var testName = doc["SectionTitle"].ToString();

                    documents.Add((sectionTitle, paragraphText, testName));
                });

                documents.ForEach(doc =>
                {
                    _logger.LogInformation($"SectionTitle: {doc.SectionTitle}, ParagraphText: {doc.ParagraphText}, DocumentName: {doc.DocumentName}");
                });
                return documents;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error during search: {ex.Message}");
                return new List<(string DocumentName, string SectionTitle, string ParagraphText)> { ("Error", "Error", "Error during search") };
            }
        }

        */

        /*
        public async Task<List<(string SectionTitle, string ParagraphText, string DocumentName)>> FindSimilarDocumentsAsync(double[] embedding, string userInput)
        {
            _logger.LogInformation($"Entered FindSimilarDocumentsAsync method with userInput: {userInput}");
            try
            {
                var documents = new List<(string SectionTitle, string ParagraphText, string DocumentName)>();

                // Debug: Print collection info
                var collectionStats = await _collection.Database.RunCommandAsync<BsonDocument>(new BsonDocument("collStats", _collection.CollectionNamespace.CollectionName));
                _logger.LogInformation($"Collection stats: {collectionStats.ToJson()}");

                // 1. Direct match search with case-insensitive regex
                var filter = Builders<BsonDocument>.Filter.Regex("SectionTitle", new BsonRegularExpression(userInput, "i"));
                _logger.LogInformation($"Executing direct match search with filter: {filter.ToJson()}");

                var directMatchResult = await _collection.Find(filter).ToListAsync();
                _logger.LogInformation($"Direct match results count: {directMatchResult.Count}");

                foreach (var doc in directMatchResult)
                {
                    _logger.LogInformation($"Direct match document: {doc.ToJson()}");

                    // Safely get values with null checking
                    string sectionTitle = doc.Contains("SectionTitle") ? doc["SectionTitle"].ToString() : "N/A";
                    string paragraphText = doc.Contains("ParagraphText") ? doc["ParagraphText"].ToString() : "N/A";
                    string testName = doc.Contains("SectionTitle") ? doc["SectionTitle"].ToString() : "N/A";

                    _logger.LogInformation($"Extracted values - SectionTitle: {sectionTitle}, ParagraphText: {paragraphText}, TestName: {testName}");
                    documents.Add((sectionTitle, paragraphText, testName));
                }

                // If direct matches found, return them
                if (documents.Count > 0)
                {
                    _logger.LogInformation($"Returning {documents.Count} direct matches");
                    return documents;
                }

                // 2. Vector search
                _logger.LogInformation("No direct matches found, proceeding with vector search");
                _logger.LogInformation($"Embedding vector length: {embedding.Length}");

                var pipeline = new BsonDocument[]
                {
            new BsonDocument("$vectorSearch", new BsonDocument
            {
                { "queryVector", new BsonArray(embedding) },
                { "path", "plot_embedding" },
                { "numCandidates", 200 },
                { "limit", 10 },
                { "index", "AuditDoc" }
            })
                };

                _logger.LogInformation($"Vector search pipeline: {pipeline[0].ToJson()}");

                try
                {
                    var result = await _collection.AggregateAsync<BsonDocument>(pipeline);

                    await result.ForEachAsync(doc =>
                    {
                        _logger.LogInformation($"Vector search document: {doc.ToJson()}");

                        // Safely get values with null checking
                        string sectionTitle = doc.Contains("SectionTitle") ? doc["SectionTitle"].ToString() : "N/A";
                        string paragraphText = doc.Contains("ParagraphText") ? doc["ParagraphText"].ToString() : "N/A";
                        string testName = doc.Contains("SectionTitle") ? doc["SectionTitle"].ToString() : "N/A";

                        _logger.LogInformation($"Extracted values - SectionTitle: {sectionTitle}, ParagraphText: {paragraphText}, TestName: {testName}");
                        documents.Add((sectionTitle, paragraphText, testName));
                    });
                }
                catch (MongoCommandException mex)
                {
                    _logger.LogError($"MongoDB command error during vector search: {mex.Message}");
                    _logger.LogError($"Command error details: {mex.Command.ToJson()}");
                    throw;
                }

                _logger.LogInformation($"Total documents found: {documents.Count}");
                return documents;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during search: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                throw new ApplicationException("Error during document search", ex);
            }
        }
        
        */
        
        public async Task<List<(string SectionTitle, string ParagraphText, string DocumentName)>> FindSimilarDocumentsAsync(double[] embedding, string userInput)
        {
            _logger.LogInformation($"Entered FindSimilarDocumentsAsync method with userInput: {userInput}");
            var documents = new List<(string SectionTitle, string ParagraphText, string DocumentName)>();

            try
            {
                // 1. Direct match search with regex for quick filtering
                var filter = Builders<BsonDocument>.Filter.Regex("SectionTitle", new BsonRegularExpression(userInput, "i"));
                _logger.LogInformation("Performing direct match search with filter: {Filter}", filter.ToJson());

                var directMatchResult = await _collection.Find(filter).ToListAsync();
                _logger.LogInformation($"Direct match results count: {directMatchResult.Count}");

                foreach (var doc in directMatchResult)
                {
                    string sectionTitle = doc.GetValue("SectionTitle", "N/A").AsString;
                    string paragraphText = doc.GetValue("ParagraphText", "N/A").AsString;
                    string documentName = doc.GetValue("DocumentName", "N/A").AsString;

                    _logger.LogInformation("Direct match document found - SectionTitle: {SectionTitle}, DocumentName: {DocumentName}", sectionTitle, documentName);

                    documents.Add((sectionTitle, paragraphText, documentName));
                }

                // If direct matches found, return them
                if (documents.Count > 0)
                {
                    _logger.LogInformation($"Returning {documents.Count} direct match documents.");
                    return documents;
                }

                // 2. Fallback to cosine similarity search if no direct matches found
                _logger.LogInformation("No direct matches found, proceeding with cosine similarity search.");

                // Retrieve all documents with embeddings
                var allDocs = await _collection.Find(Builders<BsonDocument>.Filter.Exists("plot_embedding")).ToListAsync();
                _logger.LogInformation($"Total documents retrieved with embeddings for similarity search: {allDocs.Count}");

                var resultsWithScores = new List<(double Score, string SectionTitle, string ParagraphText, string DocumentName)>();

                foreach (var doc in allDocs)
                {
                    var storedEmbeddingArray = doc["plot_embedding"].AsBsonArray;
                    double[] storedEmbedding = storedEmbeddingArray.Select(e => e.AsDouble).ToArray();
                    _logger.LogInformation($"Calculating cosine similarity for document with SectionTitle: {doc["SectionTitle"]}");

                    // Calculate cosine similarity
                    double similarityScore = CalculateCosineSimilarity(embedding, storedEmbedding);
                    _logger.LogInformation($"Calculated similarity score: {similarityScore}");

                    // Only consider documents with a high enough similarity score
                    if (similarityScore > 0.3) // You may adjust this threshold based on your needs
                    {
                        string sectionTitle = doc.GetValue("SectionTitle", "N/A").AsString;
                        string paragraphText = doc.GetValue("ParagraphText", "N/A").AsString;
                        string documentName = doc.GetValue("DocumentName", "N/A").AsString;

                        _logger.LogInformation("Document meets similarity threshold - SectionTitle: {SectionTitle}, Score: {Score}", sectionTitle, similarityScore);

                        resultsWithScores.Add((similarityScore, sectionTitle, paragraphText, documentName));
                    }
                    else
                    {
                        _logger.LogInformation("Document did not meet similarity threshold - Score: {Score}", similarityScore);
                    }
                }

                // Sort by similarity score and take the top N results
                var topResults = resultsWithScores
                    .OrderByDescending(result => result.Score)
                    .Take(100) // Limit to top 10 results
                    .Select(result => (result.SectionTitle, result.ParagraphText, result.DocumentName))
                    .ToList();

                _logger.LogInformation($"Total documents found with similarity scoring: {topResults.Count}");
                topResults.ForEach(result =>
                    _logger.LogInformation("Top result - SectionTitle: {SectionTitle}, DocumentName: {DocumentName}", result.SectionTitle, result.DocumentName));

                return topResults;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during search: {ex.Message}");
                _logger.LogError("Stack Trace: {StackTrace}", ex.StackTrace);
                throw new ApplicationException("Error during document search", ex);
            }
        }
        
        private double CalculateCosineSimilarity(double[] embedding1, double[] embedding2)
        {
            double dotProduct = 0.0;
            double magnitude1 = 0.0;
            double magnitude2 = 0.0;

            for (int i = 0; i < embedding1.Length; i++)
            {
                dotProduct += embedding1[i] * embedding2[i];
                magnitude1 += Math.Pow(embedding1[i], 2);
                magnitude2 += Math.Pow(embedding2[i], 2);
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            if (magnitude1 == 0 || magnitude2 == 0)
                return 0;

            return dotProduct / (magnitude1 * magnitude2);
        }

        public async Task<string> SaveSectionsToMongoDBAsync(List<BsonDocument> sections, string documentName)
        {
            try
            {
                // Create a top-level document to store all sections with the document name
                var document = new BsonDocument
                {
                    { "DocumentName", documentName },
                    { "Sections", new BsonArray(sections) } // Add the structured sections
                };

                // Insert the structured document into MongoDB collection
                await _collection.InsertOneAsync(document);

                return "All sections saved successfully.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving sections to MongoDB: {ex.Message}");
                return "Failed to save sections to MongoDB.";
            }
        }


    }
}
