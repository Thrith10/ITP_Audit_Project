using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using PKFAuditManagement.Controllers;
using PKFAuditManagement.Services;
using PKFAuditManagement.ViewModels;
using PKFAuditManagement.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PKFAuditManagement;
using PKFAuditManagement.Models;

namespace PKFAuditManagement.UnitTests
{
    [TestFixture]
    public class QC35Test
    {
        private Mock<IUserService> _mockUserService;
        private Mock<ApplicationDbContext> _mockContext;
        private Mock<UserManager<CustomUser>> _mockUserManager;
        private Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<Interface.IEmailSender> _mockEmailSender;
        private QC35FormController _controller;

        [SetUp]
        public void Setup()
        {
            _mockUserService = new Mock<IUserService>();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            var dbContext = new ApplicationDbContext(options);
            _mockUserManager = MockUserManager<CustomUser>();
            _mockRoleManager = MockRoleManager<IdentityRole>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockEmailSender = new Mock<Interface.IEmailSender>();

            _controller = new QC35FormController(
                _mockUserService.Object,
                 dbContext,
                _mockUserManager.Object,
                _mockRoleManager.Object,
                _mockConfiguration.Object,
                _mockEmailSender.Object
            );

            // Seed the in-memory database
            dbContext.QC6Forms.AddRange(new List<QC6Form>
    {
        new QC6Form
        {
            ProspectiveClient = "Client A",
            IsTemplate = false,
            TypeOfActivities = "DefaultActivity",
            Status = "Pending",
            SourceOfReferral = "Yes",
            PKFEntityProposingService = "few",
            NatureOfServiceForEstimateFee = "some",
            Industry = "Tech",
            GrandTotal = 1.50m,
            FileReference = "Ewgwe1231",
            FeeConcentration = 1.5m,
            EstimatedFee = 1.5m,
            EngagementType = "New",
            CreatedBy = "thrith",
            ComplexityOfEngagement = "none",
            BudgetedTimeCost = 1.5m,
            BudgetedFeeRecoveryRate = 1.5m,
            AuditFee = 1.5m,
        },
        new QC6Form
        {
            ProspectiveClient = "Client B",
            IsTemplate = false,
            TypeOfActivities = "DefaultActivity",
            Status = "Pending",
            SourceOfReferral = "Yes",
            PKFEntityProposingService = "few",
            NatureOfServiceForEstimateFee = "some",
            Industry = "Tech",
            GrandTotal = 1.50m,
            FileReference = "Ewgwe1232",
            FeeConcentration = 1.5m,
            EstimatedFee = 1.5m,
            EngagementType = "New",
            CreatedBy = "thrith",
            ComplexityOfEngagement = "none",
            BudgetedTimeCost = 1.5m,
            BudgetedFeeRecoveryRate = 1.5m,
            AuditFee = 1.5m,
        }
            });
            dbContext.SaveChanges();
        }

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

        [Test]
        public async Task QC35FormCreationAsync_ReturnsViewWithViewModel()
        {
            // Arrange
            var userEmail = "testuser@example.com";
            _mockUserService.Setup(s => s.GetUserEmailAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(userEmail);

            var clientNames = new List<string> { "Client A", "Client B" };


            var adminEmails = new List<string> { "admin1@example.com" };
            var reviewerEmails = new List<string> { "reviewer1@example.com" };

            _mockUserService.Setup(s => s.GetUserEmailsInRoleAsync("Admin"))
                .ReturnsAsync(adminEmails);
            _mockUserService.Setup(s => s.GetUserEmailsInRoleAsync("Reviewer"))
                .ReturnsAsync(reviewerEmails);

            // Act
            var result = await _controller.QC35FormCreationAsync();

            // Assert that the result is a ViewResult
            Assert.IsInstanceOf<ViewResult>(result);

            // Cast the result to ViewResult
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult); // Ensure the cast was successful

            // Assert that the Model is of type QC35FormViewModel
            var viewModel = viewResult.Model as QC35FormViewModel;
            Assert.NotNull(viewModel); // Ensure the model is not null

            // Perform further assertions on the viewModel
            Assert.AreEqual(userEmail, viewModel.CreatedBy);

            // Validate client names
            var expectedClientNames = new List<string> { "Client A", "Client B" }.OrderBy(name => name).ToList();
            Assert.AreEqual(expectedClientNames, viewModel.ClientNames.OrderBy(name => name).ToList());
            
            var expectedAdminEmails = adminEmails.Concat(reviewerEmails).Distinct().OrderBy(email => email).ToList();
            Assert.AreEqual(expectedAdminEmails, viewModel.AdminEmails);

            Assert.AreEqual(3, viewModel.ChecklistItems.Count); // Assuming 3 default checklist items

        }
    }
}
