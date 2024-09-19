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
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

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
                    QuizStart = quizViewModel.QuizStart
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
                    }
                    await _context.SaveChangesAsync(); // Save each option to get OptionID
                }

                // Handle selected participants by querying their UserID based on their email
                if (!string.IsNullOrEmpty(quizViewModel.SelectedParticipants))
                {
                    // Split the email string by the delimiter `;`
                    var emailList = quizViewModel.SelectedParticipants.Split(';');

                    foreach (var email in emailList)
                    {
                        var trimmedEmail = email.Trim(); // Ensure to remove any extra spaces around the email

                        // Query the database to find the UserID based on the email
                        var user = await _context.Users
                            .Where(u => u.Email == trimmedEmail)
                            .FirstOrDefaultAsync();

                        if (user != null)
                        {
                            var participant = new Participants
                            {
                                UserID = user.Id, // Assuming Id is the primary key of the User entity
                                QuizID = quiz.QuizID,
                                IsRequired = true
                            };
                            _context.Participants.Add(participant);
                        }
                    }

                    await _context.SaveChangesAsync();
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

        public async Task<IActionResult> Details(Guid id)
        {
            // Fetch the quiz with its questions and options
            var quiz = await _context.Quiz
                                     .Include(q => q.Questions)
                                     .ThenInclude(q => q.Options)
                                     .FirstOrDefaultAsync(q => q.QuizID == id);

            if (quiz == null)
            {
                return NotFound();
            }

            // Check if any participants associated with the quiz have ClockedAttendance, QuizDone, FeedbackDone, or OverallCompletion as true
            var hasLockedParticipants = await _context.Participants
                                                      .Where(p => p.QuizID == quiz.QuizID)
                                                      .AnyAsync(p => p.ClockedAttendance || p.QuizDone || p.FeedbackDone || p.OverallCompletion);

            // Set CanEdit to false if any of the conditions are true, otherwise true
            bool canEdit = !hasLockedParticipants;

            var totalQuestions = quiz.Questions.Count;

            // Create the view model
            var viewModel = new QuizViewModel
            {
                QuizID = quiz.QuizID,
                Title = quiz.Title,
                Description = quiz.Description,
                QuizStart = quiz.QuizStart, // Include QuizStart in the view model
                CanEdit = canEdit,          // Set CanEdit based on the criteria
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
                QRImageURL = GenerateQRCode($"Quiz ID: {quiz.QuizID}") // Generate QR code for QuizID
            };

            ViewBag.TotalQuestions = totalQuestions;

            return View("~/Views/General/Quiz/QuizDetailsPage.cshtml", viewModel);
        }



        // GET: Quizzes/Edit
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            // Fetch quiz details including questions, options, and participants (UserID)
            var quiz = await _context.Quiz
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                .Include(q => q.Participants) // Fetch the participants associated with the quiz
                .FirstOrDefaultAsync(q => q.QuizID == id);

            if (quiz == null)
            {
                return NotFound();
            }

            // Get the list of UserIDs from the Participants
            var participantUserIDs = quiz.Participants.Select(p => p.UserID).ToList();

            // Fetch the corresponding email addresses using UserID
            var participantEmails = await _context.Users
                .Where(u => participantUserIDs.Contains(u.Id)) // Filter by the participant UserIDs
                .Select(u => u.Email) // Select the email of the participants
                .ToListAsync();

            // Join the email list into a single string with semicolon delimiter
            var selectedParticipants = string.Join(";", participantEmails);

            // Build the QuizViewModel
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
                }).ToList(),
                SelectedParticipants = selectedParticipants // Assign the emails as a single string with delimiter
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
                // Fetch the quiz including its questions and options
                var quiz = await _context.Quiz
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.Options)
                    .Include(q => q.Participants) // Include participants to update later
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

                // Handle participants' emails (SelectedParticipants) and update participants table
                if (!string.IsNullOrEmpty(quizViewModel.SelectedParticipants))
                {
                    // Split the string of emails into a list
                    var participantEmails = quizViewModel.SelectedParticipants.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();

                    // Fetch corresponding users by email
                    var participants = await _context.Users
                        .Where(u => participantEmails.Contains(u.Email)) // Find users whose email matches
                        .ToListAsync();

                    // Remove existing participants
                    quiz.Participants.Clear();

                    // Add new participants (from fetched users)
                    foreach (var participant in participants)
                    {
                        quiz.Participants.Add(new Participants
                        {
                            QuizID = quiz.QuizID,
                            UserID = participant.Id // Add participant by UserID
                        });
                    }
                }

                // Save changes to the database
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Quiz updated successfully.";

                // Redirect to Quiz Details Page with the updated quizViewModel
                return RedirectToAction(nameof(Details), new { id = quiz.QuizID });
            }

            TempData["UnsuccessfulMessage"] = "Failed to update the quiz. Please review the form and try again.";
            // Return the same view with the validation error and current model
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
        public IActionResult GenerateQR(string quizId)
        {
            var qrCodeImage = GenerateQRCode($"{quizId}");
            return Json(new { qrCodeImage });
        }
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new UserViewModel
                {
                    Email = u.Email,  // Ensure email is properly fetched
                    UserId = u.Id,
                    UserName = u.UserName
                })
                .ToListAsync();

            return Json(users);
        }

        [HttpPost]
        public async Task<IActionResult> EmailValidation([FromBody] List<string> emails)
        {
            // Check if the list is null or empty
            if (emails == null || !emails.Any())
            {
                return BadRequest("No emails provided.");
            }

            // Validate each email format
            var validEmails = new List<string>();

            foreach (var email in emails)
            {
                if (IsValidEmailFormat(email))
                {
                    validEmails.Add(email);
                }
            }

            // Query database to validate emails and ensure they exist
            var existingEmails = await _context.Users
                .Where(u => validEmails.Contains(u.Email))
                .Select(u => u.Email)
                .ToListAsync();

            // Return the valid emails as a semicolon-delimited string
            var result = string.Join(";", existingEmails);
            return Ok(result);
        }

        private bool IsValidEmailFormat(string email)
        {
            // Check if the email format is valid
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email; // Ensure the email address is well-formed
            }
            catch
            {
                return false;
            }
        }





    }

}
