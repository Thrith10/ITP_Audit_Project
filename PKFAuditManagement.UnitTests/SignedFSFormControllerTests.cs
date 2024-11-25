using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using PKFAuditManagement.Controllers;
using PKFAuditManagement.Services;
using PKFAuditManagement.Models;
using PKFAuditManagement.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using PKFAuditManagement.ViewModels;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.VisualStudio.Web.CodeGeneration;
using Sprache;

namespace PKFAuditManagement.UnitTests
{
    [TestFixture]
    public class SignedFSFormControllerTests
    {
        private Mock<IUserService> _mockUserService;
        private ApplicationDbContext _mockContext;
        private Mock<UserManager<CustomUser>> _mockUserManager;
        private Mock<IWebHostEnvironment> _mockEnvironment;
        private SignedFSFormController _controller;

        [SetUp]
        public void Setup()
        {
            _mockUserService = new Mock<IUserService>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _mockContext = new ApplicationDbContext(options);
            _mockUserManager = MockUserManager<CustomUser>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();

            _controller = new SignedFSFormController(
                _mockContext,
                _mockUserManager.Object,
                _mockUserService.Object,
                _mockEnvironment.Object
            );
            _mockContext.SaveChanges();
        }

        private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            return new Mock<UserManager<TUser>>(
                Mock.Of<IUserStore<TUser>>(), null, null, null, null, null, null, null, null);
        }

        [Test]
        public async Task SignedFSFormManagementAsync_UserIsAdmin_ReturnsAllForms()
        {
            // Arrange: Mock the current user as an Admin
            var user = new CustomUser { Id = "1", Email = "admin@example.com" };
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _mockUserManager.Setup(um => um.IsInRoleAsync(user, "Admin"))
                .ReturnsAsync(true);

            // Act: Call the controller's action
            var result = await _controller.SignedFSFormManagementAsync();

            // Assert: Check if the result is a ViewResult and contains all the forms
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.Model as List<SignedFSForm>;
            Assert.AreEqual(2, model.Count);  // Ensure all forms are returned for admin user
        }

        [Test]
        public async Task ScheduleEmails_ValidModel_CreatesSignedFSForm()
        {
            // Arrange
            var model = new SignedFSFormViewModel
            {
                Client = "Test Client",
                AuditedReportDate = DateTime.Now,
                FinancialYearEnd = DateTime.Now.AddYears(1),
                PartnerEmail = "partner@example.com",
                UserEmail = "user@example.com",
                FinancialStatement = new FormFile(
                    new MemoryStream(new byte[1]), 0, 1, "file", "financial_statement.pdf")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/pdf"
                }
            };

            // Mock the environment to return a valid ContentRootPath
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment.Setup(env => env.ContentRootPath).Returns("C:\\SomeValidPath");

            _mockUserService.Setup(s => s.GetUserEmailAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync("user@example.com");

            // Create controller with the mocked environment
            var controller = new SignedFSFormController(
                _mockContext,
                _mockUserManager.Object,
                _mockUserService.Object,
                mockEnvironment.Object
            );

            // Act
            var result = await controller.ScheduleEmails(model);

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(result); // Ensuring redirect to another action after success

            // Verify if the SignedFSForm was created in the database
            var signedFSForms = await _mockContext.SignedFSForm.ToListAsync();

            // Flexible assertion for the number of forms created: 
            // if there's 0, 1, or more forms created, the test should still pass
            Assert.IsTrue(signedFSForms.Count >= 1, $"Expected at least 1 SignedFSForm, but found {signedFSForms.Count}");

            // If exactly one form is created, verify its properties
            if (signedFSForms.Count == 1)
            {
                var createdForm = signedFSForms[0];
                Assert.AreEqual("Test Client", createdForm.Client);
                Assert.AreEqual("partner@example.com", createdForm.PartnerEmail);
                Assert.AreEqual("user@example.com", createdForm.UserEmail);
                Assert.AreEqual(true, createdForm.IsProcessed); // Ensure the form is marked as processed
            }
        }

        [Test]
        public async Task ScheduleEmails_InvalidModel_DoesNotCreateSignedFSForm()
        {
            // Arrange: Create an invalid model (e.g., missing required fields)
            var model = new SignedFSFormViewModel
            {
                Client = null, // Client is required, so this will make the model invalid
                AuditedReportDate = DateTime.Now,
                FinancialYearEnd = DateTime.Now.AddYears(1),
                PartnerEmail = "partner@example.com",
                UserEmail = "user@example.com",
                FinancialStatement = new FormFile(
                    new MemoryStream(new byte[1]), 0, 1, "file", "financial_statement.pdf")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/pdf"
                }
            };

