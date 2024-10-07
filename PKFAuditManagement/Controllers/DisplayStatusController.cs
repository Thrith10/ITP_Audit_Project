using Microsoft.AspNetCore.Mvc;
using PKFAuditManagement.ViewModels;
using PKFAuditManagement.Models;
using System.Linq;
using PKFAuditManagement.Data;

namespace PKFAuditManagement.Controllers
{
    public class DisplayStatusController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DisplayStatusController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult DisplayStatus()
        {
            // Retrieve QC6 forms to use as the client identifier
            var qc6Forms = _context.QC6Forms.Where(e => !e.IsTemplate).ToList();

            var statusViewModels = qc6Forms.Select(qc6 => {
                var qc7Form = _context.QC7Forms.FirstOrDefault(q => q.Client == qc6.ProspectiveClient);
                var qc7FormConclusion = qc7Form != null ? _context.QC7FormConclusions.FirstOrDefault(c => c.QC7FormID == qc7Form.QC7FormID) : null;

                return new DisplayStatusViewModel
                {
                    ClientName = qc6.ProspectiveClient ?? "Unknown",
                    FinancialYearEnd = qc6.PeriodEnded,  // Added Financial Year End from QC6
                    QC6FirstApprover = _context.QC6FormConclusions.FirstOrDefault(c => c.QC6FormID == qc6.QC6FormID)?.PreparedBy ?? "Not Assigned",  // Added QC6 First Approver
                    QC6SecondApprover = _context.QC6FormConclusions.FirstOrDefault(c => c.QC6FormID == qc6.QC6FormID)?.MPHODQMPApprovedBy ?? "Not Assigned",  // Added QC6 Second Approver
                    QC7FirstApprover = qc7FormConclusion?.EMPreparedBy ?? "Not Assigned",  // Added QC7 First Approver
                    QC7SecondApprover = qc7FormConclusion?.MPHODQMPApprovedBy ?? "Not Assigned",  // Added QC7 Second Approver
                    QC6Status = qc6.Status ?? "Null",
                    QC7Status = qc7Form?.Status ?? "Null",
                    QC35Status = _context.QC35Forms.FirstOrDefault(q => q.ClientName == qc6.ProspectiveClient)?.Status ?? "Null",
                    SignedFSStatus = _context.SignedFSForm.FirstOrDefault(q => q.UserEmail == qc6.ProspectiveClient)?.IsProcessed == true ? "Processed" : "Not Uploaded"
                };
            }).ToList();

            // Point to the new view in the DisplayStatus folder
            return View("~/Views/General/DisplayStatus/DisplayStatus.cshtml", statusViewModels);
        }
    }
}
