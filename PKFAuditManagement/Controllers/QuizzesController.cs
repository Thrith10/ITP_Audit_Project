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
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace PKFAuditManagement.Controllers
{
    public class QuizzesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;

        public QuizzesController(ApplicationDbContext context, UserManager<CustomUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Quizzes/Create
        [HttpGet]
        [Authorize(Roles = "Admin")]
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
                var currentUser = await _userManager.GetUserAsync(User);
                var userId = currentUser?.Id;

                // Create a new Quiz entity
                var quiz = new Quiz
                {
                    Title = quizViewModel.Title,
                    Description = quizViewModel.Description,
                    QuizStart = quizViewModel.QuizStart,
                    QuizEnd = quizViewModel.QuizEnd,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    FeedbackFormID = quizViewModel.SelectedFeedbackFormId,
                    SelfAssessmentFormID = quizViewModel.SelectedSelfAssessmentFormId // Set selected self-assessment form ID



                };

                // Add the quiz to the context first to generate the QuizID
                _context.Add(quiz);
                await _context.SaveChangesAsync();

                // Process each question from the view model
                foreach (var questionViewModel in quizViewModel.Questions)
                {
                    var question = new Questions
                    {
                        QuizID = quiz.QuizID,
                        Description = questionViewModel.Description,
                        Type = (QuestionType)questionViewModel.Type,
                    };

                    // Handle question types
                    switch (questionViewModel.Type)
                    {
                        case QuestionType.TrueFalse:
                            // For True/False, set CorrectOptionText and add options for "True" and "False"
                            question.CorrectOptionText = questionViewModel.CorrectOptionText;

                            // Add the question to the context and save to generate the QuestionID
                            _context.Questions.Add(question);
                            await _context.SaveChangesAsync();

                            // Add "True" and "False" as options
                            var trueOption = new Option
                            {
                                QuestionID = question.QuestionID,
                                OptionText = "True"
                            };
                            var falseOption = new Option
                            {
                                QuestionID = question.QuestionID,
                                OptionText = "False"
                            };
                            _context.Option.Add(trueOption);
                            _context.Option.Add(falseOption);
                            break;

                        case QuestionType.SingleAnswerMCQ:
                            // For SingleAnswerMCQ, use CorrectOptionText
                            question.CorrectOptionText = questionViewModel.CorrectOptionText;
                            _context.Questions.Add(question);
                            await _context.SaveChangesAsync();
                            break;

                        case QuestionType.MultiAnswerMCQ:
                            // For MultiAnswerMCQ, join the correct options list into a single string
                            question.CorrectOptionText = questionViewModel.CorrectOptionTexts != null
                                ? string.Join(';', questionViewModel.CorrectOptionTexts)
                                : null;
                            _context.Questions.Add(question);
                            await _context.SaveChangesAsync();
                            break;
                    }

                    // Add other options for non-TrueFalse questions
                    if (questionViewModel.Type != QuestionType.TrueFalse)
                    {
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
                }

                // Handle selected participants by querying their UserID based on their email
                if (!string.IsNullOrEmpty(quizViewModel.SelectedParticipants))
                {
                    var emailList = quizViewModel.SelectedParticipants.Split(';');

                    foreach (var email in emailList)
                    {
                        var trimmedEmail = email.Trim();

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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ViewAllQuiz()
        {
            // Get the current user
            var currentUser = await _userManager.GetUserAsync(User);
            var userId = currentUser?.Id;

            // Check if the user is authenticated
            if (userId == null)
            {
                return Unauthorized();
            }

            // Query the quizzes where CreatedBy matches the current user's ID
            var quizzes = await _context.Quiz
                                        .Where(q => q.CreatedBy == userId)
                                        .ToListAsync();

            var quizListViewModel = new QuizListViewModel
            {
                Quizzes = quizzes.Select(q => new QuizViewModel
                {
                    QuizID = q.QuizID,
                    Title = q.Title,
                    Description = q.Description,
                    QuizStart = q.QuizStart,
                    QuizEnd = q.QuizEnd
                }).ToList()
            };

            return View("~/Views/General/Quiz/ViewAllQuiz.cshtml", quizListViewModel);
        }

        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var quiz = await _context.Quiz
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                .Include(q => q.Participants)
                .FirstOrDefaultAsync(q => q.QuizID == id);

            if (quiz == null)
            {
                return NotFound();
            }

            var participantUserIDs = quiz.Participants.Select(p => p.UserID).ToList();
            var participantEmails = await _context.Users
                .Where(u => participantUserIDs.Contains(u.Id))
                .Select(u => u.Email)
                .ToListAsync();

            var selectedParticipants = string.Join(";", participantEmails);

            var model = new QuizViewModel
            {
                QuizID = quiz.QuizID,
                Title = quiz.Title,
                Description = quiz.Description,
                QuizStart = quiz.QuizStart,
                QuizEnd = quiz.QuizEnd,
                SelectedFeedbackFormId = quiz.FeedbackFormID, // Load Feedback Form ID
                Questions = quiz.Questions.Select(q => new QuestionViewModel
                {
                    QuestionID = q.QuestionID,
                    Description = q.Description,
                    Type = q.Type,  // Include the Question Type here
                    CorrectOptionText = q.CorrectOptionText,
                    Options = q.Options.Select(o => new OptionViewModel
                    {
                        OptionID = o.OptionID,
                        OptionText = o.OptionText
                    }).ToList()
                }).ToList(),
                SelectedParticipants = selectedParticipants
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
                // Fetch the quiz and related data (Questions, Options, and Participants)
                var quiz = await _context.Quiz
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.Options)
                    .Include(q => q.Participants)
                    .FirstOrDefaultAsync(q => q.QuizID == quizViewModel.QuizID);

                if (quiz == null)
                {
                    return NotFound();
                }

                // Update quiz details
                quiz.Title = quizViewModel.Title;
                quiz.Description = quizViewModel.Description;
                quiz.QuizStart = quizViewModel.QuizStart;
                quiz.QuizEnd = quizViewModel.QuizEnd;
                quiz.FeedbackFormID = quizViewModel.SelectedFeedbackFormId; // Update Feedback Form ID
                quiz.SelfAssessmentFormID = quizViewModel.SelectedSelfAssessmentFormId;


                // Clear existing questions and options
                var existingQuestions = quiz.Questions.ToList();
                foreach (var question in existingQuestions)
                {
                    // Remove all options associated with this question
                    _context.Option.RemoveRange(question.Options);

                    // Remove the question itself
                    _context.Questions.Remove(question);
                }

                // Clear existing participants
                quiz.Participants.Clear();

                // Save changes to remove questions and participants
                await _context.SaveChangesAsync();

                // Add new questions and options from the view model
                foreach (var questionViewModel in quizViewModel.Questions)
                {
                    var newQuestion = new Questions
                    {
                        QuizID = quiz.QuizID,
                        Description = questionViewModel.Description,
                        Type = (QuestionType)questionViewModel.Type
                    };

                    // Handle question types
                    switch (questionViewModel.Type)
                    {
                        case QuestionType.TrueFalse:
                            // For True/False, set CorrectOptionText and add options for "True" and "False"
                            newQuestion.CorrectOptionText = questionViewModel.CorrectOptionText;

                            // Add the new question to the quiz
                            _context.Questions.Add(newQuestion);
                            await _context.SaveChangesAsync(); // Save question to get QuestionID

                            // Add "True" and "False" as options
                            var trueOption = new Option
                            {
                                QuestionID = newQuestion.QuestionID,
                                OptionText = "True"
                            };
                            var falseOption = new Option
                            {
                                QuestionID = newQuestion.QuestionID,
                                OptionText = "False"
                            };
                            _context.Option.Add(trueOption);
                            _context.Option.Add(falseOption);
                            break;

                        case QuestionType.SingleAnswerMCQ:
                            // For SingleAnswerMCQ, set CorrectOptionText
                            newQuestion.CorrectOptionText = questionViewModel.CorrectOptionText;

                            // Add the new question to the quiz
                            _context.Questions.Add(newQuestion);
                            await _context.SaveChangesAsync(); // Save question to get QuestionID

                            // Add options for SingleAnswerMCQ
                            foreach (var optionViewModel in questionViewModel.Options)
                            {
                                var newOption = new Option
                                {
                                    QuestionID = newQuestion.QuestionID,
                                    OptionText = optionViewModel.OptionText
                                };
                                _context.Option.Add(newOption);
                            }
                            break;

                        case QuestionType.MultiAnswerMCQ:
                            // For MultiAnswerMCQ, join correct options into a string
                            newQuestion.CorrectOptionText = questionViewModel.CorrectOptionTexts != null
                                ? string.Join(';', questionViewModel.CorrectOptionTexts)
                                : null;

                            // Add the new question to the quiz
                            _context.Questions.Add(newQuestion);
                            await _context.SaveChangesAsync(); // Save question to get QuestionID

                            // Add options for MultiAnswerMCQ
                            foreach (var optionViewModel in questionViewModel.Options)
                            {
                                var newOption = new Option
                                {
                                    QuestionID = newQuestion.QuestionID,
                                    OptionText = optionViewModel.OptionText
                                };
                                _context.Option.Add(newOption);
                            }
                            break;
                    }

                    // Save options after adding them for each question
                    await _context.SaveChangesAsync();
                }

                // Add new participants from the view model
                if (!string.IsNullOrEmpty(quizViewModel.SelectedParticipants))
                {
                    var participantEmails = quizViewModel.SelectedParticipants.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
                    var participants = await _context.Users
                        .Where(u => participantEmails.Contains(u.Email))
                        .ToListAsync();

                    foreach (var participant in participants)
                    {
                        quiz.Participants.Add(new Participants
                        {
                            QuizID = quiz.QuizID,
                            UserID = participant.Id,
                            IsRequired = true
                        });
                    }
                }

                // Save the new participants
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Quiz updated successfully!";
                return RedirectToAction(nameof(Edit), new { id = quiz.QuizID });
            }

            // If model state is invalid, return to the view
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

        //This Method delete quiz that have associated data
        // POST: Quizzes/ConfirmDeleteWithAssociation/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDeleteWithAssociation(Guid id)
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

            // Save changes to delete related data
            await _context.SaveChangesAsync();

            // Delete the quiz itself
            _context.Quiz.Remove(quiz);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Quiz and all associated data deleted successfully!";
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
        public async Task<IActionResult> GetAllWithUserRole()
        {
            // Get the role ID for the "User" role
            var userRoleId = await _context.Roles
                .Where(r => r.Name == "User")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(userRoleId))
            {
                return Json(new List<UserViewModel>()); // Return an empty list if the role is not found
            }

            // Fetch users with the "User" role
            var users = await _context.Users
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == userRoleId))
                .Select(u => new UserViewModel
                {
                    Email = u.Email,
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
        [HttpPost]
        public async Task<IActionResult> LoadManualQuizCreationAsync([FromBody] ExcelQuizViewModel excelQuizModel)
        {
            try
            {
                // Validate emails
                var validEmailsResult = await EmailValidation(excelQuizModel.Participants);

                if (validEmailsResult is BadRequestObjectResult badRequestResult)
                {
                    return badRequestResult; // Handle the case where no valid emails were found
                }

                // Parse the valid emails string returned by EmailValidation (semicolon-delimited)
                var validEmails = validEmailsResult is OkObjectResult okResult && okResult.Value is string validEmailsString
                    ? validEmailsString.Split(';').ToList()
                    : new List<string>();

                // Parse QuizStart
                var quizStart = DateTime.Parse(excelQuizModel.QuizStart);

                // Map ExcelQuizViewModel to QuizViewModel
                var quizViewModel = new QuizViewModel
                {
                    Title = excelQuizModel.Title,
                    Description = excelQuizModel.Description,
                    QuizStart = quizStart,
                    // Automatically set QuizEnd to 11:59 PM on the same day as QuizStart
                    QuizEnd = new DateTime(quizStart.Year, quizStart.Month, quizStart.Day, 23, 59, 59),
                    Questions = excelQuizModel.Questions.Select(q => new QuestionViewModel
                    {
                        Description = q.Description,
                        Type = q.Type, // Map to enum
                        Options = new List<OptionViewModel>
                {
                    new OptionViewModel { OptionText = q.OptionA },
                    new OptionViewModel { OptionText = q.OptionB },
                    new OptionViewModel { OptionText = q.OptionC },
                    new OptionViewModel { OptionText = q.OptionD },
                    new OptionViewModel { OptionText = q.OptionE }
                },
                        CorrectOptionText = q.CorrectAnswer
                    }).ToList(),
                    SelectedParticipants = validEmails != null && validEmails.Any()
                        ? string.Join(";", validEmails)
                        : null
                };
                TempData["QuizViewModel"] = JsonConvert.SerializeObject(quizViewModel);

                // Return the existing ManualQuizCreation PartialView with the mapped QuizViewModel
                return View("~/Views/General/Quiz/QuizAutoFilled.cshtml", quizViewModel);
            }
            catch (Exception ex)
            {
                // Log the error message with details of the failing line
                var errorDetails = $"Error occurred in {nameof(LoadManualQuizCreationAsync)}: {ex.Message}";

                // Add specific handling to include stack trace for debugging purposes
                errorDetails += Environment.NewLine + ex.StackTrace;

                // Log the error to a logging framework like Serilog, NLog, etc.
                // For example: _logger.LogError(errorDetails);

                // Optionally, include a detailed error response for easier debugging
                return StatusCode(500, new { message = "An error occurred while processing the request.", details = errorDetails });
            }
        }

        [HttpGet]
        public IActionResult QuizAutoFilled()
        {
            // Retrieve the QuizViewModel from TempData
            if (TempData["QuizViewModel"] == null)
            {
                return RedirectToAction("Error"); // Redirect to an error page if the model is missing
            }

            var quizViewModel = JsonConvert.DeserializeObject<QuizViewModel>((string)TempData["QuizViewModel"]);
            TempData["QuizViewModel"] = null;
            // Render the view with the model
            return View("~/Views/General/Quiz/QuizAutoFilled.cshtml", quizViewModel);
        }


        // GET: Feedback/GetFeedbackForms
        [HttpGet]
        public async Task<IActionResult> GetFeedbackForms()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userId = currentUser?.Id;

            if (userId == null)
            {
                return Unauthorized();
            }

            var feedbackForms = await _context.FeedbackForms
                .Where(f => f.CreatedBy == userId) // Filter by current user's ID
                .Select(f => new
                {
                    Id = f.FeedbackFormID,
                    Title = f.Title
                })
                .ToListAsync();

            return Json(feedbackForms);
        }
        // GET: SelfAssessment/GetSelfAssessmentForms
        [HttpGet]
        public async Task<IActionResult> GetSelfAssessmentForms()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userId = currentUser?.Id;

            if (userId == null)
            {
                return Unauthorized();
            }

            var selfAssessmentForms = await _context.SelfAssessmentForms
                .Where(f => f.CreatedBy == userId) // Filter by current user's ID
                .Select(f => new
                {
                    Id = f.SelfAssessmentFormID,
                    Title = f.Title
                })
                .ToListAsync();

            return Json(selfAssessmentForms);
        }

        [HttpPost]
        public async Task<IActionResult> CheckQuizAttempts(Guid id)
        {
            var hasAttempts = await _context.Attempt.AnyAsync(a => a.QuizID == id); 
            return Json(new { hasAttempts = hasAttempts });
        }





    }

}
