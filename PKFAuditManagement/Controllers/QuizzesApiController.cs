using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using PKFAuditManagement.Models;
using PKFAuditManagement.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PKFAuditManagement.Data;

namespace PKFAuditManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizzesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;

        public QuizzesApiController(ApplicationDbContext context, UserManager<CustomUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/QuizzesApi/ValidateUser
        [HttpGet("ValidateUser/{email}")]
        public async Task<ActionResult<string>> ValidateUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound("User not found");
            }

            return user.Id;
        }

        // GET: api/QuizzesApi/GetUserQuizzes/{userId}
        [HttpGet("GetUserQuizzes/{userId}")]
        public async Task<ActionResult<IEnumerable<QuizViewModel>>> GetUserQuizzes(string userId)
        {
            var userAttempts = await _context.Attempt
                .Where(a => a.UserID == userId)
                .Include(a => a.Quiz)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(q => q.Options)
                .ToListAsync();

            var quizViewModels = userAttempts.Select(a => new QuizViewModel
            {
                QuizID = a.Quiz.QuizID,
                Title = a.Quiz.Title,
                Description = a.Quiz.Description,
                AttemptDate = a.AttemptDate,
                Questions = a.Quiz.Questions.Select(ques => new QuestionViewModel
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

            return quizViewModels;
        }

        // GET: api/QuizzesApi/GetQuiz/{id}
        // GET: api/QuizzesApi/GetQuiz/{id}
        [HttpGet("GetQuiz/{id}")]
        public async Task<ActionResult<QuizViewModel>> GetQuiz(int id)
        {
            var quiz = await _context.Quiz
                .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.QuizID == id);

            if (quiz == null)
            {
                return NotFound(new { Message = $"Quiz with ID {id} not found" });
            }

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
            };

            return Ok(viewModel);
        }


        // POST: api/QuizzesApi/RecordAttempt
        [HttpPost("RecordAttempt")]
        public async Task<IActionResult> RecordAttempt([FromBody] AttemptViewModel attemptViewModel)
        {
            if (ModelState.IsValid)
            {
                var totalScore = 0;

                var attempt = new Attempt
                {
                    UserID = attemptViewModel.UserID,
                    QuizID = attemptViewModel.QuizID,
                    AttemptDate = DateTime.Now,
                    QuizResponses = attemptViewModel.Responses.Select(r =>
                    {
                        var correctOptionText = _context.Questions
                            .Where(q => q.QuestionID == r.QuestionID)
                            .Select(q => q.CorrectOptionText)
                            .FirstOrDefault();

                        var isCorrect = correctOptionText == r.SelectedOption;
                        if (isCorrect) totalScore++;

                        return new QuizResponse
                        {
                            QuestionID = r.QuestionID,
                            SelectedOption = r.SelectedOption,
                            IsCorrect = isCorrect
                        };
                    }).ToList()
                };

                _context.Attempt.Add(attempt);

                var participant = new Participants
                {
                    UserID = attemptViewModel.UserID,
                    QuizID = attemptViewModel.QuizID,
                    TotalScore = totalScore
                };

                _context.Participants.Add(participant);

                await _context.SaveChangesAsync();
                return Ok();
            }

            return BadRequest(ModelState);
        }
    }
}
