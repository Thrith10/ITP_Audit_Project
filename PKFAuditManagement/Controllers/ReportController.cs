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
using Microsoft.Data.SqlClient;
using Dapper;

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

        [HttpGet("SelectFields")]
        public IActionResult SelectFields()
        {
            var formsFields = new Dictionary<string, Dictionary<string, string>>
            {
                ["QC6Forms"] = new Dictionary<string, string>
                {
                    {"CreatedBy", "Created By"},
                    {"FileReference", "File Reference"},
                    {"ProspectiveClient", "Prospective Client"},
                    {"PeriodEnded", "Period Ended"},
                    {"EngagementType", "Engagement Type"},
                    {"PreparedBy", "Prepared By"},
                    {"PreparedByDate", "Prepared By Date"},
                    {"ReviewedBy", "Reviewed By"},
                    {"ReviewedByDate", "Reviewed By Date"},
                    {"Status", "Status"},
                    {"RejectionReason", "Rejection Reason"},
                    {"FormSubmissionDate", "Form Submission Date"},
                    {"PKFEntityProposingService", "PKF Entity Proposing Service"},
                    {"SourceOfReferral", "Source Of Referral"},
                    {"NatureOfServiceForEstimateFee", "Nature Of Service For Estimate Fee"},
                    {"EstimatedFee", "Estimated Fee"},
                    {"BudgetedTimeCost", "Budgeted Time Cost"},
                    {"BudgetedFeeRecoveryRate", "Budgeted Fee Recovery Rate"},
                    {"OutstandingUnpaidFees", "Outstanding Unpaid Fees"},
                    {"AuditFee", "Audit Fee"},
                    {"GrandTotal", "Grand Total"},
                    {"FeeConcentration", "Fee Concentration"},
                    {"ConflictsCheckDone", "Conflicts Check Done"},
                    {"TypeOfActivities", "Type Of Activities"},
                    {"ComplexityOfEngagement", "Complexity Of Engagement"},
                    {"PredecessorAuditor", "Predecessor Auditor"},
                    {"ReasonsForDiscontinuance", "Reasons For Discontinuance"},
                    {"PublicInterestEntity", "Public Interest Entity"},
                    {"PublicInterestEntityType", "Public Interest Entity Type"}
                },
                ["QC6FormConclusions"] = new Dictionary<string, string>
                {
                    {"AnySignificantRisk", "Any Significant Risk"},
                    {"SignificantRiskComment", "Significant Risk Comment"},
                    {"NewEngagementRiskRating", "New Engagement Risk Rating"},
                    {"NewEngagementRiskRatingReason", "New Engagement Risk Rating Reason"},
                    {"EngagementSubjectedTo", "Engagement Subjected To"},
                    {"SafeguardReviewerAssigned", "Safeguard Reviewer Assigned"},
                    {"SuspiciousTransactionReportFiledRationale", "Suspicious Transaction Report Filed Rationale"},
                    {"Satisfaction", "Satisfaction"},
                    {"PreparedBy", "Prepared By"},
                    {"PreparedByDate", "Prepared By Date"},
                    {"ReviewedBy", "Reviewed By"},
                    {"ReviewedByDate", "Reviewed By Date"},
                    {"EPHODApprovedBy", "EPHOD Approved By"},
                    {"EPHODApprovedByDate", "EPHOD Approved By Date"},
                    {"MPHODQMPApprovedBy", "MPHOD QMP Approved By"},
                    {"MPHODQMPApprovedByDate", "MPHOD QMP Approved By Date"},
                },
                ["QC7Forms"] = new Dictionary<string, string>
                {

                    {"CreatedBy", "Created By"},
                    {"FileReference", "File Reference"},
                    {"Client", "Client"},
                    {"PeriodEnded", "Period Ended"},
                    {"EngagementType", "Engagement Type"},
                    {"PreparedBy", "Prepared By"},
                    {"PreparedByDate", "Prepared By Date"},
                    {"ReviewedBy", "Reviewed By"},
                    {"ReviewedByDate", "Reviewed By Date"},
                    {"Status", "Status"},
                    {"RejectionReason", "Rejection Reason"},
                    {"FormSubmissionDate", "Form Submission Date"},
                    {"PriorYearFee", "Prior Year Fee"},
                    {"TimeCosts", "Time Costs"},
                    {"PriorYearRecoveryRate", "Prior Year Recovery Rate"},
                    {"AnyOutstandingUnpaidAuditFees", "Any Outstanding Unpaid Audit Fees"},
                    {"TypeOfClientActivities", "Type Of Client Activities"},
                    {"RiskRatingPriorYear", "Risk Rating Prior Year"},
                    {"AnySuspiciousTransactionReportFiled", "Any Suspicious Transaction Report Filed"},
                    {"SuspiciousTransactionReportFiledComment", "Suspicious Transaction Report Filed Comment"},
                    {"SafeguardReviewerName", "Safeguard Reviewer Name"},
                    {"AnyOutstandingUnpaidNonAuditFees", "Any Outstanding Unpaid Non Audit Fees"},
                    {"FeeConcentration", "Fee Concentration"},
                    {"ProposedFeeCurrentYear", "Proposed Fee Current Year"},
                    {"BudgetedTimeCost", "Budgeted Time Cost"},
                    {"ProposedRecoveryRateCurrentYear", "Proposed Recovery Rate Current Year"},
                    {"IsPublicInterestEntity", "Is Public Interest Entity"},
                    {"PublicInterestEntityType", "Public Interest Entity Type"}
                },
                ["QC7FormConclusions"] = new Dictionary<string, string>
                {
                    {"AnyRiskAssociated", "Any Risk Associated"},
                    {"RiskExplanationCurrentYearPriorYear", "Risk Explanation Current Year Prior Year"},
                    {"IsSafeguardApplied", "Is Safeguard Applied"},
                    {"NatureOfSafeguard", "Nature Of Safeguard"},
                    {"ContinuingEngagementRiskRated", "Continuing Engagement Risk Rated"},
                    {"SafeguardReviewPartnerAssigned", "Safeguard Review Partner Assigned"},
                    {"IsSuspiciousTransactionReportFiled", "Is Suspicious Transaction Report Filed"},
                    {"SuspiciousTransactionReportFiledRationale", "Suspicious Transaction Report Filed Rationale"},
                    {"EngagementRetainedRejected", "Engagement Retained Rejected"},
                    {"EMPreparedBy", "EM Prepared By"},
                    {"EMPreparedByDate", "EM Prepared By Date"},
                    {"EPHODApprovedBy", "EPHOD Approved By"},
                    {"EPHODApprovedByDate", "EPHOD Approved By Date"},
                    {"MPHODQMPApprovedBy", "MPHOD QMP Approved By"},
                    {"MPHODQMPApprovedByDate", "MPHOD QMP Approved By Date"}
                },
                ["QC35Forms"] = new Dictionary<string, string>
                {
                    {"CreatedBy", "Created By"},
                    {"ClientName", "Client Name"},
                    {"ReportingYearEnd", "Reporting Year End"},
                    {"PartnerName", "Partner Name"},
                    {"ManagerName", "Manager Name"},
                    {"ImageFileName", "Image File Name"},
                    {"Status", "Status"},
                }

                

                
            };

            // var fields = combinedQC6Fields.Select(f => new FieldCheckbox
            // {
            //     FieldName = f.Key,
            //     FieldLabel = f.Value,
            //     IsSelected = false
            // }).ToList();

            var fields = formsFields
                .SelectMany(section => section.Value.Select(f => new FieldCheckbox
                {
                    FieldName = f.Key,
                    FieldLabel = f.Value,
                    Section = section.Key,
                    IsSelected = false
                }))
                .ToList();

            return Ok(fields);
        }

        [HttpPost("GenerateReport")]
        public async Task<IActionResult> GenerateReport([FromBody] SelectFieldsViewModel request)

        //public IActionResult GenerateReport([FromBody] SelectFieldsViewModel request)
        {
            if (request == null || request.SelectedFormIds == null || request.Fields == null)
            {
                return BadRequest("Invalid request.");
            }

            Console.WriteLine("Data received successfully");
            Console.WriteLine("Data: " + request);

            // Get the selected form IDs and fields
            var selectedFormIds = string.Join(",", request.SelectedFormIds);
            var selectedFields = request.Fields.Where(f => f.IsSelected).Select(f => f.FieldName).ToList();
            var selectedSections = request.Fields.Where(f => f.IsSelected).Select(f => f.Section).FirstOrDefault();

            var fieldsToSelect = selectedFields.Select(f => {
                if (f.StartsWith("QC6Forms")) {
                    return $"QC6Forms.{f.Substring("QC6Forms_".Length)}";
                }
                else if (f.StartsWith("QC6FormConclusions")) {
                    return $"QC6FormConclusions.{f.Substring("QC6FormConclusions_".Length)}";
                }
                else if (f.StartsWith("QC7Forms")) {
                    return $"QC7Forms.{f.Substring("QC7Forms_".Length)}";
                }
                else if (f.StartsWith("QC7FormConclusions")) {
                    return $"QC7FormConclusions.{f.Substring("QC7FormConclusions_".Length)}";
                }
                else if (f.StartsWith("QC35Forms")) {
                    return $"QC35Forms.{f.Substring("QC35Forms_".Length)}";
                }
                else {
                    return null;
                }
            }).Where(f => f != null).ToList();

            var selectClause = string.Join(", ", fieldsToSelect);
            var query = string.Empty;

            if (selectedSections.Contains("QC6Form")) {
                query += $@"
                    SELECT {selectClause}
                    FROM QC6Forms
                    LEFT JOIN QC6FormConclusions ON QC6Forms.QC6FormID = QC6FormConclusions.QC6FormID
                    WHERE QC6Forms.QC6FormID IN ({selectedFormIds})";

            }

            if (selectedSections.Contains("QC7Form")) {
                query += $@"
                    SELECT {selectClause}
                    FROM QC7Forms
                    LEFT JOIN QC7FormConclusions ON QC7Forms.QC7FormID = QC7FormConclusions.QC7FormID
                    WHERE QC7Forms.QC7FormID IN ({selectedFormIds})";
            }

            if (selectedSections.Contains("QC35Form")) {
                query += $@"
                    SELECT {selectClause}
                    FROM QC35Forms
                    WHERE QC35Forms.QC35FormID IN ({selectedFormIds})";
            }

            // Log the received data
            Console.WriteLine("Selected Form IDs: " + string.Join(", ", selectedFormIds));
            Console.WriteLine("Selected Fields: " + string.Join(", ", selectedFields));
            Console.WriteLine("Selected Sections: " + string.Join(", ", selectedSections));

            using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {
                //connection.Open();
                var result = await connection.QueryAsync<dynamic>(query);

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Report");
                    var currentRow = 1;

                    // Add headers
                    var headers = ((IDictionary<string, object>)result.First()).Keys.ToList();
                    for (int i = 0; i < headers.Count; i++)
                    {
                        worksheet.Cell(currentRow, i + 1).Value = headers[i];
                    }

                    // Create a table first
                    var range = worksheet.Range(currentRow, 1, currentRow, headers.Count);
                    //var table = worksheet.Tables.Add(range, "DataTable");
                    
                    // Add data
                    foreach (var row in result)
                    {
                        currentRow++;
                        var dictRow = (IDictionary<string, object>)row;
                        for (int i = 0; i < dictRow.Values.Count; i++)
                        {
                            var value = dictRow.Values.ElementAt(i);
                            worksheet.Cell(currentRow, i + 1).Value = value != null ? value.ToString() : "Null";                        }
                    }

                    // Prepare the response
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report.xlsx");
                    }
                }
            }
        }

    }

}