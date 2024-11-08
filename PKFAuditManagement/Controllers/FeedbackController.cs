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

        // DELETE: Feedback/DeleteFeedbackForm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFeedbackForm(Guid feedbackFormID)
        {
            var feedbackForm = await _context.FeedbackForms
                .Include(f => f.Questions) // Include related questions to cascade delete
                .FirstOrDefaultAsync(f => f.FeedbackFormID == feedbackFormID);

            if (feedbackForm == null)
            {
                TempData["ErrorMessage"] = "Feedback form not found.";
                return RedirectToAction(nameof(ViewAllFeedbackForms));
            }

            // Remove feedback form and associated questions
            _context.FeedbackForms.Remove(feedbackForm);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Feedback form deleted successfully!";
            return RedirectToAction(nameof(ViewAllFeedbackForms));
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
    }
}
