using System;
using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.Models
{
    public class SignedFSForm
    {
        [Key]
        public int Id { get; set; }
        public string Client { get; set; }
        public DateTime AuditedReportDate { get; set; }
        public string PartnerEmail { get; set; }
        public string UserEmail { get; set; }
        public string FilePath { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public string EmailType { get; set; } // Field for storing the email type
        public string EmailBody { get; set; } // Field for storing the email body
        public bool IsProcessed { get; set; }
    }
}
