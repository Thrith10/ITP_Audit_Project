namespace PKFAuditManagement.Models
{
    public class QC7FormTestDescription
    {
        public int QC7FormTestDescriptionID { get; set; }
        public int QC7FormObjectiveID { get; set; }
        public int DescriptionNo { get; set; }
        public required string Description { get; set; }
        public QC7FormObjective QC7FormObjective { get; set; }
        public ICollection<QC7FormTest> QC7FormTests { get; set; } // One-to-Many
    }

}
