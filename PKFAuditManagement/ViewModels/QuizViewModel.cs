using PKFAuditManagement.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.ViewModels
{
    public class ParticipantViewModel
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public DateTime AttemptDate { get; set; }  

        public int TotalScore { get; set; }
    }
    public class QuizViewModel
    {
        public int QuizID { get; set; }
        public bool CanEdit { get; set; } 
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }
        public DateTime? AttemptDate { get; set; }


        public List<QuestionViewModel> Questions { get; set; } = new List<QuestionViewModel>();
        public List<ParticipantViewModel>? Participants { get; set; }
        public string? QRImageURL { get; set; } // Add this property to hold the QR code image URL

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
        public int QuizID { get; set; }
        public List<QuizResponseViewModel> Responses { get; set; }
    }

    public class QuizResponseViewModel
    {
        public int QuestionID { get; set; }
        public string SelectedOption { get; set; }

    }


}
