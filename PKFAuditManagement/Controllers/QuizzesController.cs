using Microsoft.AspNetCore.Mvc;
using PKFAuditManagement.Models;
using PKFAuditManagement.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PKFAuditManagement.Data;
using QRCoder;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;

namespace PKFAuditManagement.Controllers
{
    public class QuizzesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuizzesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Quizzes/Create
        [HttpGet]
        public IActionResult Create()
        {
            var model = new QuizViewModel
            {
                Questions = new List<QuestionViewModel>
                {
                    new QuestionViewModel
                    {
                        Options = new List<OptionViewModel>()
                    }
                }
            };

            return View("~/Views/General/Quiz/CreateQuiz.cshtml", model);
        }

        // POST: Quizzes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuizViewModel quizViewModel)
        {
            if (ModelState.IsValid)
            {
                // Create a new Quiz entity
                var quiz = new Quiz
                {
                    Title = quizViewModel.Title,
                    Description = quizViewModel.Description,
                    QuizStart = quizViewModel.QuizStart,

                };

                // Add the quiz first to generate the QuizID
                _context.Add(quiz);
                await _context.SaveChangesAsync();

                // Add questions and options
                foreach (var questionViewModel in quizViewModel.Questions)
                {
                    var question = new Questions
                    {
                        QuizID = quiz.QuizID,
                        Description = questionViewModel.Description,
                        CorrectOptionText = questionViewModel.CorrectOptionText
                    };

                    _context.Questions.Add(question);
                    await _context.SaveChangesAsync(); // Save to get QuestionID

                    foreach (var optionViewModel in questionViewModel.Options)
                    {
                        var option = new Option
                        {
                            QuestionID = question.QuestionID,
                            OptionText = optionViewModel.OptionText
                        };

                        _context.Option.Add(option);
                        await _context.SaveChangesAsync(); // Save each option to get OptionID
                    }
                }

                TempData["SuccessMessage"] = "Quiz created successfully!";
                return RedirectToAction(nameof(Create));
            }

            return View("~/Views/General/Quiz/CreateQuiz.cshtml", quizViewModel);
        }

        // GET: Quizzes
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var quizzes = await _context.Quiz
                                        .Include(q => q.Questions)
                                        .ThenInclude(q => q.Options)
                                        .ToListAsync();

            var quizViewModels = quizzes.Select(q => new QuizViewModel
            {
                QuizID = q.QuizID,
                Title = q.Title,
                Description = q.Description,
                Questions = q.Questions.Select(ques => new QuestionViewModel
                {
                    QuestionID = ques.QuestionID,
                    Description = ques.Description,
                    CorrectOptionText = ques.CorrectOptionText,
                    Options = ques.Options.Select(opt => new OptionViewModel
                    {
                        OptionID = opt.OptionID,
                        OptionText = opt.OptionText
                    }).ToList()
                }).ToList()
            }).ToList();

