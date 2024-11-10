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
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace PKFAuditManagement.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public ReportController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        [HttpGet("SelectFields")]
        public IActionResult SelectFields()
        {
            var formsFields = new Dictionary<string, Dictionary<string, string>>
            {
                ["QC6Forms"] = new Dictionary<string, string>
                {
                    {"FileReference", "File Reference"},
                    {"ProspectiveClient", "Prospective Client"},
                    {"PeriodEnded", "Period Ended"},
                    {"EngagementType", "Engagement Type"},
                    {"Industry", "Industry"},
                    {"Status", "Status"},
                    {"RejectionReason", "Rejection Reason"},
                    {"FormSubmissionDate", "Form Submission Date"},
                    {"PKFEntityProposingService", "PKF Entity Proposing Service"},
                    {"SourceOfReferral", "Source Of Referral"},
                    {"NatureOfServiceForEstimateFee", "Nature Of Service For Estimate Fee"},
                    {"EstimatedFee", "Estimated Fee"},
                    {"BudgetedTimeCost", "Budgeted Time Cost"},
                    {"BudgetedFeeRecoveryRate", "Budgeted Fee Recovery Rate"},
                    {"BudgetedFeeRecoveryRateComment", "Budgeted Fee Recovery Rate Comment"},
                    {"OutstandingUnpaidFees", "Outstanding Unpaid Fees"},
                    {"OutstandingUnpaidFeesComment", "Outstanding Unpaid Fees Comment"},
                    {"AuditFee", "Audit Fee"},
                    {"GrandTotal", "Grand Total"},
                    {"FeeConcentration", "Fee Concentration"},
                    {"ConflictsCheckDone", "Conflicts Check Done"},
                    {"TypeOfActivities", "Type Of Activities"},
                    {"ComplexityOfEngagement", "Complexity Of Engagement"},
                    {"PredecessorAuditor", "Predecessor Auditor"},
                    {"ReasonsForDiscontinuance", "Reasons For Discontinuance"},
                    {"PublicInterestEntity", "Public Interest Entity"},
                    {"PublicInterestEntityType", "Public Interest Entity Type"},
                    {"SectionCEvaluation", "TNA/TNE status"} // Added TNATNEAssessments
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
                    {"EPHODApprovedBy", "Approved By Engagment Partner/Head of Division"},
                    {"EPHODApprovedByDate", "Engagment Partner/Head of Division Approved By Date"},
                    {"MPHODQMPApprovedBy", "Approved By Managing Partner/Head of Division/Quality Management Partner"},
                    {"MPHODQMPApprovedByDate", "Managing Partner/Head of Division/Quality Management Partner Approved By Date"},
                },
                ["QC7Forms"] = new Dictionary<string, string>
                {
                    {"FileReference", "File Reference"},
                    {"Client", "Client"},
                    {"PeriodEnded", "Period Ended"},
                    {"EngagementType", "Engagement Type"},
                    {"Industry", "Industry"},
                    {"Status", "Status"},
                    {"RejectionReason", "Rejection Reason"},
                    {"FormSubmissionDate", "Form Submission Date"},
                    {"PriorYearFee", "Prior Year Fee"},
                    {"TimeCosts", "Time Costs"},
                    {"PriorYearRecoveryRate", "Prior Year Recovery Rate"},
                    {"PriorYearRecoveryRateComment", "Prior Year Recovery Rate Comment"},
                    {"AnyOutstandingUnpaidAuditFees", "Any Outstanding Unpaid Audit Fees"},
                    {"AnyOutstandingUnpaidAuditFeesComment", "Any Outstanding Unpaid Audit Fees Comment"},
                    {"TypeOfClientActivities", "Type Of Client Activities"},
                    {"RiskRatingPriorYear", "Risk Rating Prior Year"},
                    {"AnySuspiciousTransactionReportFiled", "Any Suspicious Transaction Report Filed"},
                    {"SuspiciousTransactionReportFiledComment", "Suspicious Transaction Report Filed Comment"},
                    {"SafeguardReviewerName", "Safeguard Reviewer Name"},
                    {"AnyOutstandingUnpaidNonAuditFees", "Any Outstanding Unpaid Non Audit Fees"},
                    {"AnyOutstandingUnpaidNonAuditFeesComment", "Any Outstanding Unpaid Non Audit Fees Comment"},
                    {"FeeConcentration", "Fee Concentration"},
                    {"ProposedFeeCurrentYear", "Proposed Fee Current Year"},
                    {"BudgetedTimeCost", "Budgeted Time Cost"},
                    {"ProposedRecoveryRateCurrentYear", "Proposed Recovery Rate Current Year"},
                    {"IsPublicInterestEntity", "Is Public Interest Entity"},
                    {"PublicInterestEntityType", "Public Interest Entity Type"},
                    {"SectionCEvaluation", "TNA/TNE status"} // Added TNATNEAssessments
                },
                ["QC7FormConclusions"] = new Dictionary<string, string>
                {
                    {"AnyRiskAssociated", "Any Risk Associated"},
                    {"RiskExplanationCurrentYearPriorYear", "Risk Explanation Current Year Prior Year"},
                    {"IsSafeguardApplied", "Is Safeguard Applied"},
                    {"SafeguardsApplied", "Safe guards Applied"},
                    {"NatureOfSafeguard", "Nature Of Safeguard"},
                    {"ContinuingEngagementRiskRated", "Continuing Engagement Risk Rated"},
                    {"SafeguardReviewPartnerAssigned", "Safeguard Review Partner Assigned"},
                    {"IsSuspiciousTransactionReportFiled", "Is Suspicious Transaction Report Filed"},
                    {"SuspiciousTransactionReportFiledRationale", "Suspicious Transaction Report Filed Rationale"},
                    {"EngagementRetainedRejected", "Engagement Retained Rejected"},
                    {"EMPreparedBy", "Prepared By Engagement Manager"},
                    {"EMPreparedByDate", "Engagement Manager Prepared By Date"},
                    {"EPHODApprovedBy", "Approved By Engagment Partner/Head of Division"},
                    {"EPHODApprovedByDate", "Engagment Partner/Head of Division Approved By Date"},
                    {"MPHODQMPApprovedBy", "Approved By Managing Partner/Head of Division/Quality Management Partner"},
                    {"MPHODQMPApprovedByDate", "Managing Partner/Head of Division/Quality Management Partner Approved By Date"}
                },
                ["QC35Forms"] = new Dictionary<string, string>
                {
                    {"FileReference", "File Reference"},
                    {"ClientName", "Client Name"},
                    {"ReportingYearEnd", "Reporting Year End"},
                    {"PartnerName", "Partner Name"},
                    {"ManagerName", "Manager Name"},
                    {"Status", "Status"},
                },
                ["SignedFSForm"] = new Dictionary<string, string>
                {
                    {"Client", "Client"},
                    {"AuditedReportDate", "Audited Report Date"},
                    {"FinancialYearEnd", "Financial Year End"},
                    {"PartnerEmail", "Partner Email"},
                    {"UserEmail", "User Email"},
                    {"FilePath", "File Path"},
                    {"IsProcessed", "Is Processed"}
                },
                ["ClientStatus"] = new Dictionary<string, string>
                {
                    {"QC6_Approvers_info", "QC6 information & Approvers"},
                    {"QC7_Approvers_info", "QC7 information & Approvers"},
                    {"QC6/QC7/QC35/SignedFS_Status", "Status"}
                },
                ["Quiz"] = new Dictionary<string, string>
                {
                    {"UserAttendance", "User Attendance"},
                    {"QuizAttempt", "Quiz Attempt"},
                    {"QuizResponse", "Quiz Response"},
                }

                
            };

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
        {
            if (request == null || request.SelectedFormIds == null || request.Fields == null)
            {
                return BadRequest("Invalid request.");
            }

            // Get the selected form IDs and fields
            var selectedFormIds = string.Join(",", request.SelectedFormIds);
            var selectedFields = request.Fields.Where(f => f.IsSelected).Select(f => f.FieldName).ToList();
            var selectedSections = request.Fields.Where(f => f.IsSelected).Select(f => f.Section).FirstOrDefault();

            var fieldsToSelect = selectedFields.Select(f => {

                if (f.Contains("SectionCEvaluation"))
                {
                    return $"\"TNATNEAssessments\".\"{f.Substring("QC6Forms_".Length)}\"";
                }
                else if (f.StartsWith("QC6Forms"))
                {
                    return $"\"QC6Forms\".\"{f.Substring("QC6Forms_".Length)}\"";
                }
                else if (f.StartsWith("QC6FormConclusions"))
                {
                    return $"\"QC6FormConclusions\".\"{f.Substring("QC6FormConclusions_".Length)}\"";
                }
                else if (f.StartsWith("QC7Forms"))
                {
                    return $"\"QC7Forms\".\"{f.Substring("QC7Forms_".Length)}\"";
                }
                else if (f.StartsWith("QC7FormConclusions"))
                {
                    return $"\"QC7FormConclusions\".\"{f.Substring("QC7FormConclusions_".Length)}\"";
                }
                else if (f.StartsWith("QC35Forms"))
                {
                    return $"\"QC35Forms\".\"{f.Substring("QC35Forms_".Length)}\"";
                }
                else if (f.Contains("DaysUntilDue"))
                {
                    return $"DATEDIFF(DAY, GETDATE(), CAST(QC35ChecklistItems.Response AS DATE)) AS \"DaysUntilDue\"";
                }
                else if (f.StartsWith("QC35ChecklistItems"))
                {
                    return $"\"QC35ChecklistItems\".\"{f.Substring("QC35ChecklistItems_".Length)}\"";
                }
                else if (f.StartsWith("SignedFSForm"))
                {
                    return $"\"SignedFSForm\".\"{f.Substring("SignedFSForm_".Length)}\"";
                }
                else
                {
                    return null;
                }
            }).Where(f => f != null).ToList();

            var selectClause = string.Join(", ", fieldsToSelect);
            var formattedIds = string.Join(", ", request.SelectedFormIds.Select(id => $"'{id}'"));

            var formQuery = string.Empty; // Query to get form data
            var qc6FormStatusQuery = string.Empty; // Query to get QC6 form status data
            var qc7FormStatusQuery = string.Empty; // Query to get QC7 form status data
            var qc35FormStatusQuery = string.Empty; // Query to get QC35 form status data
            var signedFSFormStatusQuery = string.Empty; // Query to get Signed FS form status data
            var userAttendanceQuery = string.Empty; // Query to get user attendance data
            var quizAttemptQuery = string.Empty; // Query to get quiz attempt data
            var quizResponseQuery = string.Empty; // Query to get quiz response data

            if (selectedSections.Contains("QC6Form"))
            {
                formQuery += $@"
                    SELECT {selectClause}
                    FROM ""QC6Forms""
                    LEFT JOIN ""QC6FormConclusions"" ON ""QC6Forms"".""QC6FormID"" = ""QC6FormConclusions"".""QC6FormID""
                    LEFT JOIN ""TNATNEAssessments"" ON ""QC6Forms"".""QC6FormID"" = ""TNATNEAssessments"".""QC6FormID""
                    WHERE ""QC6Forms"".""QC6FormID"" IN ({selectedFormIds})";
            }

            if (selectedSections.Contains("QC7Form"))
            {
                formQuery += $@"
                    SELECT {selectClause}
                    FROM ""QC7Forms""
                    LEFT JOIN ""QC7FormConclusions"" ON ""QC7Forms"".""QC7FormID"" = ""QC7FormConclusions"".""QC7FormID""
                    LEFT JOIN ""TNATNEAssessments"" ON ""QC7Forms"".""QC7FormID"" = ""TNATNEAssessments"".""QC7FormID""
                    WHERE ""QC7Forms"".""QC7FormID"" IN ({selectedFormIds})";
            }

            if (selectedSections.Contains("QC35Form") || selectedSections.Contains("DaysUntilDue"))
            {
                formQuery += $@"
                    SELECT {selectClause}
                    FROM ""QC35Forms""
                    LEFT JOIN ""QC35ChecklistItems"" ON ""QC35Forms"".""QC35FormID"" = ""QC35ChecklistItems"".""QC35FormID""
                    AND ""QC35ChecklistItems"".""Description"" = 'Date of Audit Report'
                    WHERE ""QC35Forms"".""QC35FormID"" IN ({selectedFormIds})";
            }

            if (selectedSections.Contains("SignedFSForm"))
            {
                formQuery += $@"
                    SELECT {selectClause}
                    FROM ""SignedFSForm""
                    WHERE ""SignedFSForm"".""Id"" IN ({selectedFormIds})";
            }

            if (selectedSections.Contains("ClientStatus"))
            {
                if (selectedFields.Contains("ClientStatus_QC6_Approvers_info"))
                {
                    qc6FormStatusQuery += $@"
                        SELECT
                        ""QC6Forms"".""FileReference"" AS ""File Reference"", 
                        ""QC6Forms"".""ProspectiveClient"" AS ""Client Name"",
                        ""QC6Forms"".""PeriodEnded"" AS ""Financial Year End"",
                        ""QC6FormConclusions"".""PreparedBy"" AS ""QC6 First Approver"",
                        ""QC6FormConclusions"".""MPHODQMPApprovedBy"" AS ""QC6 Second Approver""
                        FROM ""QC6Forms""
                        LEFT JOIN ""QC6FormConclusions"" ON ""QC6Forms"".""QC6FormID"" = ""QC6FormConclusions"".""QC6FormID""
                        WHERE ""QC6Forms"".""ProspectiveClient"" IN ({formattedIds})";
                }

                if (selectedFields.Contains("ClientStatus_QC7_Approvers_info"))
                {
                    qc7FormStatusQuery += $@"
                        SELECT
                        ""QC7Forms"".""FileReference"",
                        ""QC7Forms"".""Client"",
                        ""QC7FormConclusions"".""EMPreparedBy"",
                        ""QC7FormConclusions"".""MPHODQMPApprovedBy""
                        FROM ""QC7Forms""
                        LEFT JOIN ""QC7FormConclusions"" ON ""QC7Forms"".""QC7FormID"" = ""QC7FormConclusions"".""QC7FormID""
                        WHERE ""QC7Forms"".""Client"" IN ({formattedIds})";
                }

                if (selectedFields.Contains("ClientStatus_QC6/QC7/QC35/SignedFS_Status"))
                {
                    qc6FormStatusQuery += $@"
                        SELECT
                        ""QC6Forms"".""FileReference"" AS ""QC6 File Reference"", 
                        ""QC6Forms"".""ProspectiveClient"" AS ""QC6 Client Name"",
                        ""QC6Forms"".""Status"" AS ""QC6 Status""
                        FROM ""QC6Forms""
                        WHERE ""QC6Forms"".""ProspectiveClient"" IN ({formattedIds})";

                    qc7FormStatusQuery += $@"
                        SELECT
                        ""QC7Forms"".""FileReference"" AS ""QC7 File Reference"",
                        ""QC7Forms"".""Client"" AS ""QC7 Client Name"",
                        ""QC7Forms"".""Status"" AS ""QC7 Status""
                        FROM ""QC7Forms""
                        WHERE ""QC7Forms"".""Client"" IN ({formattedIds})";

                    qc35FormStatusQuery += $@"
                        SELECT
                        ""QC35Forms"".""FileReference"" AS ""QC35 File Reference"",
                        ""QC35Forms"".""ClientName"" AS ""QC35 Client Name"",
                        ""QC35Forms"".""Status"" AS ""QC35 Status""
                        FROM ""QC35Forms""
                        WHERE ""QC35Forms"".""ClientName"" IN ({formattedIds})";

                    signedFSFormStatusQuery += $@"
                        SELECT
                        ""SignedFSForm"".""Client"" AS ""Signed FS Client"",
                        ""SignedFSForm"".""IsProcessed"" AS ""Signed FS Status""
                        FROM ""SignedFSForm""
                        WHERE ""SignedFSForm"".""Client"" IN ({formattedIds})";
                }
            }

            if (selectedSections.Contains("Quiz"))
            {
                if (selectedFields.Contains("Quiz_UserAttendance"))
                {
                    userAttendanceQuery += $@"
                        SELECT
                        ""AspNetUsers"".""UserName"",
                        ""Participants"".""ClockedAttendance""
                        FROM ""Quiz""
                        LEFT JOIN ""Participants"" ON ""Quiz"".""QuizID"" = ""Participants"".""QuizID""
                        LEFT JOIN ""AspNetUsers"" ON ""Participants"".""UserID"" = ""AspNetUsers"".""Id""
                        WHERE ""Quiz"".""QuizID"" IN ({formattedIds})";
                }

                if (selectedFields.Contains("Quiz_QuizAttempt"))
                {
                    quizAttemptQuery += $@"
                        WITH QuizAttempts AS (
                            SELECT
                                ""Quiz"".""Title"",
                                ""AspNetUsers"".""UserName"",
                                ""Attempt"".""AttemptID"",
                                ""Attempt"".""Score"",
                                ROW_NUMBER() OVER (PARTITION BY ""Participants"".""UserID"", ""Quiz"".""QuizID"" 
                                ORDER BY ""Attempt"".""AttemptID"" ASC) AS ""AttemptNumber""
                            FROM ""Quiz""
                            LEFT JOIN ""Participants"" ON ""Quiz"".""QuizID"" = ""Participants"".""QuizID""
                            LEFT JOIN ""AspNetUsers"" ON ""Participants"".""UserID"" = ""AspNetUsers"".""Id""
                            LEFT JOIN ""Attempt"" ON ""Quiz"".""QuizID"" = ""Attempt"".""QuizID"" AND ""Participants"".""UserID"" = ""Attempt"".""UserID""
                            WHERE ""Quiz"".""QuizID"" IN ({formattedIds})
                        )
                        SELECT
                        ""Title"",
                        ""UserName"" AS ""User Name"",
                        ""AttemptID"" AS ""Attempt ID"",
                        'Attempt ' || ""AttemptNumber"" AS ""Attempt"",
                        ""Score""
                        FROM QuizAttempts
                        ORDER BY ""UserName""";
                }
                

                if (selectedFields.Contains("Quiz_QuizResponse"))
                {
                    quizResponseQuery += $@"
                        SELECT
                        ""QuizResponse"".""AttemptID"",
                        ""Questions"".""Description"" AS ""Questions"",
                        ""QuizResponse"".""SelectedOption"" AS ""Selected Option"",
                        ""Questions"".""CorrectOptionText"" AS ""Correct Answer"",
                        CASE
                            WHEN ""QuizResponse"".""SelectedOption"" = ""Questions"".""CorrectOptionText"" THEN 'Correct'
                            ELSE 'Incorrect'
                        END AS ""Result""
                        FROM ""Questions""
                        LEFT JOIN ""QuizResponse"" ON ""Questions"".""QuestionID"" = ""QuizResponse"".""QuestionID""
                        WHERE ""Questions"".""QuizID"" IN ({formattedIds})
                        ORDER BY ""QuizResponse"".""AttemptID"" ASC";
                }

            }

            // DataTables to store the results of the queries
            var dataTables = new List<DataTable>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync(); // Open the connection asynchronously

                    if (!string.IsNullOrEmpty(formQuery))
                    {
                        // run forms query
                        var formResult = await connection.QueryAsync<dynamic>(formQuery);
                        dataTables.Add(DataHelper.ConvertToDataTable(formResult));
                    }
                    
                    if (!string.IsNullOrEmpty(qc6FormStatusQuery))
                    {
                        // run qc6 status query
                        var qc6FormStatusResult = await connection.QueryAsync<dynamic>(qc6FormStatusQuery);
                        dataTables.Add(DataHelper.ConvertToDataTable(qc6FormStatusResult));
                    }

                    if (!string.IsNullOrEmpty(qc7FormStatusQuery))
                    {
                        // run qc7 status query
                        var qc7FormStatusResult = await connection.QueryAsync<dynamic>(qc7FormStatusQuery);
                        dataTables.Add(DataHelper.ConvertToDataTable(qc7FormStatusResult));
                    }

                    if (!string.IsNullOrEmpty(qc35FormStatusQuery))
                    {
                        // run qc35 status query
                        var qc35FormStatusResult = await connection.QueryAsync<dynamic>(qc35FormStatusQuery);
                        dataTables.Add(DataHelper.ConvertToDataTable(qc35FormStatusResult));
                    }

                    if (!string.IsNullOrEmpty(signedFSFormStatusQuery))
                    {
                        // run signedFS status query
                        var signedFSFormStatusResult = await connection.QueryAsync<dynamic>(signedFSFormStatusQuery);
                        dataTables.Add(DataHelper.ConvertToDataTable(signedFSFormStatusResult));
                    }
                    
                    if (!string.IsNullOrEmpty(userAttendanceQuery))
                    {
                        // run user attendance query
                        var userAttendanceResult = await connection.QueryAsync<dynamic>(userAttendanceQuery);
                        dataTables.Add(DataHelper.ConvertToDataTable(userAttendanceResult));
                    }
                    if (!string.IsNullOrEmpty(quizAttemptQuery))
                    {
                        // run quiz attempt query
                        var quizAttemptResult = await connection.QueryAsync<dynamic>(quizAttemptQuery);
                        dataTables.Add(DataHelper.ConvertToDataTable(quizAttemptResult));
                    }
                    if (!string.IsNullOrEmpty(quizResponseQuery))
                    {
                        // run quiz query
                        var quizResponseResult = await connection.QueryAsync<dynamic>(quizResponseQuery);
                        dataTables.Add(DataHelper.ConvertToDataTable(quizResponseResult));
                    }
                    
                }
                catch (NpgsqlException ex)
                {
                    // Handle PostgreSQL specific exceptions
                    Console.WriteLine($"PostgreSQL error: {ex.Message}");
                    return BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    // Handle general exceptions
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    return BadRequest(ex.Message);
                }

            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Report");
                int currentRow = 1;
                int currentColumn = 1; // Starting column for the first DataTable

                // Loop through each DataTable in dataTables
                foreach (var table in dataTables)
                {
                    // Add headers for the current DataTable
                    var headers = table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
                    for (int i = 0; i < headers.Count; i++)
                    {
                        worksheet.Cell(currentRow, currentColumn + i).Value = headers[i];
                    }

                    // Add data for the current DataTable, starting just below the headers
                    int dataRowStart = currentRow + 1;
                    foreach (DataRow row in table.Rows)
                    {
                        for (int i = 0; i < row.ItemArray.Length; i++)
                        {
                            var value = row.ItemArray[i];
                            worksheet.Cell(dataRowStart, currentColumn + i).Value = value != null ? value.ToString() : "Null";
                        }
                        dataRowStart++;
                    }

                    // Update the starting column for the next DataTable, leaving a one-column gap
                    currentColumn += headers.Count + 1;
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