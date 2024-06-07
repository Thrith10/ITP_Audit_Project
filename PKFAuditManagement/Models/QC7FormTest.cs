namespace PKFAuditManagement.Models
{
    public class QC7FormTest
    {
        public int QC7FormTestID { get; set; }
        public int QC7FormID { get; set; }
        public int QC7FormTestDescriptionID { get; set; }
        public required string SignOffBy { get; set; }
        public DateTime SignOffDate { get; set; }
        public string? Comments { get; set; }
    }

}
