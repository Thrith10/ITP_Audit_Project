using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PKFAuditManagement.Data;
using PKFAuditManagement.Models;
using System.Diagnostics;

namespace PKFAuditManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Authorize(Roles = "Non-Auditor,User")]
        public IActionResult Dashboard()
        {
            // Fetch the data from ChatbotDocuments table
            var chatbotDocuments = _context.ChatbotDocuments.ToList();

            return View("~/Views/General/Home/Dashboard.cshtml", chatbotDocuments);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
