using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.ViewModels
{
    public class FeedbackFormViewModel
    {
        public Guid FeedbackFormID { get; set; }
        public string Title { get; set; } // Title of the feedback form
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; } // UserID of the user who created the form

        public List<FeedbackQuestionViewModel> Questions { get; set; } = new List<FeedbackQuestionViewModel>();
    }

    public class FeedbackFormListViewModel
    {
        public List<FeedbackFormViewModel> FeedbackForms { get; set; } = new List<FeedbackFormViewModel>();
    }

    public class FeedbackQuestionViewModel
    {

        public string QuestionText { get; set; } // Text of the question
        public FeedbackType Type { get; set; } // Type of the question (Yes/No, Rate1To5, ShortAnswer)
    }
    public class EditFeedbackFormViewModel
    {

        public Guid FeedbackFormID { get; set; } // ID of the feedback form being edited

        [Required(ErrorMessage = "Feedback form title is required.")]
        [Display(Name = "Feedback Form Title")]
        public string Title { get; set; } // Title of the feedback form

        [Display(Name = "Created Date")]
        public DateTime? CreatedDate { get; set; } // Date the feedback form was created

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; } // UserID of the user who created the form

        // List of questions associated with the feedback form
        public List<FeedbackQuestionViewModel> Questions { get; set; } = new List<FeedbackQuestionViewModel>();
    }

    public enum FeedbackType
    {
        YesNo,
        Rate1To5,
        ShortAnswer
    }
}
