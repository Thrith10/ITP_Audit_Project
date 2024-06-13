using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PKFAuditManagement.Data;
using PKFAuditManagement.Models;
using PKFAuditManagement.Services;
using PKFAuditManagement.Util;
using PKFAuditManagement.ViewModels;
using System.Data;
using System.Globalization;

namespace PKFAuditManagement.Controllers
{
    public class QC35FormController : Controller
    {
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public QC35FormController(IUserService userService, ApplicationDbContext context, UserManager<CustomUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userService = userService;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> QC35FormManagement()
        {
            // Get the current user's ID
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id;

            // Retrieve engagement data from database
            var qc35forms = _context.QC35Forms.Where(e => e.CreatedBy.Equals(userId)).ToList();
            return View("~/Views/General/QC35/QC35FormManagement.cshtml", qc35forms);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult QC35FormApprovalManagement()
        {
            // Retrieve engagement data from database
            var qc35forms = _context.QC35Forms.ToList();
            return View("~/Views/General/QC35/QC35FormApprovalManagement.cshtml", qc35forms);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("/QC35Form/ApproveQC35Form/{id}")]
        public IActionResult ApproveQC35Form(int id)
        {
            // Retrieve engagement data from the database
            var engagement = _context.QC35Forms.FirstOrDefault(e => e.QC35FormID == id);

            if (engagement == null)
            {
                return NotFound();
            }

            // Update the engagement status to "Approved"
            engagement.Status = "Approved";
            _context.SaveChanges();

            return RedirectToAction("QC35FormApprovalManagement", "QC35Form");
        }

        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> ViewQC35Form(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            var qc35Form = await _context.QC35Forms
                .Include(f => f.ChecklistItems) // Include the related checklist items
                .FirstOrDefaultAsync(f => f.QC35FormID == id);

            if (qc35Form == null)
            {
                return NotFound();
            }

            var viewModel = new QC35FormViewModel
            {
                QC35FormID = qc35Form.QC35FormID,
                CreatedBy = qc35Form.CreatedBy,
                AuditFirmName = qc35Form.AuditFirmName,
                ClientName = qc35Form.ClientName,
                ReportingYearEnd = (DateTime)qc35Form.ReportingYearEnd,
                PartnerName = qc35Form.PartnerName,
                PartnerInitial = qc35Form.PartnerInitial,
                PartnerDate = qc35Form.PartnerDate,
                AuditStaffName = qc35Form.AuditStaffName,
                AuditStaffInitial = qc35Form.AuditStaffInitial,
                AuditDate = qc35Form.AuditDate,
                AdminStaffName = qc35Form.AdminStaffName,
                AdminStaffInitial = qc35Form.AdminStaffInitial,
                AdminDate = qc35Form.AdminDate,
                Status = qc35Form.Status,
                ChecklistItems = qc35Form.ChecklistItems.Select(ci => new QC35ChecklistItemViewModel
                {
                    QC35ChecklistItemID = ci.QC35ChecklistItemID,
                    Description = ci.Description,
                    Response = ci.Response,
                    ManagerInitial = ci.ManagerInitial,
                    PartnerInitial = ci.PartnerInitial
                }).ToList()
            };

            ViewBag.Roles = roles;

            return View("~/Views/General/QC35/ViewQC35Form.cshtml", viewModel);
        }

        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> QC35FormCreationAsync()
        {

            // Retrieve user email
            var userEmail = await _userService.GetUserEmailAsync(User);

            var viewModel = new QC35FormViewModel
            {
                CreatedBy = userEmail,
                AuditFirms = new List<SelectListItem>
        {
            new SelectListItem { Value = "PKF-CAP LLP", Text = "PKF-CAP LLP" },
            new SelectListItem { Value = "PKF-HT Khoo PAC", Text = "PKF-HT Khoo PAC" }
        },
                ChecklistItems = new List<QC35ChecklistItemViewModel>
        {
            new QC35ChecklistItemViewModel { Description = "No. of working paper files" },
            new QC35ChecklistItemViewModel { Description = "Working papers are transferred from arch files to paper files" },
            new QC35ChecklistItemViewModel { Description = "Working paper files are numbered sequentially" },
            new QC35ChecklistItemViewModel { Description = "All working papers in each file is complete (Manager to initial all working papers)" },
            new QC35ChecklistItemViewModel { Description = "Date of Audit Report" },
            new QC35ChecklistItemViewModel { Description = "Date of approval of files for archival is within 60 days from date of Audit Report" },
            new QC35ChecklistItemViewModel { Description = "Date of approval and confirmation that CaseWare Audit files has been locked down within 60 days from the date of the Audit Report (if applicable). Refer to the screenshot of CaseWare below." }
        }
            };


            return View("~/Views/General/QC35/QC35FormCreation.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQC35Form(QC35FormViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            if (!ModelState.IsValid)
            {
                // Access validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors);

                // Pass the errors to the view
                ViewBag.Errors = errors;

                return View("~/Views/General/QC35/QC35FormCreation.cshtml", viewModel);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Get the current user's ID
                    var userId = user?.Id;

                    var qc35Form = new QC35Form
                    {
                        CreatedBy = userId,
                        AuditFirmName = viewModel.AuditFirmName,
                        ClientName = viewModel.ClientName,
                        ReportingYearEnd = viewModel.ReportingYearEnd,
                        PartnerName = viewModel.PartnerName,
                        PartnerInitial = viewModel.PartnerInitial,
                        PartnerDate = viewModel.PartnerDate,
                        AuditStaffName = viewModel.AuditStaffName,
                        AuditStaffInitial = viewModel.AuditStaffInitial,
                        AuditDate = viewModel.AuditDate,
                        AdminStaffName = viewModel.AdminStaffName,
                        AdminStaffInitial = viewModel.AdminStaffInitial,
                        AdminDate = viewModel.AdminDate,
                        Status = "Pending"
                    };

                    _context.QC35Forms.Add(qc35Form);
                    await _context.SaveChangesAsync();

                    foreach (var item in viewModel.ChecklistItems)
                    {
                        var checklistItem = new QC35ChecklistItem
                        {
                            QC35FormID = qc35Form.QC35FormID,
                            Description = item.Description,
                            Response = item.Response,
                            ManagerInitial = item.ManagerInitial,
                            PartnerInitial = item.PartnerInitial
                        };
                        _context.QC35ChecklistItems.Add(checklistItem);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    if (roles.Contains("Admin"))
                    {
                        // Redirect to admin-specific page
                        return RedirectToAction("QC35FormApprovalManagement", "QC35Form");
                    }
                    else
                    {
                        // Redirect to user-specific page
                        return RedirectToAction("QC35FormManagement", "QC35Form");
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ViewBag.ErrorMessage = "An error occurred while processing your request. Please try again.";
                    return View("~/Views/General/QC35/QC35FormCreation.cshtml", viewModel);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQC35Form(QC35FormViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            if (!ModelState.IsValid)
            {
                // Access validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors);

                // Pass the errors to the view
                ViewBag.Errors = errors;

                return View("~/Views/General/QC35/ViewQC35Form.cshtml", viewModel);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Get the existing QC35Form from the database
                    var qc35Form = await _context.QC35Forms.Include(f => f.ChecklistItems)
                        .FirstOrDefaultAsync(f => f.QC35FormID == viewModel.QC35FormID);

                    if (qc35Form == null)
                    {
                        return NotFound();
                    }

                    // Check if the request is a PUT operation
                    var isPutRequest = Request.Method.Equals(HttpMethod.Put.ToString(), StringComparison.OrdinalIgnoreCase)
                   || Request.Form.ContainsKey("_method") && string.Equals(Request.Form["_method"], "PUT", StringComparison.OrdinalIgnoreCase);


                    // Update the QC35Form fields based on the request method
                    if (isPutRequest)
                    {

                        // Update the QC35Form fields
                        qc35Form.AuditFirmName = viewModel.AuditFirmName;
                        qc35Form.ClientName = viewModel.ClientName;
                        qc35Form.ReportingYearEnd = viewModel.ReportingYearEnd;
                        qc35Form.PartnerName = viewModel.PartnerName;
                        qc35Form.PartnerInitial = viewModel.PartnerInitial;
                        qc35Form.PartnerDate = viewModel.PartnerDate;
                        qc35Form.AuditStaffName = viewModel.AuditStaffName;
                        qc35Form.AuditStaffInitial = viewModel.AuditStaffInitial;
                        qc35Form.AuditDate = viewModel.AuditDate;
                        qc35Form.AdminStaffName = viewModel.AdminStaffName;
                        qc35Form.AdminStaffInitial = viewModel.AdminStaffInitial;
                        qc35Form.AdminDate = viewModel.AdminDate;

                        // Update the ChecklistItems
                        foreach (var item in viewModel.ChecklistItems)
                        {
                            var checklistItem = qc35Form.ChecklistItems
                                .FirstOrDefault(ci => ci.QC35ChecklistItemID == item.QC35ChecklistItemID);

                            if (checklistItem != null)
                            {
                                checklistItem.Description = item.Description;
                                checklistItem.Response = item.Response;
                                checklistItem.ManagerInitial = item.ManagerInitial;
                                checklistItem.PartnerInitial = item.PartnerInitial;
                            }
                            else
                            {
                                // Add new ChecklistItem if it doesn't exist
                                qc35Form.ChecklistItems.Add(new QC35ChecklistItem
                                {
                                    Description = item.Description,
                                    Response = item.Response,
                                    ManagerInitial = item.ManagerInitial,
                                    PartnerInitial = item.PartnerInitial
                                });
                            }
                        }
                    }
                
                    
                    _context.QC35Forms.Update(qc35Form);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();


                    if (roles.Contains("Admin"))
                    {
                        // Redirect to admin-specific page
                        return RedirectToAction("QC35FormApprovalManagement", "QC35Form");
                    }
                    else
                    {
                        // Redirect to user-specific page
                        return RedirectToAction("QC35FormManagement", "QC35Form");
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ViewBag.ErrorMessage = "An error occurred while processing your request. Please try again.";
                    return View("~/Views/General/QC35/QC35FormCreation.cshtml", viewModel);
                }
            }
        }

        [HttpDelete]
        [Authorize(Roles = "User,Admin")]
        public IActionResult DeleteQC35Form(int id)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var qc35Form = _context.QC35Forms.Find(id);

                if (qc35Form == null)
                {
                    return NotFound();
                }

                // Delete from QC35ChecklistItems
                var checklistItems = _context.QC35ChecklistItems.Where(c => c.QC35FormID == id);
                _context.QC35ChecklistItems.RemoveRange(checklistItems);

                // Delete the QC35Form itself
                _context.QC35Forms.Remove(qc35Form);

                _context.SaveChanges();

                transaction.Commit(); // Commit the transaction if all operations are successful

                return NoContent(); // Respond with 204 No Content
            }
            catch (Exception)
            {
                transaction.Rollback(); // Roll back the transaction if an error occurs
                throw;
            }
        }

    }
}
