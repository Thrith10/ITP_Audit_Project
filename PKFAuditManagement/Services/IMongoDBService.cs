using DocumentFormat.OpenXml.Office2013.Word;
using MongoDB.Bson;
using MongoDB.Driver;

namespace PKFAuditManagement.Services
{
    public interface IMongoDBService
    {
        Task<List<(string SectionTitle, string ParagraphText)>> FindSimilarDocumentsAsync(double[] embedding, string userInput);
        Task<string> SaveParagraphsToMongoDBAsync(List<(string SectionTitle, string Chunk)> paragraphs, string documentName);
        Task<string> SaveSectionsToMongoDBAsync(List<BsonDocument> sections, string documentName);
    }

    public class MongoDBService : IMongoDBService
    {
        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly IEmbeddingService _embeddingService;

        public MongoDBService(string connectionString, IEmbeddingService embeddingService)
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
            _collection = _database.GetCollection<BsonDocument>("v6");
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

        public async Task<List<(string SectionTitle, string ParagraphText)>> FindSimilarDocumentsAsync(double[] embedding, string userInput)
        {
            try
            {
                var documents = new List<(string SectionTitle, string ParagraphText)>();

                // First, try to find all paragraphs with the exact or partial match for the SectionTitle in MongoDB
                var filter = Builders<BsonDocument>.Filter.Regex("SectionTitle", new BsonRegularExpression(userInput, "i"));
                var directMatchResult = await _collection.Find(filter).ToListAsync();

                directMatchResult.ForEach(doc =>
                {
                    var sectionTitle = doc["SectionTitle"].ToString();
                    var paragraphText = doc["ParagraphText"].ToString();
                    documents.Add((sectionTitle, paragraphText));
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
                    var sectionTitle = doc["SectionTitle"].ToString();
                    var paragraphText = doc["ParagraphText"].ToString();
                    documents.Add((sectionTitle, paragraphText));
                });

                return documents;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during search: {ex.Message}");
                return new List<(string SectionTitle, string ParagraphText)> { ("Error", "Error during search") };
            }
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
