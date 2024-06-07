namespace PKFAuditManagement.Models
{
    public class QC6FormFeeDetail
    {
        public int QC6FormFeeDetailID { get; set; }
        public int QC6FormID { get; set; }
        public required string NatureOfService { get; set; }
        public required decimal Fee { get; set; }
        public string? OtherService { get; set; }
        public QC6Form QC6Form { get; set; }  
    }
}
