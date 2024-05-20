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
            return View("~/Views/General/Home/Dashboard.cshtml");
        }
        public IActionResult Login()
        {
            return View("~/Views/General/Home/Login.cshtml");
        }

        public IActionResult Privacy()
        {
            return View("~/Views/General/Home/Privacy.cshtml");
        }

        public IActionResult TablesGeneral()
        {
            return View("~/Views/General/Home/TablesGeneral.cshtml");
        }

        public IActionResult DataTables()
        {
            return View("~/Views/General/Home/DataTables.cshtml");
        }

        public IActionResult ApexCharts()
        {
            return View("~/Views/General/Home/ApexCharts.cshtml");
        }

        public IActionResult ChartJs()
        {
            return View("~/Views/General/Home/ChartJs.cshtml");
        }

        public IActionResult ECharts()
        {
            return View("~/Views/General/Home/ECharts.cshtml");
        }

        public IActionResult Accordion()
        {
            return View("~/Views/General/Home/Accordion.cshtml");
        }

        public IActionResult Alerts()
        {
            return View("~/Views/General/Home/Alerts.cshtml");
        }

        public IActionResult Badges()
        {
            return View("~/Views/General/Home/Badges.cshtml");
        }

        public IActionResult Breadcrumbs()
        {
            return View("~/Views/General/Home/Breadcrumbs.cshtml");
        }

        public IActionResult Buttons()
        {
            return View("~/Views/General/Home/Buttons.cshtml");
        }

        public IActionResult Cards()
        {
            return View("~/Views/General/Home/Cards.cshtml");
        }

        public IActionResult Carousel()
        {
            return View("~/Views/General/Home/Carousel.cshtml");
        }

        public IActionResult ListGroup()
        {
            return View("~/Views/General/Home/ListGroup.cshtml");
        }

        public IActionResult Modal()
        {
            return View("~/Views/General/Home/Modal.cshtml");
        }

        public IActionResult Pagination()
        {
            return View("~/Views/General/Home/Pagination.cshtml");
        }

        public IActionResult Progress()
        {
            return View("~/Views/General/Home/Progress.cshtml");
        }

        public IActionResult Spinners()
        {
            return View("~/Views/General/Home/Spinners.cshtml");
        }

        public IActionResult Tabs()
        {
            return View("~/Views/General/Home/Tabs.cshtml");
        }

        public IActionResult Tooltips()
        {
            return View("~/Views/General/Home/Tooltips.cshtml");
        }

        public IActionResult FormsEditors()
        {
            return View("~/Views/General/Home/FormsEditors.cshtml");
        }

        public IActionResult FormsElements()
        {
            return View("~/Views/General/Home/FormsElements.cshtml");
        }

        public IActionResult FormsLayouts()
        {
            return View("~/Views/General/Home/FormsLayouts.cshtml");
        }

        public IActionResult FormsValidation()
        {
            return View("~/Views/General/Home/FormsValidation.cshtml");
        }

        public IActionResult BootstrapIcons()
        {
            return View("~/Views/General/Home/BootstrapIcons.cshtml");
        }

        public IActionResult BoxIcons()
        {
            return View("~/Views/General/Home/BoxIcons.cshtml");
        }

        public IActionResult IconsRemix()
        {
            return View("~/Views/General/Home/IconsRemix.cshtml");
        }

        public IActionResult Blank()
        {
            return View("~/Views/General/Home/Blank.cshtml");
        }

        public IActionResult Contact()
        {
            return View("~/Views/General/Home/Contact.cshtml");
        }

        public IActionResult Profile()
        {
            return View("~/Views/General/Home/Profile.cshtml");
        }

        public IActionResult FAQ()
        {
            return View("~/Views/General/Home/FAQ.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
