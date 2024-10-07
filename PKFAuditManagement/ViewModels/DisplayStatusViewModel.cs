namespace PKFAuditManagement.ViewModels
{
    public class DisplayStatusViewModel
    {
        public string ClientName { get; set; }
        public DateTime? FinancialYearEnd { get; set; } // Added Financial Year End
        public string QC6FirstApprover { get; set; } // Added QC6 First Approver
        public string QC6SecondApprover { get; set; } // Added QC6 Second Approver
        public string QC7FirstApprover { get; set; } // Added QC7 First Approver
        public string QC7SecondApprover { get; set; } // Added QC7 Second Approver
        public string QC6Status { get; set; }
        public string QC7Status { get; set; }
        public string QC35Status { get; set; }
        public string SignedFSStatus { get; set; }
    }
}
