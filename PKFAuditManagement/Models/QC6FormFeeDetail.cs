using Microsoft.EntityFrameworkCore;

namespace PKFAuditManagement.Models
{
    public class QC6FormFeeDetail
    {
        public int QC6FormFeeDetailID { get; set; }
        public int QC6FormID { get; set; }
        public required string NatureOfService { get; set; }
        [Precision(18, 2)]
        public required decimal Fee { get; set; }
        public string? OtherService { get; set; }
    }
}
