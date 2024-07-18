using Microsoft.AspNetCore.Mvc.Rendering;
using PKFAuditManagement.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.ViewModels
{
    public class QC35FormViewModel
    {
        [Key]
        public int QC35FormID { get; set; }

        public string? CreatedBy { get; set; }

        public string ClientName { get; set; }

        public DateTime ReportingYearEnd { get; set; }
        public string ManagerName { get; set; }
        public string PartnerName { get; set; }
        public string? ImageFileName { get; set; }

        public IFormFile File { get; set; } // Add this property for file upload
        public string? Status { get; set; }
        public List<string>? AdminEmails { get; set; }

        // List of options for the dropdown
        public List<SelectListItem> AuditFirms { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "PKF-CAP LLP", Text = "PKF-CAP LLP" },
            new SelectListItem { Value = "PKF-HT Khoo PAC", Text = "PKF-HT Khoo PAC" }
        };

        public List<QC35ChecklistItemViewModel> ChecklistItems { get; set; } = new List<QC35ChecklistItemViewModel>();

        //public QC35ChecklistItemViewModel QC35ChecklistItem { get; set; }
    }

    public class QC35ChecklistItemViewModel
    {
        public int QC35ChecklistItemID { get; set; }
        public string? Description { get; set; }
        public string Response { get; set; }

    }
}
