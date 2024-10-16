using Microsoft.EntityFrameworkCore;

namespace PKFAuditManagement.Models
{
    public class QC7FormFeeDetail
    {
        public int QC7FormFeeDetailID { get; set; }
        public int QC7FormID { get; set; }
        public required string NatureOfService { get; set; }
        [Precision(18, 2)]
        public required decimal Fee { get; set; }
        public string? OtherService { get; set; }
    }
}
