namespace PKFAuditManagement.Models
{
    public class ChatbotDocument
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public DateTime DateAdded { get; set; }
        public required string FilePath { get; set; }
    }
}
