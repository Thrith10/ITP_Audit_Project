using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PKFAuditManagement.Data;
using PKFAuditManagement.Models;
using PKFAuditManagement.ViewModels;

namespace PKFAuditManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;

        public FeedbackController(ApplicationDbContext context, UserManager<CustomUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> ViewAllFeedbackForms()
        {
            var feedbackForms = await _context.FeedbackForms
                .Select(f => new FeedbackFormViewModel
                {
                    FeedbackFormID = f.FeedbackFormID,
                    Title = f.Title,
                    CreatedDate = f.CreatedDate,
                    CreatedBy = f.CreatedBy
                })
                .ToListAsync();

            var model = new FeedbackFormListViewModel
            {
                FeedbackForms = feedbackForms
            };


            return View("~/Views/General/Quiz/ViewAllFeedbackForm.cshtml", model);
        }

        // GET: Feedback/CreateFeedback
        [HttpGet]
        public IActionResult CreateFeedback()
        {
            var model = new FeedbackFormViewModel
            {
                Questions = new List<FeedbackQuestionViewModel>
                {
                    new FeedbackQuestionViewModel() // Initialize with one empty question
                }
            };

            return View("~/Views/General/Quiz/CreateFeedbackForm.cshtml", model);
        }

        // POST: Feedback/CreateFeedback
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFeedback(FeedbackFormViewModel feedbackFormViewModel)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var userId = currentUser?.Id;

                // Create a new FeedbackForm entity
                var feedbackForm = new FeedbackForm
                {
                    Title = feedbackFormViewModel.Title,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now
                };

                // Add the feedback form to the context first to generate FeedbackFormID
                _context.Add(feedbackForm);
                await _context.SaveChangesAsync();

                // Process each question from the view model
                foreach (var questionViewModel in feedbackFormViewModel.Questions)
                {
                    var feedbackQuestion = new FeedbackQuestion
                    {
                        FeedbackFormID = feedbackForm.FeedbackFormID,
                        QuestionText = questionViewModel.QuestionText,
                        Type = (Models.FeedbackType)questionViewModel.Type

                    };

                    _context.FeedbackQuestions.Add(feedbackQuestion);
                }

                // Save all questions
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Feedback form created successfully!";
                return RedirectToAction(nameof(CreateFeedback));
            }

            return View("~/Views/General/Quiz/CreateFeedbackForm.cshtml", feedbackFormViewModel);
        }
        // GET: Feedback/ViewFeedbackForm/{id}
        [HttpGet]
        public async Task<IActionResult> ViewFeedbackForm(Guid id)
        {
            var feedbackForm = await _context.FeedbackForms
                .Where(f => f.FeedbackFormID == id)
                .Select(f => new FeedbackFormViewModel
                {
                    FeedbackFormID = f.FeedbackFormID,
                    Title = f.Title,
                    CreatedDate = f.CreatedDate,
                    Questions = f.Questions.Select(q => new FeedbackQuestionViewModel
                    {
                        QuestionText = q.QuestionText,
                        Type = (ViewModels.FeedbackType)q.Type
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (feedbackForm == null)
            {
                return NotFound();
            }

            return View("~/Views/General/Quiz/ViewFeedbackForm.cshtml", feedbackForm);
        }
        // DELETE: Feedback/DeleteFeedbackForm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFeedbackForm(Guid feedbackFormID)
        {
            var feedbackForm = await _context.FeedbackForms
                .Include(f => f.Questions) // Include related questions for cascade delete
                .FirstOrDefaultAsync(f => f.FeedbackFormID == feedbackFormID);

            if (feedbackForm == null)
            {
                TempData["ErrorMessage"] = "Feedback form not found.";
                return RedirectToAction(nameof(ViewAllFeedbackForms));
            }

            // Check for linked quizzes
            var linkedQuizzesForDelete = await _context.Quiz
                .Where(q => q.FeedbackFormID == feedbackFormID)
                .Select(q => q.Title) // Get only the quiz titles for display
                .ToListAsync();

            if (linkedQuizzesForDelete.Any())
            {
                // Set TempData for linked quizzes and display message
                TempData["LinkedQuizzesForDelete"] = linkedQuizzesForDelete;
                TempData["ErrorMessage"] = "This feedback form is linked to quizzes and cannot be deleted until they are removed.";

                // Redirect to display the message
                return RedirectToAction(nameof(ViewAllFeedbackForms));
            }

            // If no linked quizzes, proceed with deletion
            _context.FeedbackForms.Remove(feedbackForm);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Feedback form deleted successfully!";

            // Clear TempData["LinkedQuizzesForDelete"] after successful deletion to avoid any residual data
            TempData.Remove("LinkedQuizzesForDelete");
            return RedirectToAction(nameof(ViewAllFeedbackForms));
        }

        [HttpGet]
        public async Task<IActionResult> EditFeedbackForm(Guid feedbackFormId)
        {
            var feedbackForm = await _context.FeedbackForms
                .Include(f => f.Questions)
                .FirstOrDefaultAsync(f => f.FeedbackFormID == feedbackFormId);

            if (feedbackForm == null)
            {
                TempData["ErrorMessage"] = "Feedback form not found.";
                return RedirectToAction(nameof(ViewAllFeedbackForms));
            }

            // Map to ViewModel
            var viewModel = new EditFeedbackFormViewModel
            {
                FeedbackFormID = feedbackForm.FeedbackFormID,
                Title = feedbackForm.Title,
                CreatedDate = feedbackForm.CreatedDate,
                CreatedBy = feedbackForm.CreatedBy,
                Questions = feedbackForm.Questions.Select(q => new FeedbackQuestionViewModel
                {
                    QuestionText = q.QuestionText,
                    Type = (ViewModels.FeedbackType)q.Type
                }).ToList()
            };

            return View("~/Views/General/Quiz/EditFeedbackForm.cshtml", viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFeedbackForm(EditFeedbackFormViewModel viewModel)
        {
            TempData["ErrorMessage"] = null;
            TempData["SuccessMessage"] = null;

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "An unexpected error has occurred.";
                return View("~/Views/General/Quiz/EditFeedbackForm.cshtml", viewModel);
            }

            var feedbackForm = await _context.FeedbackForms
                .Include(f => f.Questions)
                .FirstOrDefaultAsync(f => f.FeedbackFormID == viewModel.FeedbackFormID);

            if (feedbackForm == null)
            {
                TempData["ErrorMessage"] = "Feedback form not found.";
                return RedirectToAction(nameof(ViewAllFeedbackForms));
            }

            // Update feedback form title
            feedbackForm.Title = viewModel.Title;

            // Remove all existing questions for this feedback form
            _context.FeedbackQuestions.RemoveRange(feedbackForm.Questions);

            // Add new questions from the view model
            foreach (var questionViewModel in viewModel.Questions)
            {
                feedbackForm.Questions.Add(new FeedbackQuestion
                {
                    FeedbackFormID = viewModel.FeedbackFormID,
                    QuestionText = questionViewModel.QuestionText,
                    Type = (Models.FeedbackType)questionViewModel.Type
                });
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Feedback form updated successfully!";
            return RedirectToAction(nameof(ViewAllFeedbackForms));
        }




        [HttpPost]
        public async Task<IActionResult> CheckLinkedQuizzes([FromBody] Guid feedbackFormID)
        {
          var linkedQuizzes = await _context.Quiz
         .Where(q => q.FeedbackFormID == feedbackFormID)
         .Select(q => q.Title)
         .ToListAsync();

            return Json(new { isLinked = linkedQuizzes.Any(), linkedQuizzes });
        }

        [HttpPost]
        public async Task<IActionResult> CheckLinkedQuizAttempts([FromBody] Guid feedbackFormID)
        {
            // Get all quizzes linked to this feedback form
            var linkedQuizzes = await _context.Quiz
                .Where(q => q.FeedbackFormID == feedbackFormID)
                .Select(q => q.QuizID)
                .ToListAsync();

            // Check if any of the linked quizzes have attempts
            bool hasAttempts = await _context.Attempt
                .AnyAsync(a => linkedQuizzes.Contains(a.QuizID));

            if (hasAttempts)
            {
                // Get the titles of quizzes with attempts for display purposes
                var attemptedQuizzes = await _context.Quiz
                    .Where(q => linkedQuizzes.Contains(q.QuizID) && _context.Attempt.Any(a => a.QuizID == q.QuizID))
                    .Select(q => q.Title)
                    .ToListAsync();

                return Json(new { hasAttempts = true, attemptedQuizzes });
            }

            // If no attempts, return false for hasAttempts
            return Json(new { hasAttempts = false });
        }

        private async Task<bool> IsFeedbackFormLinkedToQuiz(Guid feedbackFormId)
        {
            return await _context.Quiz
                .AnyAsync(q => q.FeedbackFormID == feedbackFormId);
        }

    }
}
