using Amazon.S3;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using NuGet.ProjectModel;
using PKFAuditManagement.Controllers;
using PKFAuditManagement.Data;
using PKFAuditManagement.Interface;
using PKFAuditManagement.Models;
using PKFAuditManagement.Services;
using PKFAuditManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PKFAuditManagement.UnitTests
{
    [TestFixture]
    public class QC6FormControllerTest
    {
        private Mock<IUserService> _mockUserService;
        private ApplicationDbContext _mockContext;
        private Mock<UserManager<CustomUser>> _mockUserManager;
        private Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<Interface.IEmailSender> _mockEmailSender;
        private Mock<IWebHostEnvironment> _mockEnvironment;
        private Mock<IS3Service> _mockS3Service;
        private Mock<IFormFile> _mockFile;
        private QC6FormController _controller;
        private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            return new Mock<UserManager<TUser>>(
                Mock.Of<IUserStore<TUser>>(), null, null, null, null, null, null, null, null);
        }

        private static Mock<RoleManager<TRole>> MockRoleManager<TRole>() where TRole : class
        {
            return new Mock<RoleManager<TRole>>(
                Mock.Of<IRoleStore<TRole>>(), null, null, null, null);
        }

        [SetUp]
        public void Setup()
        {
            // Mocks
            _mockUserService = new Mock<IUserService>();
            _mockFile = new Mock<IFormFile>();

            // Set up in-memory database options for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabaseQC6")
                .Options;

            var dbContext = new ApplicationDbContext(options);

            // Mock UserManager and RoleManager
            _mockUserManager = MockUserManager<CustomUser>();
            _mockRoleManager = MockRoleManager<IdentityRole>();

            // Mock other dependencies
            _mockConfiguration = new Mock<IConfiguration>();
            _mockEmailSender = new Mock<Interface.IEmailSender>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();

            // Mock S3 service (AmazonS3)
            _mockS3Service = new Mock<IS3Service>();

            // Create controller instance with mocked dependencies
            _controller = new QC6FormController(
                _mockUserService.Object,
                dbContext,
                _mockUserManager.Object,
                _mockRoleManager.Object,
                _mockEmailSender.Object,
                _mockEnvironment.Object,
                _mockS3Service.Object // Pass the mocked S3 service
            );

            // Seed the in-memory database with QC6Form data
            dbContext.QC6Forms.AddRange(new List<QC6Form>
        {
            new QC6Form
            {
                ProspectiveClient = "Client A",
                IsTemplate = false,
                TypeOfActivities = "Audit",
                Status = "Pending",
                SourceOfReferral = "Direct",
                PKFEntityProposingService = "Audit Service",
                NatureOfServiceForEstimateFee = "Audit Fee Estimate",
                Industry = "Tech",
                GrandTotal = 5000.00m,
                FileReference = "QC6File001",
                FeeConcentration = 2.0m,
                EstimatedFee = 2500.00m,
                EngagementType = "New Engagement",
                CreatedBy = "User1",
                ComplexityOfEngagement = "Complex",
                BudgetedTimeCost = 1200.00m,
                BudgetedFeeRecoveryRate = 1.25m,
                AuditFee = 2400.00m,
                FormSubmissionDate = DateTime.Now,
                OutstandingUnpaidFees = true,
                ConflictsCheckDone = true,
                PredecessorAuditor = true,
                PublicInterestEntity = false,
                IsSubForm2NotApplicable = false,
                IsSubForm3NotApplicable = false
            },
            new QC6Form
            {
                ProspectiveClient = "Client B",
                IsTemplate = false,
                TypeOfActivities = "Consulting",
                Status = "Completed",
                SourceOfReferral = "Referral",
                PKFEntityProposingService = "Consulting Service",
                NatureOfServiceForEstimateFee = "Consulting Fee Estimate",
                Industry = "Finance",
                GrandTotal = 10000.00m,
                FileReference = "QC6File002",
                FeeConcentration = 3.0m,
                EstimatedFee = 5000.00m,
                EngagementType = "Ongoing Engagement",
                CreatedBy = "User2",
                ComplexityOfEngagement = "Moderate",
                BudgetedTimeCost = 2500.00m,
                BudgetedFeeRecoveryRate = 1.5m,
                AuditFee = 4500.00m,
                FormSubmissionDate = DateTime.Now.AddDays(-1),
                OutstandingUnpaidFees = false,
                ConflictsCheckDone = true,
                PredecessorAuditor = false,
                PublicInterestEntity = true,
                IsSubForm2NotApplicable = true,
                IsSubForm3NotApplicable = false
            }
        });

            // Save the data to the in-memory database
            dbContext.SaveChanges();
        }

        [Test]
        public async Task QC6FormManagement_ReturnsViewWithCorrectModel_WhenUserIsFound()
        {
            // Arrange
            var mockUser = new CustomUser
            {
                Id = "User1",
                UserName = "testuser"
            };

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "User1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync(mockUser);

            // Act
            var result = await _controller.QC6FormManagement();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as List<QC6Form>;
            Assert.IsNotNull(model);
        }


        [Test]
        public async Task QC6FormManagement_ReturnsNotFound_WhenUserIsNotFound()
        {
            // Arrange
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync((CustomUser)null); // Return null to simulate a user not being found

            // Act
            var result = await _controller.QC6FormManagement();

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>()); // Verify the result type
            var notFoundResult = (NotFoundObjectResult)result; // Cast to access the Value property
            Assert.AreEqual("User not found.", notFoundResult.Value); // Verify the error message
        }

        [Test]
        public async Task EditQC6Form_UserNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var formId = 1;
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync((CustomUser)null); // Simulate user not found

            // Act
            var result = await _controller.EditQC6Form(formId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual("User not found.", notFoundResult.Value);
        }

        [Test]
        public void GetProspectiveClients_ReturnsMatchingClients()
        {
            // Arrange
            string searchTerm = "Client";

            // Act
            var result = _controller.GetProspectiveClients(searchTerm) as JsonResult;

            // Assert
            Assert.IsNotNull(result);
            var clientList = result.Value as List<string>;
            Assert.IsNotNull(clientList);
            Assert.Contains("Client A", clientList);
            Assert.Contains("Client B", clientList);
        }

        [Test]
        public void GroupIndustries_ReturnsCorrectGroups_WhenFileHasValidData()
        {
            // Arrange
            var industriesFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "SSIC_Codes.txt");
            var fileContents = new[]
            {
            "A AGRICULTURE AND FISHING",
            "01 Crop Production",
            "02 Animal Production",
            "B MINING AND QUARRYING",
            "05 Mining of coal and lignite",
            "C MANUFACTURING",
            "10 Manufacture of food products"
        };
            Directory.CreateDirectory(Path.GetDirectoryName(industriesFilePath));
            File.WriteAllLines(industriesFilePath, fileContents);

            // Act
            var result = _controller.GroupIndustries();

            // Assert
            Assert.AreEqual(3, result.Count); // 3 groups: A, B, C
            Assert.AreEqual("A AGRICULTURE AND FISHING", result[0].Key);
            Assert.AreEqual(2, result[0].Value.Count);
            Assert.AreEqual("01 Crop Production", result[0].Value[0]);
            Assert.AreEqual("02 Animal Production", result[0].Value[1]);
        }

        [Test]
        public void GroupIndustries_ReturnsEmptyList_WhenFileIsEmpty()
        {
            // Arrange
            var industriesFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "SSIC_Codes.txt");
            File.WriteAllText(industriesFilePath, string.Empty);
            // Act
            var result = _controller.GroupIndustries();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count); // No groups in the result
        }

        [Test]
        public async Task GetAllClients_ShouldReturnUniqueClientNames()
        {
            // Act
            var result = await _controller.GetAllClients();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var clientNames = okResult.Value as List<string>;
            Assert.IsNotNull(clientNames);
            Assert.Contains("Client A", clientNames);
            Assert.Contains("Client B", clientNames);
        }

        [Test]
        public void ExtractDocumentName_ShouldReturnDocumentName_WhenValidS3Key()
        {
            // Arrange
            var s3Key = "QC6FormUploads/OtherDocuments/DocumentName/c73a5d21-c0bc-4c6b-aaf5-0b20b58b16fd.pdf";

            // Act
            var result = _controller.ExtractDocumentName(s3Key);

            // Assert
            Assert.AreEqual("DocumentName", result);
        }

        [Test]
        public void ExtractDocumentName_ShouldReturnEmpty_WhenS3KeyHasInsufficientParts()
        {
            // Arrange
            var s3Key = "QC6FormUploads/OtherDocuments/";

            // Act
            var result = _controller.ExtractDocumentName(s3Key);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void ExtractDocumentName_ShouldReturnEmpty_WhenS3KeyIsEmpty()
        {
            // Arrange
            var s3Key = "";

            // Act
            var result = _controller.ExtractDocumentName(s3Key);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void ExtractDocumentName_ShouldReturnEmpty_WhenS3KeyHasNoSlash()
        {
            // Arrange
            var s3Key = "DocumentName";

            // Act
            var result = _controller.ExtractDocumentName(s3Key);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void ExtractDocumentName_ShouldReturnEmpty_WhenS3KeyHasOnlyOneSlash()
        {
            // Arrange
            var s3Key = "QC6FormUploads/";

            // Act
            var result = _controller.ExtractDocumentName(s3Key);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public async Task SaveFileAsync_ShouldCreateDirectory_WhenDirectoryDoesNotExist()
        {
            // Arrange
            var filePath = "C:/InvalidPath/testfile.txt";

            // Mock the IFormFile to simulate a file copy
            var memoryStream = new MemoryStream();
            _mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act & Assert
            var dirCreationResult = Directory.Exists(Path.GetDirectoryName(filePath));
            await _controller.SaveFileAsync(_mockFile.Object, filePath);

            // Ensure directory was created
            Assert.IsTrue(Directory.Exists(Path.GetDirectoryName(filePath)));
        }

        [Test]
        public async Task SaveFileAsync_ShouldWriteFileToDisk_WhenValidFileAndPathProvided()
        {
            // Arrange
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "testfile.txt"); // Use a valid relative path
            var memoryStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }); // Mocked file content
            var mockFile = new Mock<IFormFile>();

            // Mock the IFormFile CopyToAsync method
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .Callback<Stream, CancellationToken>((stream, token) =>
                    {
                        memoryStream.CopyTo(stream);
                    })
                    .Returns(Task.CompletedTask);

            // Act
            await _controller.SaveFileAsync(mockFile.Object, filePath);

            // Assert
            Assert.IsTrue(File.Exists(filePath)); // Ensure the file was written

            // Clean up
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [Test]
        public async Task SaveFileAsync_ShouldThrowException_WhenFileCannotBeSaved()
        {
            // Arrange
            var filePath = "C:/InvalidPath/testfile.txt";

            // Mock the IFormFile to simulate an exception during file copy
            _mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Throws(new IOException("File error"));

            // Act & Assert
            Assert.ThrowsAsync<IOException>(async () => await _controller.SaveFileAsync(_mockFile.Object, filePath));
        }
    }

}
