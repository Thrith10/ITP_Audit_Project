using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PKFAuditManagement.Models;
using System.Diagnostics;

namespace PKFAuditManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Authorize(Roles = "Non-Auditor,User")]
        public IActionResult Dashboard()
        {
            return View("~/Views/General/Home/Dashboard.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
