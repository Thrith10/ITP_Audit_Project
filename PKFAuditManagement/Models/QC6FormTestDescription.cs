namespace PKFAuditManagement.Models
{
    public class QC6FormTestDescription
    {
        public int QC6FormTestDescriptionID { get; set; }
        public int QC6FormObjectiveID { get; set; }
        public int DescriptionNo { get; set; }
        public required string Description { get; set; }
        public QC6FormObjective QC6FormObjective { get; set; }
        public ICollection<QC6FormTest> QC6FormTests { get; set; } // One-to-Many
    }

}
