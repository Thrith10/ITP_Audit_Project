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
using System.Reflection;

namespace PKFAuditManagement.UnitTests
{
    [TestFixture]
    public class QuizzesControllerTests
    {
        private Mock<ApplicationDbContext> _mockContext;
        private Mock<UserManager<CustomUser>> _mockUserManager;
        private QuizzesController _controller;
        private ApplicationDbContext _dbContext;


        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB for each test
                .Options;

            _dbContext = new ApplicationDbContext(options);

            _mockUserManager = MockUserManager<CustomUser>();
            _controller = new QuizzesController(_dbContext, _mockUserManager.Object);

            // Seed users
            _dbContext.Users.AddRange(
                new CustomUser { Id = "1", Email = "user1@example.com" },
                new CustomUser { Id = "2", Email = "user2@example.com" }
            );

            // Seed quizzes
            var quiz1 = new Quiz
            {
                QuizID = Guid.NewGuid(),
                Title = "Quiz 1",
                CreatedBy = "1",
                Description = "First Quiz",
                QuizStart = DateTime.Now.AddDays(-1),
                QuizEnd = DateTime.Now.AddHours(1)
            };
            var quiz2 = new Quiz
            {
                QuizID = Guid.NewGuid(),
                Title = "Quiz 2",
                CreatedBy = "1",
                Description = "Second Quiz",
                QuizStart = DateTime.Now.AddDays(-2),
                QuizEnd = DateTime.Now.AddHours(2)
            };
            _dbContext.Quiz.AddRange(quiz1, quiz2);

            // Seed participants
            _dbContext.Participants.AddRange(
                new Participants { QuizID = quiz1.QuizID, UserID = "1", IsRequired = true },
                new Participants { QuizID = quiz2.QuizID, UserID = "2", IsRequired = true }
            );

            _dbContext.SaveChanges();

            // Validate seeding (debugging step)
            if (!_dbContext.Quiz.Any() || !_dbContext.Users.Any())
            {
                throw new InvalidOperationException("Database seeding failed: Quizzes or Users are missing.");
            }
        }


