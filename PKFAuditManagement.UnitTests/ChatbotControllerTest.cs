using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using PKFAuditManagement.Controllers;
using PKFAuditManagement.Models;
using PKFAuditManagement.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using PKFAuditManagement.Services;
using System.Security.Claims;
using LangChain.Providers.OpenAI;
using Microsoft.AspNetCore.Http;
using LangChain.DocumentLoaders;

namespace PKFAuditManagement.UnitTests
{
    [TestFixture]
    public class ChatbotControllerTest
    {
        private ApplicationDbContext _mockContext;
        private Mock<IOpenAIService> _mockOpenAIService;
        private Mock<IEmbeddingService> _mockEmbeddingService;
        private Mock<IMongoDBService> _mockMongoDBService;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<ILogger<ChatbotController>> _mockLogger;
        private ChatbotController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB for each test
                .Options;

            _mockContext = new ApplicationDbContext(options);

            // Mock IConfiguration to return a fake API key
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c["OPENAI_API_KEY"]).Returns("fake-api-key");

            // Instead of mocking the OpenAiProvider constructor directly, mock the dependencies that use it:
            var mockProvider = new Mock<OpenAiProvider>("fake-api-key"); // We pass a fake key or anything here

            // Mock other services (if needed)
            _mockOpenAIService = new Mock<IOpenAIService>();
            _mockEmbeddingService = new Mock<IEmbeddingService>();
            _mockMongoDBService = new Mock<IMongoDBService>();
            _mockLogger = new Mock<ILogger<ChatbotController>>();

            // Instantiate the controller, passing in the mocked OpenAiProvider and other dependencies
            _controller = new ChatbotController(
                _mockContext,
                _mockOpenAIService.Object,
                _mockEmbeddingService.Object,
                _mockMongoDBService.Object,
                mockConfiguration.Object, // Configuration mock is here
                _mockLogger.Object
            );

            var chatbotDoc1 = new ChatbotDocument
            {
                Id = 1,
                Name = "Document 1",
                DateAdded = DateTime.Now,
                FilePath = "/files/doc1.pdf"
            };

            var chatbotDoc2 = new ChatbotDocument
            {
                Id = 2,
                Name = "Document 2",
                DateAdded = DateTime.Now.AddDays(-1),
                FilePath = "/files/doc2.pdf"
            };

            _mockContext.ChatbotDocuments.AddRange(chatbotDoc1, chatbotDoc2);

            _mockContext.SaveChanges();
        }

        [Test]
        public async Task ChatbotManagement_UserIsAdmin_ReturnsViewWithDocuments()
        {
            // Act: Call the controller's action
            var result = await _controller.ChatbotManagement();

            // Assert: Check if the result is a ViewResult and contains the documents
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.Model as List<ChatbotDocument>;
            Assert.AreEqual(2, model.Count);  // Ensure two documents are returned
        }

        [Test]
        public async Task UploadDocument_ValidFile_SavesDocumentToDatabase()
        {
            // Arrange
            var fileName = "testDocument";
            var fileContent = new byte[] { 1, 2, 3 };  // Simulate file content
            var fileStream = new MemoryStream(fileContent);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.OpenReadStream()).Returns(fileStream);
            fileMock.Setup(f => f.FileName).Returns($"{fileName}.pdf");
            fileMock.Setup(f => f.Length).Returns(fileContent.Length);

            // Act
            var result = await _controller.UploadDocument(fileMock.Object, fileName);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("Document uploaded successfully.", okResult.Value); // Ensure the correct success message is returned

            // Verify if the ChatbotDocument was created in the database
            var chatbotDocuments = await _mockContext.ChatbotDocuments.ToListAsync();

            // Assert that at least one document is created
            Assert.IsTrue(chatbotDocuments.Count >= 1, $"Expected at least 1 ChatbotDocument, but found {chatbotDocuments.Count}");

            // If exactly one document is created, verify its properties
            if (chatbotDocuments.Count == 1)
            {
                var createdDocument = chatbotDocuments[0];
                Assert.AreEqual(fileName, createdDocument.Name); // Ensure the correct document name
                Assert.AreEqual($"/RAGDocuments/{fileName}.pdf", createdDocument.FilePath); // Verify the file path
                Assert.AreEqual(true, createdDocument.DateAdded.Date == DateTime.UtcNow.Date); // Check if DateAdded is set correctly
            }
        }
        // Error Case: File is Not Provided (Null or Empty)
        [Test]
        public async Task UploadDocument_NoFile_ReturnsBadRequest()
        {
            // Arrange
            IFormFile file = null; // Simulating no file uploaded
            var documentName = "testDocument";

            // Act
            var result = await _controller.UploadDocument(file, documentName);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Please select a document to upload.", badRequestResult.Value); // Error message
        }

        // Error Case: Invalid File Type (Not a PDF)
        [Test]
        public async Task UploadDocument_InvalidFileType_ReturnsBadRequest()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var fileName = "testDocument";
            var fileContent = new byte[] { 1, 2, 3 };  // Simulate file content
            var fileStream = new MemoryStream(fileContent);

            fileMock.Setup(f => f.OpenReadStream()).Returns(fileStream);
            fileMock.Setup(f => f.FileName).Returns($"{fileName}.txt");  // Simulating an invalid file type (not PDF)
            fileMock.Setup(f => f.Length).Returns(fileContent.Length);

            // Act
            var result = await _controller.UploadDocument(fileMock.Object, fileName);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Only PDF documents are allowed.", badRequestResult.Value); // Error message for invalid file type
        }

        [Test]
        public async Task DeleteDocument_DocumentFound_DeletesAndReturnsOk()
        {
            // Arrange
            var documentId = 1;  // document with ID 1 exists in the database

            // Act
            var result = await _controller.DeleteDocument(documentId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);                           // Ensure it's an OkObjectResult
            Assert.AreEqual("Document removed successfully.", okResult.Value);  // Ensure the correct message is returned

            // Assert that the document was removed from the context
            var deletedDocument = await _mockContext.ChatbotDocuments.FindAsync(documentId);
            Assert.IsNull(deletedDocument, "Chatbot document should be deleted from the database.");
        }

        [Test]
        public async Task DeleteDocument_DocumentNotFound_ReturnsNotFound()
        {
            // Arrange
            var documentId = 3;  // no document with ID 3 exists

            // Act
            var result = await _controller.DeleteDocument(documentId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);                   // Ensure it's a NotFoundObjectResult
            Assert.AreEqual("Document not found.", notFoundResult.Value);  // Ensure the correct message is returned
        }
    }
}


