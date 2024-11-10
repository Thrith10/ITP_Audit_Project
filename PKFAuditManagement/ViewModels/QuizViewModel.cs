using PKFAuditManagement.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.ViewModels
{
    public class ParticipantViewModel
    {
        public string UserID { get; set; }

        // New properties reflecting the Participants model
        public Guid QuizID { get; set; }  // To map the participant to a specific quiz
        public DateTime OverallCompletionDate { get; set; }

        public bool IsRequired { get; set; } = true;  // Default is true, similar to the Participants model
        public bool ClockedAttendance { get; set; } = false; // Default is false
        public bool QuizDone { get; set; } = false; // Default is false
        public bool FeedbackDone { get; set; } = false; // Default is false

        public bool OverallCompletion { get; set; } = false; // Default is false
    }

    public class QuizViewModel
    {
        public Guid QuizID { get; set; }  
        public bool CanEdit { get; set; }
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }
        public DateTime QuizStart { get; set; }
        public DateTime QuizEnd { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public DateTime? AttemptDate { get; set; }

        public List<QuestionViewModel> Questions { get; set; } = new List<QuestionViewModel>();
        public string? SelectedParticipants { get; set; }
        public string? QRImageURL { get; set; }
        public List<TopicViewModel> Topics { get; set; } = new List<TopicViewModel>();
        public Guid SelectedFeedbackFormId { get; set; }

    }
    public class TopicViewModel
    {
        public Guid TopicID { get; set; }  // Unique identifier for each topic
        [Required]
        [StringLength(255)]
        public string Name { get; set; }  // Name of the topic
    }
    public class QuestionViewModel
    {
        public int QuestionID { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public QuestionType Type { get; set; }  // Distinguishes between the question types

        // For True/False and SingleAnswerMCQ
        public string? CorrectOptionText { get; set; }

        // For Multi-Answer MCQ
        public List<string>? CorrectOptionTexts { get; set; } = new List<string>();

        public List<OptionViewModel> Options { get; set; } = new List<OptionViewModel>();
    }


    public class OptionViewModel
    {
        public int OptionID { get; set; }

        [Required]
        public string OptionText { get; set; }

        // Indicates whether this option is correct for multi-answer MCQ
        public bool IsCorrect { get; set; }
    }


    public class QuizListViewModel
    {
        public List<QuizViewModel> Quizzes { get; set; } = new List<QuizViewModel>();
    }

    public class AttemptViewModel
    {
        public string UserID { get; set; }
        public Guid QuizID { get; set; }  // Changed from int to Guid
        public List<QuizResponseViewModel> Responses { get; set; }
    }

    public class QuizResponseViewModel
    {
        public int QuestionID { get; set; }

        // For Single Answer MCQ and True/False
        public string SelectedOption { get; set; }

        // For Multi-Answer MCQ
        public List<string> SelectedOptions { get; set; } = new List<string>();
    }


    public class ExcelQuizViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string QuizStart { get; set; } // DateTime in string format
        public List<ExcelQuestionViewModel> Questions { get; set; }
        public List<string> Participants { get; set; } // Emails from the Participants sheet

        public ExcelQuizViewModel()
        {
            Questions = new List<ExcelQuestionViewModel>();
            Participants = new List<string>();
        }
    }

    public class ExcelQuestionViewModel
    {
        public string Description { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
        public string OptionE { get; set; }
        public string CorrectAnswer { get; set; }
    }
}
