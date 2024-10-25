using Amazon.S3;
using Amazon.S3.Model;
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
using PKFAuditManagement.Interface;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Text;
using DocumentFormat.OpenXml.Wordprocessing;

namespace PKFAuditManagement.Controllers
{
    public class QC35FormController : Controller
    {
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly string _bucketName;
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly Interface.IEmailSender _emailSender;

        public QC35FormController(IUserService userService, ApplicationDbContext context, 
            UserManager<CustomUser> userManager, RoleManager<IdentityRole> roleManager, 
            IConfiguration configuration, Interface.IEmailSender emailSender)
        {
            _userService = userService;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _bucketName = _configuration["AWS_BUCKET_NAME"];
            _accessKey = _configuration["AWS_ACCESS_KEY"];
            _secretKey = _configuration["AWS_SECRET_KEY"];
            _emailSender = emailSender;
        }

        [Authorize(Roles = "User,Non-Auditor")]
        public async Task<IActionResult> QC35FormManagement()
        {
            // Get the current user's ID
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id;

            // Retrieve engagement data from database
            var qc35forms = _context.QC35Forms.Where(e => e.CreatedBy.Equals(userId)).ToList();
            return View("~/Views/General/QC35/QC35FormManagement.cshtml", qc35forms);
        }

        [Authorize(Roles = "Admin,Reviewer")]
        public async Task<IActionResult> QC35FormApprovalManagement()
        {
            // Get the current user's ID
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id;

            // Check if the user is in the "Admin" role
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            // Retrieve QC35Forms data from the database
            List<QC35Form> selectedForms;

            if (isAdmin)
            {
                // If the user is an Admin, retrieve all forms
                selectedForms = _context.QC35Forms.ToList();
            }
            else
            {
                // Otherwise, retrieve only those created by the current user
                selectedForms = _context.QC35Forms.Where(e => e.CreatedBy.Equals(userId)).ToList();
            }

            // Get the current user's email (assume _userService provides this)
            var userEmail = await _userService.GetUserEmailAsync(User);

            // Filter for first approver (assuming ManagerName is first approver)
            var firstApproverForms = _context.QC35Forms
                .Where(f => f.FirstApprover == userEmail && (f.IsFirstApproved == false || f.IsFirstApproved == null))
                .ToList();

            // Filter for second approver (assuming PartnerName is second approver)
            var secondApproverForms = _context.QC35Forms
                .Where(f => f.SecondApprover == userEmail && (f.IsSecondApproved == false || f.IsSecondApproved == null))
                .ToList();

            // Populate the view model
            var viewModel = new QC35FormAdminManagementViewModel
            {
                AllQC35Forms = selectedForms,
                FirstApproverForms = firstApproverForms,
                SecondApproverForms = secondApproverForms
            };

            return View("~/Views/General/QC35/QC35FormApprovalManagement.cshtml", viewModel);
        }


