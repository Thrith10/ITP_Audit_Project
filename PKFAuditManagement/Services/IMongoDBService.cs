using MongoDB.Bson;
using MongoDB.Driver;

namespace PKFAuditManagement.Services
{
    public interface IMongoDBService
    {
        Task<List<string>> FindSimilarDocumentsAsync(double[] embedding);
        Task<string> SaveParagraphsToMongoDBAsync(List<string> paragraphs, string documentName);
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
            _collection = _database.GetCollection<BsonDocument>("audit_collection");
        }

        public async Task<string> SaveParagraphsToMongoDBAsync(List<string> paragraphs, string documentName)
        {
            // Insert each paragraph into the MongoDB collection
            for (var i = 0; i < paragraphs.Count; i++)
            {
                var paragraph = paragraphs[i];

                // Call embedding service to generate embeddings based on user input
                double[] embeddings = await _embeddingService.GenerateEmbeddingsAsync(paragraph);

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
                    { "ParagraphText", paragraph },
                    { "plot_embedding", embeddingArray  }
                };

                // Asynchronously insert the document into the collection
                await _collection.InsertOneAsync(document);

                // Print the index of the inserted paragraph
                Console.Write($".{i}");
            }

            return "All paragraphs saved successfully."; // Return success message
        }

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
    }
}
