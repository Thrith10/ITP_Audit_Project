using Microsoft.EntityFrameworkCore;

namespace PKFAuditManagement.Models
{
    public class QueryRequest
    {
        public List<string> SelectedFields { get; set; }
    }

}