using Microsoft.AspNetCore.Mvc;

namespace PKFAuditManagement.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
