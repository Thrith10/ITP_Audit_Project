namespace PKFAuditManagement.Models
{
    public class QCDocument
    {
        public int Id { get; set; }
        public string DocumentType { get; set; } 
        public string FileName { get; set; } 

        // Nullable foreign keys
        public int? QC6FormID { get; set; }
        public int? QC7FormID { get; set; }

    }
}
