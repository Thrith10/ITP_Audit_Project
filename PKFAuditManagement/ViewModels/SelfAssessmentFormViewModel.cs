using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.ViewModels
{
    public class SelfAssessmentFormViewModel
    {
        public Guid SelfAssessmentFormID { get; set; }
        public string Title { get; set; } // Title of the self-assessment form
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; } // UserID of the user who created the form

        public List<SelfAssessmentQuestionViewModel> Questions { get; set; } = new List<SelfAssessmentQuestionViewModel>();
    }

    public class SelfAssessmentFormListViewModel
    {
        public List<SelfAssessmentFormViewModel> SelfAssessmentForms { get; set; } = new List<SelfAssessmentFormViewModel>();
    }

    public class SelfAssessmentQuestionViewModel
    {
        public string QuestionText { get; set; } // Text of the question
        public SelfAssessmentType Type { get; set; } // Type of the question (Rate1To5)
    }

    public class EditSelfAssessmentFormViewModel
    {
        public Guid SelfAssessmentFormID { get; set; } // ID of the self-assessment form being edited

        [Required(ErrorMessage = "Self-assessment form title is required.")]
        [Display(Name = "Self-Assessment Form Title")]
        public string Title { get; set; } // Title of the self-assessment form

        [Display(Name = "Created Date")]
        public DateTime? CreatedDate { get; set; } // Date the self-assessment form was created

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; } // UserID of the user who created the form

        // List of questions associated with the self-assessment form
        public List<SelfAssessmentQuestionViewModel> Questions { get; set; } = new List<SelfAssessmentQuestionViewModel>();
    }

    public enum SelfAssessmentType
    {
        Rate1To5
    }
}
