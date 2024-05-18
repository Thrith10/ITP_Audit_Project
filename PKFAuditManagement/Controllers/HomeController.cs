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

        [Authorize(Roles = "Auditor")]
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult TablesGeneral()
        {
            return View();
        }

        public IActionResult DataTables()
        {
            return View();
        }

        public IActionResult ApexCharts()
        {
            return View();
        }

        public IActionResult ChartJs()
        {
            return View();
        }

        public IActionResult ECharts()
        {
            return View();
        }

        public IActionResult Accordion()
        {
            return View();
        }

        public IActionResult Alerts()
        {
            return View();
        }

        public IActionResult Badges()
        {
            return View();
        }

        public IActionResult Breadcrumbs()
        {
            return View();
        }

        public IActionResult Buttons()
        {
            return View();
        }

        public IActionResult Cards()
        {
            return View();
        }

        public IActionResult Carousel()
        {
            return View();
        }

        public IActionResult ListGroup()
        {
            return View();
        }

        public IActionResult Modal()
        {
            return View();
        }

        public IActionResult Pagination()
        {
            return View();
        }

        public IActionResult Progress()
        {
            return View();
        }

        public IActionResult Spinners()
        {
            return View();
        }

        public IActionResult Tabs()
        {
            return View();
        }

        public IActionResult Tooltips()
        {
            return View();
        }

        public IActionResult FormsEditors()
        {
            return View();
        }

        public IActionResult FormsElements()
        {
            return View();
        }

        public IActionResult FormsLayouts()
        {
            return View();
        }

        public IActionResult FormsValidation()
        {
            return View();
        }

        public IActionResult BootstrapIcons()
        {
            return View();
        }

        public IActionResult BoxIcons()
        {
            return View();
        }

        public IActionResult IconsRemix()
        {
            return View();
        }

        public IActionResult Blank()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult FAQ()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
