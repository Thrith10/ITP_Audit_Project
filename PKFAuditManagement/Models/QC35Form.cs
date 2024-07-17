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
        //public string? AuditFirmName { get; set; }
        public string? ClientName { get; set; }
        public DateTime? ReportingYearEnd { get; set; }
        public string? PartnerName { get; set; }
        public string? ManagerName { get; set; }
        public string? ImageFileName { get; set; } 
        public string? Status { get; set; }

        public ICollection<QC35ChecklistItem> ChecklistItems { get; set; }
    }

    public class QC35ChecklistItem
    {
        [Key]
        public int QC35ChecklistItemID { get; set; }
        public int QC35FormID { get; set; }
        public string Description { get; set; }
        public string Response { get; set; }
    }
}