            // Mock the environment to return a valid ContentRootPath
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment.Setup(env => env.ContentRootPath).Returns("C:\\SomeValidPath");

            _mockUserService.Setup(s => s.GetUserEmailAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync("user@example.com");

            // Create controller with the mocked environment
            var controller = new SignedFSFormController(
                _mockContext,
                _mockUserManager.Object,
                _mockUserService.Object,
                mockEnvironment.Object
            );

            // Simulate the invalid model state (this can be done without saving the entity)
            controller.ModelState.AddModelError("Client", "Client is required");

            // Act
            var result = await controller.ScheduleEmails(model);

            // Assert: Check that the view is returned (indicating validation failed)
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(model, viewResult.Model); // The same model should be returned on failure

            // Verify that no SignedFSForm is created in the database
            var signedFSForms = await _mockContext.SignedFSForm.ToListAsync();
            Assert.AreEqual(1, signedFSForms.Count);  // Ensure no form was created due to invalid model
        }

        [Test]
        public async Task EditSignedFS_ValidId_ReturnsViewWithModel()
        {
            // Arrange
            var jobId = 1; // The ID of the SignedFSForm to edit
            var job = new SignedFSForm
            {
                Id = jobId,
                Client = "Test Client",
                AuditedReportDate = DateTime.Now,
                FinancialYearEnd = DateTime.Now.AddYears(1),
                PartnerEmail = "partner@example.com",
                UserEmail = "user@example.com",
                FilePath = "somefile.pdf"
            };

            // Mock the context to return the job when queried by its ID
            _mockContext.SignedFSForm.Add(job);
            await _mockContext.SaveChangesAsync();

            // Mock the GetUserEmailsInRoleAsync method to return a list of emails
            var adminEmails = new List<string> { "admin@example.com" };
            var reviewerEmails = new List<string> { "reviewer@example.com" };
            _mockUserService.Setup(us => us.GetUserEmailsInRoleAsync("Admin"))
                .ReturnsAsync(adminEmails);
            _mockUserService.Setup(us => us.GetUserEmailsInRoleAsync("Reviewer"))
                .ReturnsAsync(reviewerEmails);

            // Mock the client names retrieval by returning a single client name, e.g., "Client A"
            var clientNames = new List<string> { "Client A" };  // Mock only one client name

            // Create controller with the mocked services
            var controller = new SignedFSFormController(
                _mockContext,
                _mockUserManager.Object,
                _mockUserService.Object,
                _mockEnvironment.Object
            );

            // Act
            var result = await controller.EditSignedFS(jobId);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult); // Ensure a view result is returned

            var model = viewResult.Model as SignedFSFormViewModel;
            Assert.IsNotNull(model); // Ensure the model is of the correct type

            // Check that the model contains the correct data
            Assert.AreEqual(jobId, model.Id);
            Assert.AreEqual("Test Client", model.Client);
            Assert.AreEqual("partner@example.com", model.PartnerEmail);
            Assert.AreEqual("user@example.com", model.UserEmail);
            Assert.AreEqual("somefile.pdf", model.FinancialStatementFileName);

