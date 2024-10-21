namespace PKFAuditManagement.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LangChain.Databases.Sqlite;
    using LangChain.DocumentLoaders;
    using LangChain.Extensions;
    using LangChain.Providers.OpenAI;
    using LangChain.Providers.OpenAI.Predefined;
    using LangChain.Splitters.Text;

    public class DocumentService
    {
        private readonly SqLiteVectorDatabase _vectorDatabase;
        private readonly TextEmbeddingV3SmallModel _embeddingModel;

        public DocumentService(OpenAiProvider provider)
        {
            // Initialize the vector database
            _vectorDatabase = new SqLiteVectorDatabase("vectors.db");
            _embeddingModel = new TextEmbeddingV3SmallModel(provider);
        }

        // Combined method to either add documents or retrieve similar documents
        public async Task<ManageDocumentsResult> ManageDocumentsAsync(string pdfPath = null, string userInput = null, int amount = 5, string collectionName = "EP200")
        {
            pdfPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "RAGDocuments", "ep200.pdf");

            // Add documents from the specified PDF
            var vectorCollection = await _vectorDatabase.AddDocumentsFromAsync<PdfPigPdfLoader>(
                _embeddingModel,
                dimensions: 1536,
                dataSource: DataSource.FromPath(pdfPath),
                collectionName: collectionName,
                textSplitter: new RecursiveCharacterTextSplitter(chunkSize: 500, chunkOverlap: 200)
            );

            // Prepare the result
            var result = new ManageDocumentsResult
            {
                Message = "Documents added successfully.",
                SimilarDocuments = ""
            };

            // If userInput is provided, get similar documents
            if (!string.IsNullOrEmpty(userInput))
            {
                var similarDocuments = await vectorCollection.GetSimilarDocuments(_embeddingModel, userInput, amount);
                result.SimilarDocuments = similarDocuments.AsString();
            }

            return result;
        }
    }

    // Custom return type to encapsulate the results
    public class ManageDocumentsResult
    {
        public string Message { get; set; }
        public string SimilarDocuments { get; set; }
    }
}
