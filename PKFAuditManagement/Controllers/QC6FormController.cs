using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PKFAuditManagement.Controllers
{
    public class QC6FormController : Controller
    {
        [Authorize(Roles = "Auditor")]
        public IActionResult QC6FormManagement()
        {
            return View("~/Views/General/QC6/QC6FormManagement.cshtml");
        }

        [Authorize(Roles = "Auditor")]
        public IActionResult QC6FormCreation()
        {
            return View("~/Views/General/QC6/QC6FormCreation.cshtml");
        }
    }
}
