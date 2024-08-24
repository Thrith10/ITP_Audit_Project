namespace PKFAuditManagement.Models
{
    public class QCDocument
    {
        public int Id { get; set; }
        public string DocumentType { get; set; } 
        public string FileName { get; set; } 
        public string S3Key { get; set; } // S3 path for the file

        // Nullable foreign keys
        public int? QC6FormID { get; set; }
        public int? QC7FormID { get; set; }

    }
}
