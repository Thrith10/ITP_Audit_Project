using System;
using System.Collections.Generic;

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
        public Guid FeedbackQuestionID { get; set; } // Unique ID for the question
        public string QuestionText { get; set; } // Text of the question
        public FeedbackType Type { get; set; } // Type of the question (Yes/No, Rate1To5, ShortAnswer)
    }

    public enum FeedbackType
    {
        YesNo,
        Rate1To5,
        ShortAnswer
    }
}
