using PKFAuditManagement.Models;

namespace PKFAuditManagement.ViewModels
{
    public class QC6FormAdminManagementViewModel
    {
        public List<QC6FormCombinedViewModel> FirstApproverConclusions { get; set; }
        public List<QC6FormCombinedViewModel> SecondApproverConclusions { get; set; }
        public List<QC6Form> AllQC6Forms { get; set; }
    }

    public class QC6FormCombinedViewModel
    {
        public QC6FormConclusion Conclusion { get; set; }
        public QC6Form Form { get; set; }
    }

}