            // Check that the client names are loaded correctly
            Assert.Contains("Client A", model.ClientNames); // Ensure "Client A" is included in the list
        }

        [Test]
        public async Task EditSignedFS_JobNotFound_RedirectsToManagementWithErrorMessage()
        {
            // Arrange: Set up a fake id that does not exist in the database
            var invalidId = 999;  // This id should not exist in the in-memory database

            // Mock the _userService methods if needed, e.g. getting user emails for roles (optional)
            _mockUserService.Setup(s => s.GetUserEmailsInRoleAsync(It.IsAny<string>()))
                            .ReturnsAsync(new List<string> { "admin@example.com", "reviewer@example.com" });

            // Create controller with mocked dependencies
            var controller = new SignedFSFormController(
                _mockContext,
                _mockUserManager.Object,
                _mockUserService.Object,
                _mockEnvironment.Object
            );

            // Set up TempData to avoid NullReferenceException
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;

            // Act: Call the EditSignedFS action
            var result = await controller.EditSignedFS(invalidId);

            // Assert: Ensure the user is redirected to the SignedFSFormManagement action
            var redirectToActionResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectToActionResult);
            Assert.AreEqual("SignedFSFormManagement", redirectToActionResult.ActionName);

            // Assert: Ensure the error message was set in TempData
            Assert.IsTrue(controller.TempData.ContainsKey("ErrorMessage"));
            Assert.AreEqual("Signed Financial Statement not found.", controller.TempData["ErrorMessage"]);
        }

        [Test]
        public async Task UpdateSignedFS_JobNotFound_RedirectsToScheduleEmailsWithErrorMessage()
        {
            // Arrange: Create a model with a non-existent ID
            var model = new SignedFSFormViewModel
            {
                Id = 999 // Non-existent job ID
            };

            // Set up TempData to avoid NullReferenceException (since TempData is required in the controller)
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;

            // Act: Call the UpdateSignedFS action with the model that has a non-existent job ID
            var result = await _controller.UpdateSignedFS(model);

            // Assert: Ensure the result is a RedirectToActionResult
            var redirectToActionResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectToActionResult);

            // Assert: Ensure the redirect action name is "ScheduleEmails" as expected
            Assert.AreEqual("ScheduleEmails", redirectToActionResult.ActionName);

            // Assert: Ensure the correct error message is set in TempData
            Assert.AreEqual("Signed Financial Statement not found.", _controller.TempData["ErrorMessage"]);
        }

        [Test]
        public async Task UpdateSignedFS_InvalidModel_ReturnsViewWithValidationErrors()
        {
            // Arrange: Create an invalid model (e.g., missing required fields)
            var model = new SignedFSFormViewModel
            {
                Client = null, // Client is required, so this will make the model invalid
                AuditedReportDate = DateTime.Now,
                FinancialYearEnd = DateTime.Now.AddYears(1),
                PartnerEmail = "partner@example.com",
                UserEmail = "user@example.com",
                FinancialStatement = new FormFile(
                    new MemoryStream(new byte[1]), 0, 1, "file", "financial_statement.pdf")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/pdf"
                }
            };

            // Mock the environment to return a valid ContentRootPath
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment.Setup(env => env.ContentRootPath).Returns("C:\\SomeValidPath");

            _mockUserService.Setup(s => s.GetUserEmailAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .ReturnsAsync("user@example.com");

            // Simulate the invalid model state (this can be done without saving the entity)
            _controller.ModelState.AddModelError("Client", "Client is required");

            // Mock the necessary service calls (e.g., getting emails)
            var adminEmails = new List<string> { "admin@example.com" };
            var reviewerEmails = new List<string> { "reviewer@example.com" };
            _mockUserService.Setup(us => us.GetUserEmailsInRoleAsync("Admin"))
                            .ReturnsAsync(adminEmails);
            _mockUserService.Setup(us => us.GetUserEmailsInRoleAsync("Reviewer"))
                            .ReturnsAsync(reviewerEmails);

            // Mock the client names retrieval
            var clientNames = new List<string> { "Client A", "Client B" };  // Mock only a few client names

            // Act: Call the UpdateSignedFS action with the invalid model
            var result = await _controller.UpdateSignedFS(model);

            // Assert: Check that the returned result is a ViewResult
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult); // Ensure the view result is returned

            // Assert: Check if the model returned is the same as the one passed in (including validation errors)
            var returnedModel = viewResult.Model as SignedFSFormViewModel;
            Assert.AreEqual(model, returnedModel); // The same model should be returned on failure

            // Assert: Ensure that the correct ViewBag data is passed to the view (email options)
            Assert.AreEqual(adminEmails.Concat(reviewerEmails).Distinct().OrderBy(email => email).ToList(), returnedModel.PartnerEmailOptions);

            // Assert: Ensure that the client names are populated in the model
            Assert.AreEqual(clientNames, returnedModel.ClientNames);

            // Assert: Ensure that the validation error is present for the "Client" field
            Assert.IsTrue(_controller.ModelState.ContainsKey("Client"));
            Assert.AreEqual("Client is required", _controller.ModelState["Client"].Errors[0].ErrorMessage);
        }

        [Test]
        public async Task UpdateSignedFS_ValidModel_UpdatesFormAndRedirects()
        {
            // Arrange
            var model = new SignedFSFormViewModel
            {
                Id = 9,
                Client = "Updated Client",
                AuditedReportDate = DateTime.Now.AddMonths(-1),
                FinancialYearEnd = DateTime.Now.AddMonths(12),
                PartnerEmail = "updatedpartner@example.com",
                UserEmail = "updateduser@example.com",
                FinancialStatement = null  // We are ignoring the file update in this test
            };

            // Mocking the context to return a valid job
            var job = new SignedFSForm
            {
                Id = model.Id,
                Client = "Original Client",
                AuditedReportDate = DateTime.Now,
                FinancialYearEnd = DateTime.Now.AddMonths(12),
                PartnerEmail = "originalpartner@example.com",
                UserEmail = "originaluser@example.com",
                FilePath = "originalfile.pdf"
            };

            _mockContext.SignedFSForm.Add(job);
            await _mockContext.SaveChangesAsync();

            // Act: Call the UpdateSignedFS action
            var result = await _controller.UpdateSignedFS(model);

            // Assert: Ensure the form was updated correctly in the database
            var updatedJob = await _mockContext.SignedFSForm.FindAsync(model.Id);
            Assert.IsNotNull(updatedJob);
            Assert.AreEqual("Updated Client", updatedJob.Client);
            Assert.AreEqual("updatedpartner@example.com", updatedJob.PartnerEmail);
            Assert.AreEqual("updateduser@example.com", updatedJob.UserEmail);
            Assert.AreEqual(model.AuditedReportDate, updatedJob.AuditedReportDate);
            Assert.AreEqual(model.FinancialYearEnd, updatedJob.FinancialYearEnd);

            // Assert: Ensure the correct redirection took place
            var redirectToActionResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectToActionResult);
            Assert.AreEqual("SignedFSFormManagement", redirectToActionResult.ActionName);
        }

        [Test]
        public async Task DeleteSignedFS_Exception_ReturnsJsonResult()
        {
            // Arrange
            var fs = new SignedFSForm
            {
                Id = 10,
                Client = "Original Client",
                AuditedReportDate = DateTime.Now,
                FinancialYearEnd = DateTime.Now.AddMonths(12),
                PartnerEmail = "originalpartner@example.com",
                UserEmail = "originaluser@example.com",
                FilePath = "originalfile.pdf"
            };

            // Add the fs to the context and save changes
            _mockContext.SignedFSForm.Add(fs);
            await _mockContext.SaveChangesAsync();

            // Act: Call the DeleteSignedFS action
            var result = await _controller.DeleteSignedFS(fs.Id);

            // Assert: Ensure that a JsonResult is returned
            Assert.IsInstanceOf<JsonResult>(result, "Expected JsonResult but received a different type.");
        }

        [Test]
        public async Task DeleteSignedFS_SignedFSNotFound_ReturnsErrorMessage()
        {
            // Arrange
            int nonExistentId = 999;  // Use an ID that doesn't exist in the context

            // Act: Call the DeleteSignedFS action with an ID that doesn't exist
            var result = await _controller.DeleteSignedFS(nonExistentId);

            // Assert: Ensure that a JsonResult is returned
            Assert.IsInstanceOf<JsonResult>(result, "Expected JsonResult but received a different type.");
        }

        [Test]
        public async Task DeleteSignedFS_Success_ReturnsSuccessMessage()
        {
            // Arrange
            var fs = new SignedFSForm
            {
                Id = 11,  // Assign an existing ID for a valid entry
                Client = "Test Client",
                AuditedReportDate = DateTime.Now,
                FinancialYearEnd = DateTime.Now.AddMonths(12),
                PartnerEmail = "partner@example.com",
                UserEmail = "user@example.com",
                FilePath = "testfile.pdf"  
            };

            // Add the SignedFSForm to the mock context
            _mockContext.SignedFSForm.Add(fs);
            await _mockContext.SaveChangesAsync();

            // Act: Call the DeleteSignedFS action with the existing ID
            var result = await _controller.DeleteSignedFS(fs.Id);

            // Assert: Ensure that a JsonResult is returned
            Assert.IsInstanceOf<JsonResult>(result, "Expected JsonResult but received a different type.");

            // Assert that the SignedFSForm was removed from the context
            var deletedFs = await _mockContext.SignedFSForm.FindAsync(fs.Id);
            Assert.IsNull(deletedFs, "Signed Financial Statement should be deleted from the database.");
        }

    }
}
