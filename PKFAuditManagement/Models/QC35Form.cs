using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.Models
{
    public class QC35Form
    {
        [Key]
        public int QC35FormID { get; set; }
        public string CreatedBy { get; set; }
        public string PreparedBy { get; set; }
        public string? FileReference { get; set; }
        public string? ClientName { get; set; }
        public DateTime? ReportingYearEnd { get; set; }
        public string? PartnerName { get; set; }
        public string? ManagerName { get; set; }
        public string? ImageFileName { get; set; } 
        public string? Status { get; set; }
        public string? RejectionReason { get; set; }
        public string? FirstApprover { get; set; }
        public string? SecondApprover { get; set; }
        public bool? IsFirstApproved { get; set; }
        public bool? IsSecondApproved { get; set; }
        public ICollection<QC35ChecklistItem> ChecklistItems { get; set; }
    }

    public class QC35ChecklistItem
    {
        [Key]
        public int QC35ChecklistItemID { get; set; }
        public int QC35FormID { get; set; }
        public string? Description { get; set; }
        public string Response { get; set; }
    }
}