        private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            return new Mock<UserManager<TUser>>(
                Mock.Of<IUserStore<TUser>>(), null, null, null, null, null, null, null, null);
        }
        [Test]
        public void Create_ReturnsQuizViewModelWithDefaultValues()
        {
            // Act
            var result = _controller.Create();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);

            var viewResult = result as ViewResult;
            var model = viewResult.Model as QuizViewModel;

            Assert.NotNull(model);
            Assert.AreEqual(1, model.Questions.Count); // Default question is added
            Assert.NotNull(model.Questions[0].Options);
        }
       

        [Test]
        public async Task ViewAllQuiz_ReturnsQuizzesForCurrentUser()
        {
            // Arrange
            var mockUser = new CustomUser { Id = "1", Email = "user1@example.com" };
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(mockUser);

            // Clear existing data to avoid conflicts
            _dbContext.Quiz.RemoveRange(_dbContext.Quiz);
            _dbContext.SaveChanges();

            // Seed quizzes with distinct `CreatedBy` values
            _dbContext.Quiz.AddRange(
                new Quiz
                {
                    QuizID = Guid.NewGuid(),
                    Title = "User's Quiz",
                    CreatedBy = "1", // Matches current user
                    Description = "Description for User's Quiz",
                    QuizStart = DateTime.Now,
                    QuizEnd = DateTime.Now.AddHours(1)
                },
                new Quiz
                {
                    QuizID = Guid.NewGuid(),
                    Title = "Another User's Quiz",
                    CreatedBy = "2", // Different user
                    Description = "Description for Another User's Quiz",
                    QuizStart = DateTime.Now,
                    QuizEnd = DateTime.Now.AddHours(1)
                },
                new Quiz
                {
                    QuizID = Guid.NewGuid(),
                    Title = "Yet Another User's Quiz",
                    CreatedBy = "3", // Different user
                    Description = "Description for Yet Another User's Quiz",
                    QuizStart = DateTime.Now,
                    QuizEnd = DateTime.Now.AddHours(1)
                }
            );
            _dbContext.SaveChanges();

            // Act
            var result = await _controller.ViewAllQuiz();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);

            var viewResult = result as ViewResult;
            var model = viewResult.Model as QuizListViewModel;

            Assert.NotNull(model);
            Assert.AreEqual(1, model.Quizzes.Count); // Only one quiz should match
            Assert.AreEqual("User's Quiz", model.Quizzes[0].Title);
        }


        [Test]
        public void Create_Get_ReturnsViewWithDefaultQuizViewModel()
        {
            // Act
            var result = _controller.Create();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);

            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);

            var model = viewResult.Model as QuizViewModel;
            Assert.NotNull(model);
            Assert.AreEqual(1, model.Questions.Count); // One default question
            Assert.NotNull(model.Questions[0].Options);
        }

        [Test]
        public async Task ViewAllQuiz_AuthenticatedUser_ReturnsUserQuizzes()
        {
            // Arrange
            var mockUser = new CustomUser { Id = "1", Email = "user1@example.com" };
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(mockUser);

            // Act
            var result = await _controller.ViewAllQuiz();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);

            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);

            var model = viewResult.Model as QuizListViewModel;
            Assert.NotNull(model);

            // Validate the quizzes returned
            Assert.AreEqual(2, model.Quizzes.Count); // Two quizzes created by user 1
            Assert.IsTrue(model.Quizzes.All(q => q.Title.StartsWith("Quiz")));
            Assert.IsTrue(model.Quizzes.Any(q => q.Title == "Quiz 1"));
            Assert.IsTrue(model.Quizzes.Any(q => q.Title == "Quiz 2"));
        }
        [Test]
        public async Task ViewAllQuiz_UnauthenticatedUser_ReturnsUnauthorized()
        {
            // Arrange
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((CustomUser)null);

            // Act
            var result = await _controller.ViewAllQuiz();

            // Assert
            Assert.IsInstanceOf<UnauthorizedResult>(result);
        }
        [Test]
        public async Task ViewAllQuiz_NoQuizzesForUser_ReturnsEmptyList()
        {
            // Arrange
            var mockUser = new CustomUser { Id = "3", Email = "newuser@example.com" }; // User with no quizzes
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(mockUser);

            // Act
            var result = await _controller.ViewAllQuiz();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);

            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);

            var model = viewResult.Model as QuizListViewModel;
            Assert.NotNull(model);

            // Validate the quizzes returned
            Assert.AreEqual(0, model.Quizzes.Count); // No quizzes for this user
        }
        [Test]
        public async Task Edit_Get_ValidQuizId_ReturnsViewWithQuizViewModel()
        {
            // Arrange
            var quizId = _dbContext.Quiz.FirstOrDefault()?.QuizID;
            Assert.NotNull(quizId); // Ensure quiz exists

            // Act
            var result = await _controller.Edit(quizId.Value);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);

            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);

            var model = viewResult.Model as QuizViewModel;
            Assert.NotNull(model);

            Assert.AreEqual("Quiz 1", model.Title); // Validate quiz title
        }

        [Test]
        public async Task Edit_Get_InvalidQuizId_ReturnsNotFound()
        {
            // Arrange
            var invalidQuizId = Guid.NewGuid(); // Generate a random ID

            // Act
            var result = await _controller.Edit(invalidQuizId);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Edit_Post_InvalidQuizId_ReturnsNotFound()
        {
            // Arrange
            var invalidQuizViewModel = new QuizViewModel
            {
                QuizID = Guid.NewGuid(), // An ID not in the database
                Title = "Nonexistent Quiz"
            };

            // Act
            var result = await _controller.Edit(invalidQuizViewModel);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }
        [Test]
        public async Task Edit_Post_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var quiz = _dbContext.Quiz.First();
            var quizViewModel = new QuizViewModel
            {
                QuizID = quiz.QuizID,
                Title = "" // Invalid (required field is empty)
            };

            _controller.ModelState.AddModelError("Title", "Title is required");

            // Act
            var result = await _controller.Edit(quizViewModel);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);

            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);

            var model = viewResult.Model as QuizViewModel;
            Assert.NotNull(model);
            Assert.AreEqual(quiz.QuizID, model.QuizID); // Ensure the model is preserved
        }



    }

}
