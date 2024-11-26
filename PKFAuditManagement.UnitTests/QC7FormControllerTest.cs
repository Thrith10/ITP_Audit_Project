using Amazon.S3;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using PKFAuditManagement.Controllers;
using PKFAuditManagement.Data;
using PKFAuditManagement.Interface;
using PKFAuditManagement.Models;
using PKFAuditManagement.Services;
using PKFAuditManagement.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PKFAuditManagement.UnitTests
{
    [TestFixture]
    public class QC7FormControllerTest
    {
        private Mock<IUserService> _mockUserService;
        private ApplicationDbContext _mockContext;
        private Mock<UserManager<CustomUser>> _mockUserManager;
        private Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<Interface.IEmailSender> _mockEmailSender;
        private Mock<IWebHostEnvironment> _mockEnvironment;
        private QC7FormController _controller;
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

            // Set up in-memory database options for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabaseQC7")
                .Options;

            var dbContext = new ApplicationDbContext(options);

            // Mock UserManager and RoleManager
            _mockUserManager = MockUserManager<CustomUser>();
            _mockRoleManager = MockRoleManager<IdentityRole>();

            // Mock other dependencies
            _mockConfiguration = new Mock<IConfiguration>();
            _mockEmailSender = new Mock<Interface.IEmailSender>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();

            // Create controller instance with mocked dependencies
            _controller = new QC7FormController(
                _mockUserService.Object,
                dbContext,
                _mockUserManager.Object,
                _mockRoleManager.Object,
                _mockEmailSender.Object,
                _mockEnvironment.Object
            );

            // Seed the in-memory database with QC7Form data
            dbContext.QC7Forms.AddRange(new List<QC7Form>
        {
            new QC7Form
            {
                CreatedBy = "John Doe",
                FileReference = "REF123",
                Client = "Example Client Ltd.",
                PeriodEnded = new DateTime(2023, 12, 31),
                EngagementType = "Audit",
                Industry = "Technology",
                Status = "Draft",
                FormSubmissionDate = new DateTime(2024, 1, 15),
                PriorYearFee = 100000m,
                TimeCosts = 50000m,
                PriorYearRecoveryRate = 80.5m,
                TypeOfClientActivities = "Software Development",
                RiskRatingPriorYear = "Low",
                AnyOutstandingUnpaidAuditFees = false,
                AnySuspiciousTransactionReportFiled = false,
                ProposedFeeCurrentYear = 120000m,
                BudgetedTimeCost = 60000m,
                ProposedRecoveryRateCurrentYear = 85.0m,
                IsPublicInterestEntity = false,
                IsSubForm2NotApplicable = true,
                IsSubForm3NotApplicable = false,
                QC7FormFeeDetails = new List<QC7FormFeeDetail>(),
                QC7FormTests = new List<QC7FormTest>(),
                QC7FormConclusion = null // Adjust if you have seeded QC7FormConclusion
            },
            new QC7Form
            {
                CreatedBy = "John",
                FileReference = "REF1234",
                Client = "Example Client2 Ltd.",
                PeriodEnded = new DateTime(2023, 12, 31),
                EngagementType = "Audit",
                Industry = "Technology",
                Status = "Draft",
                FormSubmissionDate = new DateTime(2024, 1, 15),
                PriorYearFee = 100000m,
                TimeCosts = 50000m,
                PriorYearRecoveryRate = 80.5m,
                TypeOfClientActivities = "Software Development",
                RiskRatingPriorYear = "Low",
                AnyOutstandingUnpaidAuditFees = false,
                AnySuspiciousTransactionReportFiled = false,
                ProposedFeeCurrentYear = 120000m,
                BudgetedTimeCost = 60000m,
                ProposedRecoveryRateCurrentYear = 85.0m,
                IsPublicInterestEntity = false,
                IsSubForm2NotApplicable = true,
                IsSubForm3NotApplicable = false,
                QC7FormFeeDetails = new List<QC7FormFeeDetail>(),
                QC7FormTests = new List<QC7FormTest>(),
                QC7FormConclusion = null // Adjust if you have seeded QC7FormConclusion
            }
        });

            // Save the data to the in-memory database
            dbContext.SaveChanges();
        }

        [Test]
        public async Task QC7FormManagement_ReturnsViewWithCorrectModel_WhenUserIsFound()
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
            var result = await _controller.QC7FormManagement();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as List<QC7Form>;
            Assert.IsNotNull(model);
            Assert.AreEqual(0, model.Count); 
        }


        [Test]
        public async Task QC7FormManagement_ReturnsFound_WhenViewResultIsReturned()
        {
            // Act
            var result = await _controller.QC7FormManagement();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>()); // Verify the result type
            var viewResult = (ViewResult)result; // Cast to access the ViewName or Model

            // Verify the correct view is returned
            Assert.AreEqual("~/Views/General/QC7/QC7FormManagement.cshtml", viewResult.ViewName); 
        }

        [Test]
        public async Task EditQC7Form_UserNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var formId = 1;
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync((CustomUser)null); // Simulate user not found

            // Act
            var result = await _controller.EditQC7Form(formId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual("User not found.", notFoundResult.Value);
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
        public void RetrieveNASFeeDetails_ShouldReturnNotNullForNonExistingClient()
        {
            // Arrange
            var clientName = "NonExistingClient";

            // Act
            var result = _controller.RetrieveNASFeeDetails(clientName) as JsonResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
        }

    }

}
