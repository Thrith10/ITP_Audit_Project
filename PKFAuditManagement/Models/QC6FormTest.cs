namespace PKFAuditManagement.Models
{
    public class QC6FormTest
    {
        public int QC6FormTestID { get; set; }
        public int QC6FormID { get; set; }
        public int QC6FormTestDescriptionID { get; set; }
        public required string SignOffBy { get; set; }
        public DateTime SignOffDate { get; set; }
        public string? Comments { get; set; }
    }

}
