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
        public DateTime FinancialYearEnd { get; set; }
        public string PartnerEmail { get; set; }
        public string UserEmail { get; set; }
        public string FilePath { get; set; }
    }
}
