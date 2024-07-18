using Microsoft.AspNetCore.Mvc;
using PKFAuditManagement.Data;

namespace PKFAuditManagement.Controllers
{
    public class TagsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public TagsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetTags()
        {
            // Retrieve all unique client names
            var tags = _context.QC6Forms
                .Select(q => q.ProspectiveClient)
                .Distinct()
                .ToList();

            return Ok(tags);
        }

        [HttpGet]
        public IActionResult GetQC7Tags()
        {
            // Retrieve all unique client names
            var tags = _context.QC7Forms
                .Select(q => q.Client)
                .Distinct()
                .ToList();

            return Ok(tags);
        }
    }
}
