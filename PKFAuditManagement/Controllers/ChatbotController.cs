using LangChain.Databases.Sqlite;
using LangChain.DocumentLoaders;
using LangChain.Extensions;
using LangChain.Providers;
using LangChain.Providers.OpenAI;
using LangChain.Providers.OpenAI.Predefined;
using LangChain.Splitters.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using PKFAuditManagement.Data;
using PKFAuditManagement.Models;
using PKFAuditManagement.Services;
using PKFAuditManagement.Util;
using System.Configuration;
using System.Text;

namespace PKFAuditManagement.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OpenAiProvider _provider;
        private readonly IOpenAIService _openAIService;
        private readonly IEmbeddingService _embeddingService;
        private readonly IMongoDBService _mongoDBService;
        private readonly IConfiguration _configuration;
        private readonly TextEmbeddingV3SmallModel _embeddingModel;
        private readonly ILogger<ChatbotController> _logger;

        public ChatbotController(ApplicationDbContext context, IOpenAIService openAIService, IEmbeddingService embeddingService, IMongoDBService mongoDBService, IConfiguration configuration,
            ILogger<ChatbotController> logger)
        {
            _context = context;
            _openAIService = openAIService;
            _embeddingService = embeddingService;
            _mongoDBService = mongoDBService;
            _logger = logger;
            _configuration = configuration;
            _provider = new OpenAiProvider(_configuration["OPENAI_API_KEY"]);
            _embeddingModel = new TextEmbeddingV3SmallModel(_provider);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChatbotManagement()
        {
            // Retrieve documents from the database
            var documents = await _context.ChatbotDocuments.ToListAsync();

            return View("~/Views/Admin/ChatbotManagement/ChatbotManagement.cshtml", documents);
        }

        [HttpPost]
        public async Task<IActionResult> UploadDocument([FromForm] IFormFile file, [FromForm] string documentName)
        {
            // Check if a file is selected
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please select a document to upload.");
            }

            // Check if the file is a PDF
            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only PDF documents are allowed.");
            }

            // Check if the documentName already exists in the database
            var existingDocument = await _context.ChatbotDocuments
                .FirstOrDefaultAsync(doc => doc.Name.ToLower() == documentName.ToLower());

            if (existingDocument != null)
            {
                return BadRequest("A document with this name already exists.");
            }

            // Set the storage path to wwwroot/RAGDocuments
            var storagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "RAGDocuments");

            // Ensure the upload directory exists
            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }

            // Generate the file name
            var fileName = $"{documentName}.pdf";
            var filePath = Path.Combine(storagePath, fileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Create a new Document instance
            var document = new ChatbotDocument
            {
                Name = documentName,
                DateAdded = DateTime.UtcNow,
                FilePath = $"/RAGDocuments/{fileName}"
            };

            try
            {
                _context.Add(document);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "An error occurred while saving the document to the database");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred" );
            }

            return Ok("Document uploaded successfully.");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var document = await _context.ChatbotDocuments.FindAsync(id); 
            if (document == null)
            {
                return NotFound("Document not found.");
            }

            _context.ChatbotDocuments.Remove(document);
            await _context.SaveChangesAsync();

            return Ok("Document removed successfully.");
        }

        [HttpPost]
        public async Task<IActionResult> GetChatResponse(string userInput, string currentSelection)
        {

            //var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "RAGDocuments", "SSQM2.pdf");
            //// Read a list of paragraphs from the PDF uploaded
            //List<string> paragraphs = PdfReader.ReadPdf(filePath);

            //// Paragraphs read will be saved to MongoDB collection
            //await _mongoDBService.SaveParagraphsToMongoDBAsync(paragraphs, "SSQM2");

            _logger.LogInformation("GetChatResponse method called.");

            // Call embedding service to generate embeddings based on user input
            double[] embeddings = await _embeddingService.GenerateEmbeddingsAsync(userInput);

            // Failed to generate embeddings
            if (embeddings.Length == 0) // Check the length of the float array
            {
                return BadRequest("Failed to generate embeddings.");
            }

            // Find similar documents in MongoDB using the generated embeddings
            //List<string> similarDocuments = await _mongoDBService.FindSimilarDocumentsAsync(embeddings);

            List<(string SectionTitle, string ParagraphText, string DocumentName)> similarDocuments = 
                await _mongoDBService.FindSimilarDocumentsAsync(embeddings, userInput);

            _logger.LogInformation($"Number of similar documents retrieved: {similarDocuments.Count}");

            similarDocuments.ForEach(doc =>
            {
                _logger.LogInformation($"SectionTitle: {doc.SectionTitle}, ParagraphText: {doc.ParagraphText}, DocumentName: {doc.DocumentName}");
            });

            // Combine SectionTitle and ParagraphText for better context
            var combinedText = new StringBuilder();

            // Append a single line of the document nam
            foreach (var doc in similarDocuments)
            {
                combinedText.AppendLine($"Section: {doc.SectionTitle}\n{doc.ParagraphText}\n");
            }

            string firstDocumentName = similarDocuments.Count > 0 ? similarDocuments[0].DocumentName : "No documents found";

            // Retrieve response from LLM based on the combined input
            //var response = await _openAIService.GetChatResponseAsync(userInput, string.Join("\n", similarDocuments));
            var response = await _openAIService.GetChatResponseAsync(userInput, combinedText.ToString(), firstDocumentName.ToString());

            // Return the generated response
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> GetNewChatResponse(string userInput, string currentSelection)
        {
            var pdfPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "RAGDocuments", $"{currentSelection}.pdf");

            // Database selection based on the current selection
            SqLiteVectorDatabase _vectorDatabase;

            // Name of Db
            var dbName = $"{currentSelection}-docs.db";

            // Use the DatabaseName from the selected document to create the SqLiteVectorDatabase instance
            _vectorDatabase = new SqLiteVectorDatabase(dbName);

            // Retrieve relevant documents
            var vectorCollection = await _vectorDatabase.AddDocumentsFromAsync<PdfPigPdfLoader>(
                _embeddingModel,
                dimensions: 1536,
                dataSource: DataSource.FromPath(pdfPath),
                collectionName: "RAGDocuments",
                textSplitter: new RecursiveCharacterTextSplitter(chunkSize: 512, chunkOverlap: 200));

            var similarDocuments = await vectorCollection.GetSimilarDocuments(_embeddingModel, userInput, amount: 10);

            // Retrieve response from LLM based on the combined input
            var response = await _openAIService.GetNewChatResponseAsync(userInput, similarDocuments);

            // Return the generated response
            return Ok(response);
        }
    }
}
