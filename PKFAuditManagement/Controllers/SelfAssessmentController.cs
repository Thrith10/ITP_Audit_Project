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
    public class SelfAssessmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;

        public SelfAssessmentController(ApplicationDbContext context, UserManager<CustomUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllSelfAssessmentForms()
        {
            // Get the current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized(); // Handle unauthenticated users
            }

            var currentUserId = currentUser.Id;

            // Filter forms created by the current user
            var selfAssessmentForms = await _context.SelfAssessmentForms
                .Where(f => f.CreatedBy == currentUserId) // Filter by user ID
                .Select(f => new SelfAssessmentFormViewModel
                {
                    SelfAssessmentFormID = f.SelfAssessmentFormID,
                    Title = f.Title,
                    CreatedDate = f.CreatedDate,
                    CreatedBy = f.CreatedBy
                })
                .ToListAsync();

            var model = new SelfAssessmentFormListViewModel
            {
                SelfAssessmentForms = selfAssessmentForms
            };

            return View("~/Views/General/Quiz/ViewAllSelfAssessmentForm.cshtml", model);
        }


        [HttpGet]
        public IActionResult CreateSelfAssessment()
        {
            var model = new SelfAssessmentFormViewModel
            {
                Questions = new List<SelfAssessmentQuestionViewModel>
                {
                    new SelfAssessmentQuestionViewModel() // Initialize with one empty question
                }
            };

            return View("~/Views/General/Quiz/CreateSelfAssessmentForm.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSelfAssessment(SelfAssessmentFormViewModel selfAssessmentFormViewModel)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var userId = currentUser?.Id;

                var selfAssessmentForm = new SelfAssessmentForm
                {
                    Title = selfAssessmentFormViewModel.Title,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now
                };

                _context.Add(selfAssessmentForm);
                await _context.SaveChangesAsync();

                foreach (var questionViewModel in selfAssessmentFormViewModel.Questions)
                {
                    var selfAssessmentQuestion = new SelfAssessmentQuestion
                    {
                        SelfAssessmentFormID = selfAssessmentForm.SelfAssessmentFormID,
                        QuestionText = questionViewModel.QuestionText,
                        Type = (Models.SelfAssessmentType)questionViewModel.Type
                    };

                    _context.SelfAssessmentQuestions.Add(selfAssessmentQuestion);
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Self-assessment form created successfully!";
                return RedirectToAction(nameof(CreateSelfAssessment));
            }

            return View("~/Views/General/Quiz/CreateSelfAssessmentForm.cshtml", selfAssessmentFormViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ViewSelfAssessmentForm(Guid id)
        {
            var selfAssessmentForm = await _context.SelfAssessmentForms
                .Where(f => f.SelfAssessmentFormID == id)
                .Select(f => new SelfAssessmentFormViewModel
                {
                    SelfAssessmentFormID = f.SelfAssessmentFormID,
                    Title = f.Title,
                    CreatedDate = f.CreatedDate,
                    Questions = f.Questions.Select(q => new SelfAssessmentQuestionViewModel
                    {
                        QuestionText = q.QuestionText,
                        Type = (ViewModels.SelfAssessmentType)q.Type
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (selfAssessmentForm == null)
            {
                return NotFound();
            }

            return View("~/Views/General/Quiz/ViewSelfAssessmentForm.cshtml", selfAssessmentForm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelfAssessmentForm(Guid selfAssessmentFormId)
        {
            var selfAssessmentForm = await _context.SelfAssessmentForms
                .Include(f => f.Questions)
                .FirstOrDefaultAsync(f => f.SelfAssessmentFormID == selfAssessmentFormId);

            if (selfAssessmentForm == null)
            {
                TempData["ErrorMessage"] = "Self-assessment form not found.";
                return RedirectToAction(nameof(ViewAllSelfAssessmentForms));
            }

            _context.SelfAssessmentForms.Remove(selfAssessmentForm);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Self-assessment form deleted successfully!";
            return RedirectToAction(nameof(ViewAllSelfAssessmentForms));
        }

        [HttpGet]
        public async Task<IActionResult> EditSelfAssessmentForm(Guid selfAssessmentFormId)
        {
            var selfAssessmentForm = await _context.SelfAssessmentForms
                .Include(f => f.Questions)
                .FirstOrDefaultAsync(f => f.SelfAssessmentFormID == selfAssessmentFormId);

            if (selfAssessmentForm == null)
            {
                TempData["ErrorMessage"] = "Self-assessment form not found.";
                return RedirectToAction(nameof(ViewAllSelfAssessmentForms));
            }

            var viewModel = new EditSelfAssessmentFormViewModel
            {
                SelfAssessmentFormID = selfAssessmentForm.SelfAssessmentFormID,
                Title = selfAssessmentForm.Title,
                CreatedDate = selfAssessmentForm.CreatedDate,
                CreatedBy = selfAssessmentForm.CreatedBy,
                Questions = selfAssessmentForm.Questions.Select(q => new SelfAssessmentQuestionViewModel
                {
                    QuestionText = q.QuestionText,
                    Type = (ViewModels.SelfAssessmentType)q.Type
                }).ToList()
            };

            return View("~/Views/General/Quiz/EditSelfAssessmentForm.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSelfAssessmentForm(EditSelfAssessmentFormViewModel viewModel)
        {
            TempData["ErrorMessage"] = null;
            TempData["SuccessMessage"] = null;

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "An unexpected error has occurred.";
                return View("~/Views/General/Quiz/EditSelfAssessmentForm.cshtml", viewModel);
            }

            var selfAssessmentForm = await _context.SelfAssessmentForms
                .Include(f => f.Questions)
                .FirstOrDefaultAsync(f => f.SelfAssessmentFormID == viewModel.SelfAssessmentFormID);

            if (selfAssessmentForm == null)
            {
                TempData["ErrorMessage"] = "Self-assessment form not found.";
                return RedirectToAction(nameof(ViewAllSelfAssessmentForms));
            }

            selfAssessmentForm.Title = viewModel.Title;

            _context.SelfAssessmentQuestions.RemoveRange(selfAssessmentForm.Questions);

            foreach (var questionViewModel in viewModel.Questions)
            {
                selfAssessmentForm.Questions.Add(new SelfAssessmentQuestion
                {
                    SelfAssessmentFormID = viewModel.SelfAssessmentFormID,
                    QuestionText = questionViewModel.QuestionText,
                    Type = (Models.SelfAssessmentType)questionViewModel.Type
                });
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Self-assessment form updated successfully!";
            return RedirectToAction(nameof(ViewAllSelfAssessmentForms));
        }
        [HttpPost]
        public async Task<IActionResult> CheckLinkedQuizAttempts([FromBody] Guid selfAssessmentFormID)
        {
            // Get all quizzes linked to this self-assessment form
            var linkedQuizzes = await _context.Quiz
                .Where(q => q.SelfAssessmentFormID == selfAssessmentFormID)
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

        [HttpPost]
        public async Task<IActionResult> CheckLinkedQuizzes([FromBody] Guid selfAssessmentFormID)
        {
            // Find quizzes linked to the specified self-assessment form
            var linkedQuizzes = await _context.Quiz
                .Where(q => q.SelfAssessmentFormID == selfAssessmentFormID)
                .Select(q => q.Title)
                .ToListAsync();

            return Json(new { isLinked = linkedQuizzes.Any(), linkedQuizzes });
        }


        private async Task<bool> IsSelfAssessmentFormLinkedToQuiz(Guid selfAssessmentFormId)
        {
            return await _context.Quiz
                .AnyAsync(q => q.SelfAssessmentFormID == selfAssessmentFormId);
        }
    }
}
