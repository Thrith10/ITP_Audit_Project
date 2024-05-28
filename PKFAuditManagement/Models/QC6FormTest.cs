namespace PKFAuditManagement.Models
{
    public class QC6FormTest
    {
        public int QC6FormTestID { get; set; }
        public int QC6FormID { get; set; }
        public int QC6FormTestDescriptionID { get; set; }
        public string? Reference { get; set; }
        public string? SignOffBy { get; set; }
        public DateTime? SignOffDate { get; set; }
        public string? Comments { get; set; }

        // Navigation properties
        public QC6Form QC6Form { get; set; }
        public QC6FormTestDescription QC6FormTestDescription { get; set; }
    }

}
