namespace PKFAuditManagement.Models
{
    public class QC6SubForm
    {
        public int QC6SubFormID { get; set; }
        public string? SubFormType { get; set; }
        public ICollection<QC6FormObjective> QC6FormObjectives { get; set; } // One-to-Many
    }
}
