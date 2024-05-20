using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PKFAuditManagement.Controllers
{
    public class QC7FormController : Controller
    {
        [Authorize(Roles = "Auditor")]
        public IActionResult QC7FormManagement()
        {
            return View("~/Views/General/QC7/QC7FormManagement.cshtml");
        }

        [Authorize(Roles = "Auditor")]
        public IActionResult QC7FormCreation()
        {
            return View("~/Views/General/QC7/QC7FormCreation.cshtml");
        }
    }
}
