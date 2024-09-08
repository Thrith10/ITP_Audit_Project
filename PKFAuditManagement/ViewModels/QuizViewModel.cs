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

        public bool IsRequired { get; set; } = false;  // Default is false, similar to the Participants model
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

        public DateTime? AttemptDate { get; set; }

        public List<QuestionViewModel> Questions { get; set; } = new List<QuestionViewModel>();
        public List<ParticipantViewModel>? Participants { get; set; }
        public string? QRImageURL { get; set; }
    }

    public class QuestionViewModel
    {
        public int QuestionID { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string CorrectOptionText { get; set; }

        public List<OptionViewModel> Options { get; set; } = new List<OptionViewModel>();
    }

    public class OptionViewModel
    {
        public int OptionID { get; set; }

        [Required]
        public string OptionText { get; set; }
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
        public string SelectedOption { get; set; }
    }
}