            return View(quizViewModels);
        }
        // GET: Quizzes/ViewAllQuiz
        [HttpGet]
        public async Task<IActionResult> ViewAllQuiz()
        {
            var quizzes = await _context.Quiz
                                        .ToListAsync();

            var quizListViewModel = new QuizListViewModel
            {
                Quizzes = quizzes.Select(q => new QuizViewModel
                {
                    QuizID = q.QuizID,
                    Title = q.Title,
                    Description = q.Description,
                    QuizStart = q.QuizStart // Include QuizStart in the view model

                }).ToList()
            };

            return View("~/Views/General/Quiz/ViewAllQuiz.cshtml", quizListViewModel);
        }
        // Additional actions for Edit, Delete, Details, etc., can be added here
        // Your existing Details method with QR code generation added
        public async Task<IActionResult> Details(Guid id)
        {
            var quiz = await _context.Quiz
                                     .Include(q => q.Questions)
                                     .ThenInclude(q => q.Options)
                                     .FirstOrDefaultAsync(q => q.QuizID == id);

            if (quiz == null)
            {
                return NotFound();
            }

            var totalQuestions = quiz.Questions.Count;



            var viewModel = new QuizViewModel
            {
                QuizID = quiz.QuizID,
                Title = quiz.Title,
                Description = quiz.Description,
                QuizStart = quiz.QuizStart, // Include QuizStart in the view model

                Questions = quiz.Questions.Select(q => new QuestionViewModel
                {
                    QuestionID = q.QuestionID,
                    Description = q.Description,
                    CorrectOptionText = q.CorrectOptionText,
                    Options = q.Options.Select(o => new OptionViewModel
                    {
                        OptionID = o.OptionID,
                        OptionText = o.OptionText
                    }).ToList()
                }).ToList(),
      
                QRImageURL = GenerateQRCode($"Quiz ID: {quiz.QuizID}"), // Generate QR code for QuizID

            };

            ViewBag.TotalQuestions = totalQuestions;
         
            return View("~/Views/General/Quiz/QuizDetailsPage.cshtml", viewModel);
        }


        // GET: Quizzes/Edit
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var quiz = await _context.Quiz
                .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.QuizID == id);

            if (quiz == null)
            {
                return NotFound();
            }

            var model = new QuizViewModel
            {
                QuizID = quiz.QuizID,
                Title = quiz.Title,
                Description = quiz.Description,
                QuizStart = quiz.QuizStart,
                Questions = quiz.Questions.Select(q => new QuestionViewModel
                {
                    QuestionID = q.QuestionID,
                    Description = q.Description,
                    CorrectOptionText = q.CorrectOptionText,
                    Options = q.Options.Select(o => new OptionViewModel
                    {
                        OptionID = o.OptionID,
                        OptionText = o.OptionText
                    }).ToList()
                }).ToList()
            };

            return View("~/Views/General/Quiz/EditQuiz.cshtml", model);
        }

        // POST: Quizzes/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuizViewModel quizViewModel)
        {
            if (ModelState.IsValid)
            {
                var quiz = await _context.Quiz
                    .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                    .FirstOrDefaultAsync(q => q.QuizID == quizViewModel.QuizID);

                if (quiz == null)
                {
                    return NotFound();
                }

                // Update quiz details
                quiz.Title = quizViewModel.Title;
                quiz.Description = quizViewModel.Description;
                quiz.QuizStart = quizViewModel.QuizStart;


                // Remove existing questions and options
                var existingQuestions = quiz.Questions.ToList();
                foreach (var question in existingQuestions)
                {
                    _context.Option.RemoveRange(question.Options);
                    _context.Questions.Remove(question);
                }

                // Add new questions and options
                foreach (var questionViewModel in quizViewModel.Questions)
                {
                    var newQuestion = new Questions
                    {
                        QuizID = quiz.QuizID,
                        Description = questionViewModel.Description,
                        CorrectOptionText = questionViewModel.CorrectOptionText,
                        Options = questionViewModel.Options.Select(o => new Option
                        {
                            OptionText = o.OptionText
                        }).ToList()
                    };
                    quiz.Questions.Add(newQuestion);
                }

                _context.Update(quiz);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Quiz updated successfully!";
                return RedirectToAction(nameof(Details), new { id = quiz.QuizID });
            }

            return View("~/Views/General/Quiz/EditQuiz.cshtml", quizViewModel);
        }

        // POST: Quizzes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, bool deleteAssociated = false)
        {
            var quiz = await _context.Quiz
                                     .Include(q => q.Questions)
                                     .ThenInclude(q => q.Options)
                                     .FirstOrDefaultAsync(q => q.QuizID == id);

            if (quiz == null)
            {
                TempData["ErrorMessage"] = "Quiz not found.";
                return RedirectToAction(nameof(ViewAllQuiz));
            }

            if (!deleteAssociated)
            {
                // Check for related QuizResponse records
                var relatedQuizResponses = await _context.QuizResponse
                    .Where(qr => qr.Question.QuizID == id)
                    .ToListAsync();

                if (relatedQuizResponses.Any())
                {
                    TempData["ErrorMessage"] = "Failed to delete quiz because it is referenced by quiz responses. Do you want to delete the quiz and all associated participants and attempts?";
                    TempData["DeleteQuizID"] = id;
                    return RedirectToAction(nameof(ViewAllQuiz));
                }
            }
            else
            {
                // Delete related QuizResponses
                var relatedQuizResponses = await _context.QuizResponse
                    .Where(qr => qr.Question.QuizID == id)
                    .ToListAsync();

                _context.QuizResponse.RemoveRange(relatedQuizResponses);

                // Delete related Attempts
                var relatedAttempts = await _context.Attempt
                    .Where(a => a.QuizID == id)
                    .ToListAsync();

                _context.Attempt.RemoveRange(relatedAttempts);

                // Delete related Participants
                var relatedParticipants = await _context.Participants
                    .Where(p => p.QuizID == id)
                    .ToListAsync();

                _context.Participants.RemoveRange(relatedParticipants);

                await _context.SaveChangesAsync();
            }

            _context.Quiz.Remove(quiz);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Quiz deleted successfully!";
            return RedirectToAction(nameof(ViewAllQuiz));
        }



        // Add a method to generate QR code image as base64 string
        private string GenerateQRCode(string text)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                byte[] qrCodeImage = qrCode.GetGraphic(20);
                var base64String = Convert.ToBase64String(qrCodeImage);
                return $"data:image/png;base64,{base64String}";
            }
        }

        [HttpGet]
        public IActionResult GenerateQR(int quizId)
        {
            var qrCodeImage = GenerateQRCode($"{quizId}");
            return Json(new { qrCodeImage });
        }

    }

}