        [Authorize(Roles = "Admin,Reviewer")]
        [HttpPost]
        [Route("/QC35Form/ApproveQC35Form/{id}")]
        public async Task<IActionResult> ApproveQC35Form(int id)
        {
            // Retrieve engagement data from the database
            var engagement = _context.QC35Forms.FirstOrDefault(e => e.QC35FormID == id);

            if (engagement == null)
            {
                return NotFound();
            }

            // Get the current user's email
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            var currentUserEmail = user?.Email;

            if(currentUserEmail == engagement.FirstApprover)
            {
                engagement.IsFirstApproved = true;
                engagement.Status = "Pending 2nd Approval";

                _context.SaveChanges();

                // Send email to creator to notify on approval
                await _emailSender.SendEmailAsync(engagement.PreparedBy, "QC35 Form Approval Notification",
                    $"<p>Dear {engagement.PreparedBy},</p>" +
                    $"<p>Your new QC35 Form <strong>{engagement.FileReference}</strong> has been approved by: <strong>{engagement.FirstApprover}</strong>.</p>" +
                    $"<p>The QC35 Form is now awaiting the second approval.</p>" +
                    $"<p>If you need further information, please log in to the Audit Management System.</p>" +
                    $"<p>Thank you!</p>" +
                    $"<p>Best regards,<br/>" +
                    $"PKF Team</p>"
                );

                // Send email to 2nd approver on action to take
                await _emailSender.SendEmailAsync(engagement.SecondApprover, "QC35 Form Action Required",
                    $"<p>Dear {engagement.SecondApprover},</p>" +
                    $"<p>A new QC35 Form <strong>{engagement.FileReference}</strong> has been approved by: <strong>{engagement.FirstApprover}</strong>.</p>" +
                    $"<p>You have been designated as the second approver. Please log in to the System to approve or reject the QC35 Form.</p>" +
                    $"<p>Thank you for your attention!</p>" +
                    $"<p>Best regards,<br/>" +
                    $"PKF Team</p>"
                );

                return Ok(new { success = true, message = "The QC35 Form has been approved." });

            }else if (currentUserEmail == engagement.SecondApprover)
            {
                
                if (engagement.IsFirstApproved == false)
                {
                    return Forbid();
                }
                
                engagement.IsSecondApproved = true;

                if (engagement.IsFirstApproved == true && engagement.IsSecondApproved == true)
                {
                    engagement.Status = "Approved";
                }

                _context.SaveChanges();

                // List of approvers
                var recipients = new List<string>
                {
                    engagement.PreparedBy,
                    engagement.FirstApprover,
                    engagement.SecondApprover
                };

                // Send the email to all approvers on the creation of the QC6 form
                foreach (var recipient in recipients)
                {
                    // Subject and body of the email
                    var subject = "QC35 Form Approval";
                    var body =
                        $"<p>Dear {recipient},</p>" +
                        $"<p>The QC35 Form <strong>{engagement.FileReference}</strong> has been successfully approved by the second approver: <strong>{engagement.SecondApprover}</strong>, and is currently active.</p>" +
                        $"<p>If you need further information, please log in to the Audit Management System.</p>" +
                        $"<p>Thank you for your attention!</p>" +
                        $"<p>Best regards,<br/>" +
                        $"PKF Team</p>";

                    await _emailSender.SendEmailAsync(recipient, subject, body);
                }

                return Ok(new { success = true, message = "The QC35 Form has been approved." });

            }

            return RedirectToAction("QC35FormApprovalManagement", "QC35Form");
        }

        [Authorize(Roles = "Admin,Reviewer")]
        [HttpPost]
        [Route("/QC35Form/RejectQC35Form/{id}")]
        public async Task<IActionResult> RejectQC35Form(int id)
        {
                // Get the request body as a string
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                string requestBody = await reader.ReadToEndAsync();

                // Parse the request body JSON to extract the QC35FormID and RejectionReason
                JObject jsonBody = JObject.Parse(requestBody);
                int qc35FormId = (int)jsonBody["QC35FormID"];
                string rejectionReason = (string)jsonBody["RejectionReason"];

                // Retrieve engagement data from the database
                var engagement = _context.QC35Forms.FirstOrDefault(e => e.QC35FormID == qc35FormId);
                if (engagement == null)
                {
                    return NotFound();
                }

                // Get the current user's email
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound("User not found.");
                }
                var currentUserEmail = user?.Email;

