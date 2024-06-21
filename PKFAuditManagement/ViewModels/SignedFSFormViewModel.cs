using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PKFAuditManagement.ViewModels
{
    public class SignedFSFormViewModel
    {
        [Required]
        [Display(Name = "Audited Report Date")]
        public DateTime AuditedReportDate { get; set; }

        [Required]
        [Display(Name = "Partner Email")]
        public string PartnerEmail { get; set; }

        [EmailAddress]
        [Display(Name = "Your Email")]
        public string UserEmail { get; set; }

        [Required]
        [Display(Name = "Financial Statement (PDF)")]
        public IFormFile FinancialStatement { get; set; }

        public List<string> PartnerEmailOptions { get; set; } = new List<string>
        {
            "russelpoon25@gmail.com",
            "bookhaven50@gmail.com",
            "khoojunwei6671@gmail.com"
        };
    }
}
