namespace PKFAuditManagement.Models
{
    public class QC7SubForm
    {
        public int QC7SubFormID { get; set; }
        public required string SubFormType { get; set; }
        public ICollection<QC7FormObjective> QC7FormObjectives { get; set; } // One-to-Many
    }
}