                // Check if current user is the first approver
                if (currentUserEmail == engagement.FirstApprover)
                {
                    // Update the engagement status to "Rejected"
                    engagement.Status = "Rejected";
                    engagement.RejectionReason = rejectionReason; // Set the rejection reason

                    // Reset approval status to repeat the approval process
                    engagement.IsFirstApproved = false;
                    engagement.IsSecondApproved = false;

                    _context.SaveChanges();

                    // Send email to creator to notify about the rejection
                    await _emailSender.SendEmailAsync(engagement.PreparedBy, "QC35 Form Rejection Notification",
                        $"<p>Dear {engagement.PreparedBy},</p>" +
                        $"<p>Your QC35 Form has been rejected by: <strong>{engagement.FirstApprover}</strong>.</p>" +
                        $"<p>Please make the necessary amendments and resubmit the form.</p>" +
                        $"<p>If you need further clarification, feel free to contact our support team.</p>" +
                        $"<p>Thank you for your attention!</p>" +
                        $"<p>Best regards,<br/>" +
                        $"PKF Team</p>"
                    );

                    return RedirectToAction("QC35FormApprovalManagement", "QC35Form");
                }

                // Check if current user is the second approver
                if (currentUserEmail == engagement.SecondApprover)
                {
                    if (engagement.IsFirstApproved == false)
                    {
                        return Forbid();
                    }
                    else
                    {
                        // Update the engagement status to "Rejected"
                        engagement.Status = "Rejected";
                        engagement.RejectionReason = rejectionReason; // Set the rejection reason

                        // Reset approval status to repeat the approval process
                        engagement.IsFirstApproved = false;
                        engagement.IsSecondApproved = false;

                        _context.SaveChanges();

                        // Send email to creator to notify about the rejection
                        await _emailSender.SendEmailAsync(engagement.PreparedBy, "QC35 Form Rejection Notification",
                            $"<p>Dear {engagement.PreparedBy},</p>" +
                            $"<p>Your QC35 Form has been rejected by: <strong>{engagement.SecondApprover}</strong>.</p>" +
                            $"<p>Please make the necessary amendments and resubmit the form.</p>" +
                            $"<p>If you need further clarification, feel free to contact our support team.</p>" +
                            $"<p>Thank you for your attention!</p>" +
                            $"<p>Best regards,<br/>" +
                            $"PKF Team</p>"
                        );

                        return RedirectToAction("QC35FormApprovalManagement", "QC35Form");
                    }
                }

