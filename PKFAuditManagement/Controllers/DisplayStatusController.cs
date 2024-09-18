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

            var statusViewModels = qc6Forms.Select(qc6 => new DisplayStatusViewModel
            {
                ClientName = qc6.ProspectiveClient ?? "Unknown",
                QC6Status = qc6.Status ?? "Null",
                QC7Status = _context.QC7Forms.FirstOrDefault(q => q.Client == qc6.ProspectiveClient)?.Status ?? "Null",
                QC35Status = _context.QC35Forms.FirstOrDefault(q => q.ClientName == qc6.ProspectiveClient)?.Status ?? "Null",
                SignedFSStatus = _context.SignedFSForm.FirstOrDefault(q => q.UserEmail == qc6.ProspectiveClient)?.IsProcessed == true ? "Processed" : "Null"
            }).ToList();

            // Point to the new view in the DisplayStatus folder
            return View("~/Views/General/DisplayStatus/DisplayStatus.cshtml", statusViewModels);
        }
    }
}
