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

        //public string? PartnerInitial { get; set; }
        //public DateTime? PartnerDate { get; set; }
        //public string? AuditStaffName { get; set; }
        //public string? AuditStaffInitial { get; set; }
        //public DateTime? AuditDate { get; set; }
        //public string? AdminStaffName { get; set; }
        //public string? AdminStaffInitial { get; set; }
        //public DateTime? AdminDate { get; set; }
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
        //public string ManagerInitial { get; set; }
        //public string PartnerInitial { get; set; }
    }
}
