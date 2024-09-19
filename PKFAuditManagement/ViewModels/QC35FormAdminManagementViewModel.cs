using PKFAuditManagement.Models;

namespace PKFAuditManagement.ViewModels
{
    public class QC35FormAdminManagementViewModel
    {
        public List<QC35Form> AllQC35Forms { get; set; } // All forms
        public List<QC35Form> FirstApproverForms { get; set; } // Forms for the first approver
        public List<QC35Form> SecondApproverForms { get; set; } // Forms for the second approver
    }

}
