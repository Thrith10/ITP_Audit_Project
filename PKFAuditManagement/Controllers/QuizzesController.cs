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
                    Description = quizViewModel.Description
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
                    Description = q.Description
                }).ToList()
            };

            return View("~/Views/General/Quiz/ViewAllQuiz.cshtml", quizListViewModel);
        }
        // Additional actions for Edit, Delete, Details, etc., can be added here
        // Your existing Details method with QR code generation added
        public async Task<IActionResult> Details(int id)
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

            var participants = await (from p in _context.Participants
                                      join u in _context.Users on p.UserID equals u.Id
                                      join a in _context.Attempt on new { p.UserID, p.QuizID } equals new { a.UserID, a.QuizID }
                                      where p.QuizID == id
                                      select new ParticipantViewModel
                                      {
                                          UserID = p.UserID,
                                          UserName = u.UserName,
                                          Email = u.Email, // Add Email field
                                          TotalScore = p.TotalScore,
                                          AttemptDate = a.AttemptDate // Fetch AttemptDate from the Attempt table
                                      }).Distinct().ToListAsync();

            var viewModel = new QuizViewModel
            {
                QuizID = quiz.QuizID,
                Title = quiz.Title,
                Description = quiz.Description,
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
                Participants = participants,
                QRImageURL = GenerateQRCode($"Quiz ID: {quiz.QuizID}") // Generate QR code for QuizID
            };

            ViewBag.TotalQuestions = totalQuestions;
            ViewBag.FailedParticipantsEmails = JsonSerializer.Serialize(viewModel.Participants.Where(p => p.TotalScore < 0.5 * viewModel.Questions.Count()).Select(p => p.Email).ToList());

            return View("~/Views/General/Quiz/QuizDetailsPage.cshtml", viewModel);
        }


        // GET: Quizzes/Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
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

                quiz.Title = quizViewModel.Title;
                quiz.Description = quizViewModel.Description;

                _context.Update(quiz);
                await _context.SaveChangesAsync();

                foreach (var questionViewModel in quizViewModel.Questions)
                {
                    var question = quiz.Questions.FirstOrDefault(q => q.QuestionID == questionViewModel.QuestionID);
                    if (question != null)
                    {
                        question.Description = questionViewModel.Description;
                        question.CorrectOptionText = questionViewModel.CorrectOptionText;

                        foreach (var optionViewModel in questionViewModel.Options)
                        {
                            var option = question.Options.FirstOrDefault(o => o.OptionID == optionViewModel.OptionID);
                            if (option != null)
                            {
                                option.OptionText = optionViewModel.OptionText;
                            }
                            else
                            {
                                question.Options.Add(new Option
                                {
                                    QuestionID = question.QuestionID,
                                    OptionText = optionViewModel.OptionText
                                });
                            }
                        }

                        _context.Update(question);
                    }
                    else
                    {
                        quiz.Questions.Add(new Questions
                        {
                            QuizID = quiz.QuizID,
                            Description = questionViewModel.Description,
                            CorrectOptionText = questionViewModel.CorrectOptionText,
                            Options = questionViewModel.Options.Select(o => new Option
                            {
                                OptionText = o.OptionText
                            }).ToList()
                        });
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Quiz updated successfully!";
                return RedirectToAction(nameof(Details), new { id = quiz.QuizID });
            }

            return View("~/Views/General/Quiz/EditQuiz.cshtml", quizViewModel);
        }
        // POST: Quizzes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var quiz = await _context.Quiz
                                     .Include(q => q.Questions)
                                     .ThenInclude(q => q.Options)
                                     .FirstOrDefaultAsync(q => q.QuizID == id);

            if (quiz == null)
            {
                return NotFound();
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
