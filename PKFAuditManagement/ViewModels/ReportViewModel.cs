using System;
using System.Collections.Generic;
using PKFAuditManagement.Models;

namespace PKFAuditManagement.ViewModels
{
    public class ReportViewModel
    {
        // Properties for forms 
        public List<QC6Form> QC6Forms { get; set; }
        public List<QC7Form> QC7Forms { get; set; }

        // Additional properties for form selection and report generation 
        public string SelectedFormType { get; set; }  // Added to track selected form type 
        public List<int> SelectedFormIds { get; set; } // Assuming this is for selected form IDs 
        public List<string> SelectedFields { get; set; } // Assuming this is for selected fields 

        // Placeholder for report data (you can adjust this based on actual report structure) 
        public string ReportData { get; set; }

        // Error message if any 
        public string ErrorMessage { get; set; }
    }
}