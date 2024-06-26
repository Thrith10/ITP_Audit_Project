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

        [Required]
        public string AuditFirmName { get; set; }

        [Required]
        public string ClientName { get; set; }

        [Required]
        public DateTime ReportingYearEnd { get; set; }
        public string ManagerName { get; set; }
        public string PartnerName { get; set; }
        public string PartnerInitial { get; set; }
        public DateTime? PartnerDate { get; set; }
        public string AuditStaffName { get; set; }
        public string AuditStaffInitial { get; set; }
        public DateTime? AuditDate { get; set; }
        public string AdminStaffName { get; set; }
        public string AdminStaffInitial { get; set; }
        public DateTime? AdminDate { get; set; }

        public string? Status { get; set; }

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
        [Required]
        public int QC35ChecklistItemID { get; set; }
        public string? Description { get; set; }
        public string Response { get; set; }
        public string ManagerInitial { get; set; }
        public string PartnerInitial { get; set; }

    }
}
