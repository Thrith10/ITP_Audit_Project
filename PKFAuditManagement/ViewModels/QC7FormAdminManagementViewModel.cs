using PKFAuditManagement.Models;

namespace PKFAuditManagement.ViewModels
{
    public class QC7FormAdminManagementViewModel
    {
        public List<QC7FormCombinedViewModel> FirstApproverConclusions { get; set; }
        public List<QC7FormCombinedViewModel> SecondApproverConclusions { get; set; }
        public List<QC7Form> AllQC7Forms { get; set; }
    }

    public class QC7FormCombinedViewModel
    {
        public QC7FormConclusion Conclusion { get; set; }
        public QC7Form Form { get; set; }
    }

}