                // If neither the first nor the second approver, forbid the operation
                return Forbid();
            }
        }


        [Authorize(Roles = "User,Admin,Non-Auditor,Reviewer")]
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

            // Retrieve all emails for users in the "Admin" and "Reviewer" roles
            var adminEmails = await _userService.GetUserEmailsInRoleAsync("Admin");
            var reviewerEmails = await _userService.GetUserEmailsInRoleAsync("Reviewer");

            // Combine the emails and remove any duplicates
            var combinedEmails = adminEmails.Concat(reviewerEmails).Distinct().OrderBy(email => email).ToList();

            // Retrieve client names for display
            var clientNames = await _context.QC6Forms
                                             .Where(c => c.IsTemplate == false) // Filter based on IsTemplate
                                             .Select(c => c.ProspectiveClient) // Select the column with client names
                                             .Distinct() // Ensure unique client names
                                             .OrderBy(name => name) // Order client names from A to Z
                                             .ToListAsync(); // Fetch the ordered list of unique client names

            // Avoid duplicates in the checklist items based on the description
            var checklistItems = qc35Form.ChecklistItems
                .GroupBy(ci => ci.Description) // Group by description to eliminate duplicates
                .Select(g => g.First()) // Select only the first occurrence of each unique description
                .Select(ci => new QC35ChecklistItemViewModel
                {
                    QC35ChecklistItemID = ci.QC35ChecklistItemID,
                    Description = ci.Description,
                    Response = ci.Response
                })
                .ToList();

            var viewModel = new QC35FormViewModel
            {
                QC35FormID = qc35Form.QC35FormID,
                CreatedBy = qc35Form.CreatedBy,
                ClientName = qc35Form.ClientName,
                ClientNames = clientNames,
                ReportingYearEnd = (DateTime)qc35Form.ReportingYearEnd,
                PartnerName = qc35Form.PartnerName,
                ManagerName = qc35Form.ManagerName,
                Status = qc35Form.Status,
                ImageFileName = qc35Form.ImageFileName,
                AdminEmails = combinedEmails,
                ChecklistItems = checklistItems
                /*
                ChecklistItems = qc35Form.ChecklistItems.Select(ci => new QC35ChecklistItemViewModel
                {
                    QC35ChecklistItemID = ci.QC35ChecklistItemID,
                    Description = ci.Description,
                    Response = ci.Response
                }).ToList()
                */

            };

            ViewBag.Roles = roles;

            return View("~/Views/General/QC35/ViewQC35Form.cshtml", viewModel);
        }

        [Authorize(Roles = "User,Admin,Non-Auditor,Reviewer")]
        public async Task<IActionResult> EditQC35Form(int id)
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

            // Retrieve all emails for users in the "Admin" and "Reviewer" roles
            var adminEmails = await _userService.GetUserEmailsInRoleAsync("Admin");
            var reviewerEmails = await _userService.GetUserEmailsInRoleAsync("Reviewer");

            // Combine the emails and remove any duplicates
            var combinedEmails = adminEmails.Concat(reviewerEmails).Distinct().OrderBy(email => email).ToList();

            // Retrieve client names for display
            var clientNames = await _context.QC6Forms
                                             .Where(c => c.IsTemplate == false) // Filter based on IsTemplate
                                             .Select(c => c.ProspectiveClient) // Select the column with client names
                                             .Distinct() // Ensure unique client names
                                             .OrderBy(name => name) // Order client names from A to Z
                                             .ToListAsync(); // Fetch the ordered list of unique client names

            // Avoid duplicates in the checklist items based on the description
            var checklistItems = qc35Form.ChecklistItems
                .GroupBy(ci => ci.Description) // Group by description to eliminate duplicates
                .Select(g => g.First()) // Select only the first occurrence of each unique description
                .Select(ci => new QC35ChecklistItemViewModel
                {
                    QC35ChecklistItemID = ci.QC35ChecklistItemID,
                    Description = ci.Description,
                    Response = ci.Response
                })
                .ToList();

            var viewModel = new QC35FormViewModel
            {
                QC35FormID = qc35Form.QC35FormID,
                CreatedBy = qc35Form.CreatedBy,
                ClientName = qc35Form.ClientName,
                ClientNames = clientNames,
                ReportingYearEnd = (DateTime)qc35Form.ReportingYearEnd,
                PartnerName = qc35Form.PartnerName,
                ManagerName = qc35Form.ManagerName,
                Status = qc35Form.Status,
                ImageFileName = qc35Form.ImageFileName,
                AdminEmails = combinedEmails,
                ChecklistItems = checklistItems
            };

            ViewBag.Roles = roles;

            return View("~/Views/General/QC35/EditQC35Form.cshtml", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetImage(string key)
        {
            var s3Client = new AmazonS3Client(_accessKey, _secretKey, Amazon.RegionEndpoint.APSoutheast1);

            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                using (var response = await s3Client.GetObjectAsync(request))
                using (var responseStream = response.ResponseStream)
                using (var memoryStream = new MemoryStream())
                {
                    await responseStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    return File(memoryStream.ToArray(), response.Headers.ContentType);
                }
            }
            catch (AmazonS3Exception ex)
            {
                // Handle exception (e.g., log it, return a default image, etc.)
                return NotFound();
            }
        }

        [Authorize(Roles = "User,Admin,Non-Auditor,Reviewer")]
        public async Task<IActionResult> QC35FormCreationAsync()
        {

            // Retrieve user email
            var userEmail = await _userService.GetUserEmailAsync(User);

            // Retrieve client names for display
            
            var clientNames = await _context.QC6Forms
                                             .Where(c => c.IsTemplate == false) // Filter based on IsTemplate
                                             .Select(c => c.ProspectiveClient) // Select the column with client names
                                             .Distinct() // Ensure unique client names
                                             .OrderBy(name => name) // Order client names from A to Z
                                             .ToListAsync(); // Fetch the ordered list of unique client names


            // Retrieve all emails for users in the "Admin" and "Reviewer" roles
            var adminEmails = await _userService.GetUserEmailsInRoleAsync("Admin");
            var reviewerEmails = await _userService.GetUserEmailsInRoleAsync("Reviewer");

            // Combine the emails and remove any duplicates
            var combinedEmails = adminEmails.Concat(reviewerEmails).Distinct().OrderBy(email => email).ToList();

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
                    new QC35ChecklistItemViewModel { Description = "All working papers in each file is reviewed and completed (Manager to initial or sign all working papers)" },
                    new QC35ChecklistItemViewModel { Description = "Date of Audit Report" },
                    new QC35ChecklistItemViewModel { Description = "Date of approval and confirmation that CaseWare Audit files has been locked down within 60 days from the date of the Audit Report (if applicable). Refer to the screenshot of CaseWare below." }
                },
                ClientNames = clientNames
            };

            // Append combined emails to viewModel
            viewModel.AdminEmails = combinedEmails;

            return View("~/Views/General/QC35/QC35FormCreation.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQC35Form(QC35FormViewModel viewModel, IFormFile file)
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

            /*
            if (viewModel.ManagerName == viewModel.PartnerName)
            {
                ViewBag.ErrorMessage = "The Manager and Partner cannot be the same person. Please select different individuals for each role.";
                return View("~/Views/General/QC35/QC35FormCreation.cshtml", viewModel);
            }
            */

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Get the current user's ID
                    var userId = user?.Id;

                    // Retrieve email based on userId from the AspNetUsers table
                    var userEmail = await _context.Users
                        .Where(u => u.Id == userId)
                        .Select(u => u.Email)
                        .FirstOrDefaultAsync();

                    // QCForm File Reference will contain _NAS for Non-Auditor role creation
                    string fileReference = Helper.GenerateQCFormFileReference();

                    if (roles.Contains("Non-Auditor"))
                    {
                        // Modify the fileReference if the "Non-Auditor" role is present
                        fileReference += "_NAS";
                    }

                    var qc35Form = new QC35Form
                    {
                        CreatedBy = userId,
                        FileReference = fileReference,
                        ClientName = viewModel.ClientName,
                        ReportingYearEnd = viewModel.ReportingYearEnd,
                        PartnerName = viewModel.PartnerName,
                        ManagerName = viewModel.ManagerName,
                        ImageFileName = viewModel.ImageFileName,
                        Status = "Pending",
                        FirstApprover = viewModel.ManagerName,
                        SecondApprover = viewModel.PartnerName,
                        IsFirstApproved = false,
                        IsSecondApproved = false,
                        PreparedBy = userEmail
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
                        };
                        _context.QC35ChecklistItems.Add(checklistItem);
                    }


                    if (viewModel.File != null && viewModel.File.Length > 0)
                    {
                        var fileName = await UploadFileAsync(viewModel.File, _bucketName, "qc35forms");
                        qc35Form.ImageFileName = fileName;
                        await _context.SaveChangesAsync();
                    }
                    
                    await transaction.CommitAsync();

                    // Send email to manager (first approver)
                    await _emailSender.SendEmailAsync(viewModel.ManagerName, "QC35 Form Approval Required",
                        $"<p>Dear {viewModel.ManagerName},</p>" +
                        $"<p>A new QC35 Form has been created with File Reference: <strong>{fileReference}</strong>.</p>" +
                        $"<p>You have been designated as the first approver. Please log in to the Audit Management System to approve or reject the QC35 Form.</p>" +
                        $"<p>Thank you for your attention!</p>" +
                        $"<p>Best regards,<br/>" +
                        $"PKF Team</p>"
                    );

                    if (roles.Contains("Admin") || roles.Contains("Reviewer"))
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
        public async Task<IActionResult> UpdateQC35Form(QC35FormViewModel viewModel, IFormFile file)
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            if (viewModel.File == null)
            {
                // Ensure that the File property is treated as valid
                ModelState.Remove("File");
            }

            if (!ModelState.IsValid)
            {
                // Access validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors);

                // Retrieve all emails for users in the "Admin" and "Reviewer" roles
                var adminEmails = await _userService.GetUserEmailsInRoleAsync("Admin");
                var reviewerEmails = await _userService.GetUserEmailsInRoleAsync("Reviewer");

                // Combine the emails and remove any duplicates
                var combinedEmails = adminEmails.Concat(reviewerEmails).Distinct().OrderBy(email => email).ToList();

                // Append combined emails to viewModel
                viewModel.AdminEmails = combinedEmails;

                // Pass the errors to the view
                ViewBag.Errors = errors;

                return View("~/Views/General/QC35/EditQC35Form.cshtml", viewModel);
            }

            if (viewModel.ManagerName == viewModel.PartnerName)
            {
                ViewBag.ErrorMessage = "The Manager and Partner cannot be the same person. Please select different individuals for each role.";
                return View("~/Views/General/QC35/EditQC35Form.cshtml", viewModel);
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

                    // Store the old values for comparison
                    var oldPartnerName = qc35Form.PartnerName;
                    var oldManagerName = qc35Form.ManagerName;

                    // Check if the request is a PUT operation
                    var isPutRequest = Request.Method.Equals(HttpMethod.Put.ToString(), StringComparison.OrdinalIgnoreCase)
                   || Request.Form.ContainsKey("_method") && string.Equals(Request.Form["_method"], "PUT", StringComparison.OrdinalIgnoreCase);

                    // Flag to track if Partner or Manager has been updated
                    var isPartnerOrManagerNameUpdated = false;

                    // Update the QC35Form fields based on the request method
                    if (isPutRequest)
                    {

                        // Update the QC35Form fields
                        qc35Form.ClientName = viewModel.ClientName;
                        qc35Form.ReportingYearEnd = viewModel.ReportingYearEnd;

                        if (qc35Form.PartnerName != viewModel.PartnerName)
                        {
                            qc35Form.PartnerName = viewModel.PartnerName;
                            isPartnerOrManagerNameUpdated = true;
                        }

                        if (qc35Form.ManagerName != viewModel.ManagerName)
                        {
                            qc35Form.ManagerName = viewModel.ManagerName;
                            isPartnerOrManagerNameUpdated = true;
                        }
                        

                        // Update the ChecklistItems
                        foreach (var item in viewModel.ChecklistItems)
                        {
                            var checklistItem = qc35Form.ChecklistItems
                                .FirstOrDefault(ci => ci.QC35ChecklistItemID == item.QC35ChecklistItemID);

                            if (checklistItem != null)
                            {
                                checklistItem.Response = item.Response;
                            }
                            else
                            {
                                // Add new ChecklistItem if it doesn't exist
                                qc35Form.ChecklistItems.Add(new QC35ChecklistItem
                                {
                                    Description = item.Description,
                                    Response = item.Response
                                });
                            }
                        }
                    }

                    // Handle file upload and deletion of old image
                    if (file != null && file.Length > 0)
                    {
                        // Delete the old image from S3
                        if (!string.IsNullOrEmpty(qc35Form.ImageFileName))
                        {
                            await DeleteFileAsync(qc35Form.ImageFileName);
                        }

                        // Upload the new image
                        var fileName = await UploadFileAsync(file, _bucketName, "qc35forms");
                        qc35Form.ImageFileName = fileName;
                    }

                    _context.QC35Forms.Update(qc35Form);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    if (isPartnerOrManagerNameUpdated)
                    {
                        if(qc35Form.PartnerName != oldPartnerName)
                        {
                            // Send email to partner (first approver) about form update
                            await _emailSender.SendEmailAsync(qc35Form.PartnerName, "QC35 Form Update Notification",
                                $"<p>Dear {qc35Form.PartnerName},</p>" +
                                $"<p>The QC35 Form with File Reference: <strong>{qc35Form.FileReference}</strong> has been updated, and you have been designated as the first approver.</p>" +
                                $"<p>Please review the changes and log in to the Audit Management System to take the necessary action.</p>" +
                                $"<p>Thank you for your attention!</p>" +
                                $"<p>Best regards,<br/>" +
                                $"PKF Team</p>"
                            );
                        }

                        if (qc35Form.ManagerName != oldManagerName)
                        {
                            // Send email to manager (second approver) about form update
                            await _emailSender.SendEmailAsync(qc35Form.ManagerName, "QC35 Form Update Notification",
                                $"<p>Dear {qc35Form.ManagerName},</p>" +
                                $"<p>The QC35 Form with File Reference: <strong>{qc35Form.FileReference}</strong> has been updated, and you have been designated as the second approver.</p>" +
                                $"<p>Please review the changes and log in to the Audit Management System to take the necessary action.</p>" +
                                $"<p>Thank you for your attention!</p>" +
                                $"<p>Best regards,<br/>" +
                                $"PKF Team</p>"
                            );
                        }
                    }

                    if (roles.Contains("Admin") || roles.Contains("Reviewer"))
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
                    return View("~/Views/General/QC35/EditQC35Form.cshtml", viewModel);
                }
            }
        }

        [HttpDelete]
        [Authorize(Roles = "User,Admin,Non-Auditor,Reviewer")]
        public async Task<IActionResult> DeleteQC35FormAsync(int id)
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

                //Delete Image from S3 bucket
                var formImage = qc35Form.ImageFileName;
                if (!string.IsNullOrEmpty(formImage))
                {
                    var s3DeletionSuccess = await DeleteFileAsync(formImage);
                    if (!s3DeletionSuccess)
                    {
                        // Optionally return an error message if S3 deletion fails
                        return StatusCode(500, "Error deleting the image from S3");
                    }
                }

                // Delete the QC35Form itself
                _context.QC35Forms.Remove(qc35Form);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync(); // Commit the transaction if all operations are successful

                return NoContent(); // Respond with 204 No Content
            }
            catch (Exception)
            {
                transaction.Rollback(); // Roll back the transaction if an error occurs
                throw;
            }
        }

        [HttpPost]
        public async Task<string> UploadFileAsync(IFormFile file, string bucketName, string? prefix)
        {
            var s3Client = new AmazonS3Client(_accessKey, _secretKey, Amazon.RegionEndpoint.APSoutheast1);

            // Generate a unique filename using GUID
            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var fileName = $"{prefix?.TrimEnd('/')}/{uniqueFileName}";

            //var fileName = $"{prefix?.TrimEnd('/')}/{file.FileName}";
            var request = new PutObjectRequest()
            {
                BucketName = bucketName,
                Key = fileName,
                InputStream = file.OpenReadStream()
            };
            request.Metadata.Add("Content-Type", file.ContentType);

            try
            {
            await s3Client.PutObjectAsync(request);
            }
            catch (AmazonS3Exception s3Ex)
            {
                // Handle S3-specific exceptions
                Console.WriteLine($"S3 error: {s3Ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                // Handle general exceptions
                Console.WriteLine($"General error: {ex.Message}");
                throw;
            }

            //return Ok($"File {prefix}/{file.FileName} uploaded to S3 successfully!");
            return fileName;
        }

        private async Task<bool> DeleteFileAsync(string fileName)
        {
            var s3Client = new AmazonS3Client(_accessKey, _secretKey, Amazon.RegionEndpoint.APSoutheast1);

            try
            {
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileName
                };

                await s3Client.DeleteObjectAsync(deleteObjectRequest);
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                return false;
            }
        }


    }
}
