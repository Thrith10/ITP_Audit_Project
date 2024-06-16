using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PKFAuditManagement.Data;
using PKFAuditManagement.Models;
using PKFAuditManagement.Util;
using PKFAuditManagement.ViewModels;
using ClosedXML.Excel;
using System.Data;
using System.Linq.Dynamic.Core;


namespace PKFAuditManagement.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;


        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("GenerateReport")]
        public async Task<IActionResult> GenerateReport([FromBody] QueryRequest request)
        {

            if (request == null || request.SelectedFields == null || !request.SelectedFields.Any())
            {
                return BadRequest("No fields selected.");
            }
            // Start with the base query
            var query = _context.QC6Forms.AsQueryable();

            // Construct the select clause dynamically
            var selectedFields = string.Join(", ", request.SelectedFields);

            // Apply the dynamic select clause
            var selectedQuery = query.Select($"new ({selectedFields})");

            // Execute the query
            var data = await selectedQuery.ToDynamicListAsync();


            // Generate the Excel file using ClosedXML
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Report");

                // Adding Headers
                for (int i = 0; i < request.SelectedFields.Count; i++)
                {
                    worksheet.Cell(1, i + 1).Value = request.SelectedFields[i];
                }

                // Adding Data
                for (int i = 0; i < data.Count; i++)
                {
                    var item = data[i];
                    var itemType = item.GetType();
                    for (int j = 0; j < request.SelectedFields.Count; j++)
                    {
                        var fieldName = request.SelectedFields[j];
                        var fieldValue = itemType.GetProperty(fieldName).GetValue(item, null);
                        worksheet.Cell(i + 2, j + 1).Value = fieldValue;
                    }
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report.xlsx");
                }
            }

        }

    }

}