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
                    {"QuizActivityRecord", "Quiz Activity Record"}
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

            // Separate SelectedFormIds into GUIDs and integers
            var guidIds = request.SelectedFormIds
                .Where(id => Guid.TryParse(id, out _)) // Filter valid GUIDs
                .Select(Guid.Parse) // Parse them into Guid
                .ToArray(); // Convert to array

            var intIds = request.SelectedFormIds
                .Where(id => int.TryParse(id, out _)) // Filter valid integers
                .Select(int.Parse) // Parse them into integers
                .ToArray(); // Convert to array of integers

            // keep stringIds that are neither GUIDs nor integers
            var stringIds = request.SelectedFormIds
                .Where(id => !Guid.TryParse(id, out _) && !int.TryParse(id, out _)) // Filter non-GUIDs and non-integers
                .ToArray(); // Convert to array of strings

            var formQuery = string.Empty; // Query to get form data
            var qc6FormStatusQuery = string.Empty; // Query to get QC6 form status data
            var qc6FormApproversQuery = string.Empty; // Query to get QC6 form approvers data
            var qc7FormStatusQuery = string.Empty; // Query to get QC7 form status data
            var qc7FormApproversQuery = string.Empty; // Query to get QC7 form approvers data
            var qc35FormStatusQuery = string.Empty; // Query to get QC35 form status data
            var signedFSFormStatusQuery = string.Empty; // Query to get Signed FS form status data
            var userAttendanceQuery = string.Empty; // Query to get user attendance data
            var quizAttemptQuery = string.Empty; // Query to get quiz attempt data
            var quizResponseQuery = string.Empty; // Query to get quiz response data
            var quizFeedbackResponseQuery = string.Empty; // Query to get quiz feedback response data
            var quizSelfAssessmentQuery = string.Empty; // Query to get quiz self assessment data

            if (selectedSections.Contains("QC6Form"))
            {
                formQuery += $@"
                    SELECT {selectClause}
                    FROM ""QC6Forms""
                    LEFT JOIN ""QC6FormConclusions"" ON ""QC6Forms"".""QC6FormID"" = ""QC6FormConclusions"".""QC6FormID""
                    LEFT JOIN ""TNATNEAssessments"" ON ""QC6Forms"".""QC6FormID"" = ""TNATNEAssessments"".""QC6FormID""
                    WHERE ""QC6Forms"".""QC6FormID"" = ANY(@intIds)";
            }

            if (selectedSections.Contains("QC7Form"))
            {
                formQuery += $@"
                    SELECT {selectClause}
                    FROM ""QC7Forms""
                    LEFT JOIN ""QC7FormConclusions"" ON ""QC7Forms"".""QC7FormID"" = ""QC7FormConclusions"".""QC7FormID""
                    LEFT JOIN ""TNATNEAssessments"" ON ""QC7Forms"".""QC7FormID"" = ""TNATNEAssessments"".""QC7FormID""
                    WHERE ""QC7Forms"".""QC7FormID"" = ANY(@intIds)";
            }

            if (selectedSections.Contains("QC35Form") || selectedSections.Contains("DaysUntilDue"))
            {
                formQuery += $@"
                    SELECT {selectClause}
                    FROM ""QC35Forms""
                    LEFT JOIN ""QC35ChecklistItems"" ON ""QC35Forms"".""QC35FormID"" = ""QC35ChecklistItems"".""QC35FormID""
                    AND ""QC35ChecklistItems"".""Description"" = 'Date of Audit Report'
                    WHERE ""QC35Forms"".""QC35FormID"" = ANY(@intIds)";
            }

            if (selectedSections.Contains("SignedFSForm"))
            {
                formQuery += $@"
                    SELECT {selectClause}
                    FROM ""SignedFSForm""
                    WHERE ""SignedFSForm"".""Id"" = ANY(@intIds)";
            }

            if (selectedSections.Contains("ClientStatus"))
            {
                if (selectedFields.Contains("ClientStatus_QC6_Approvers_info"))
                {
                    qc6FormApproversQuery += $@"
                        SELECT
                        ""QC6Forms"".""FileReference"" AS ""File Reference"", 
                        ""QC6Forms"".""ProspectiveClient"" AS ""Client Name"",
                        ""QC6Forms"".""PeriodEnded"" AS ""Financial Year End"",
                        ""QC6FormConclusions"".""PreparedBy"" AS ""QC6 First Approver"",
                        ""QC6FormConclusions"".""MPHODQMPApprovedBy"" AS ""QC6 Second Approver""
                        FROM ""QC6Forms""
                        LEFT JOIN ""QC6FormConclusions"" ON ""QC6Forms"".""QC6FormID"" = ""QC6FormConclusions"".""QC6FormID""
                        WHERE ""QC6Forms"".""ProspectiveClient"" = ANY(@stringIds)";
                }

                if (selectedFields.Contains("ClientStatus_QC7_Approvers_info"))
                {
                    qc7FormApproversQuery += $@"
                        SELECT
                        ""QC7Forms"".""FileReference"",
                        ""QC7Forms"".""Client"",
                        ""QC7FormConclusions"".""EMPreparedBy"",
                        ""QC7FormConclusions"".""MPHODQMPApprovedBy""
                        FROM ""QC7Forms""
                        LEFT JOIN ""QC7FormConclusions"" ON ""QC7Forms"".""QC7FormID"" = ""QC7FormConclusions"".""QC7FormID""
                        WHERE ""QC7Forms"".""Client"" = ANY(@stringIds)";
                }

                if (selectedFields.Contains("ClientStatus_QC6/QC7/QC35/SignedFS_Status"))
                {
                    qc6FormStatusQuery += $@"
                        SELECT
                        ""QC6Forms"".""FileReference"" AS ""QC6 File Reference"", 
                        ""QC6Forms"".""ProspectiveClient"" AS ""QC6 Client Name"",
                        ""QC6Forms"".""Status"" AS ""QC6 Status""
                        FROM ""QC6Forms""
                        WHERE ""QC6Forms"".""ProspectiveClient"" = ANY(@stringIds)";

                    qc7FormStatusQuery += $@"
                        SELECT
                        ""QC7Forms"".""FileReference"" AS ""QC7 File Reference"",
                        ""QC7Forms"".""Client"" AS ""QC7 Client Name"",
                        ""QC7Forms"".""Status"" AS ""QC7 Status""
                        FROM ""QC7Forms""
                        WHERE ""QC7Forms"".""Client"" = ANY(@stringIds)";

                    qc35FormStatusQuery += $@"
                        SELECT
                        ""QC35Forms"".""FileReference"" AS ""QC35 File Reference"",
                        ""QC35Forms"".""ClientName"" AS ""QC35 Client Name"",
                        ""QC35Forms"".""Status"" AS ""QC35 Status""
                        FROM ""QC35Forms""
                        WHERE ""QC35Forms"".""ClientName"" = ANY(@stringIds)";

                    signedFSFormStatusQuery += $@"
                        SELECT
                        ""SignedFSForm"".""Client"" AS ""Signed FS Client"",
                        ""SignedFSForm"".""IsProcessed"" AS ""Signed FS Status""
                        FROM ""SignedFSForm""
                        WHERE ""SignedFSForm"".""Client"" = ANY(@stringIds)";
                }
            }

            if (selectedSections.Contains("Quiz"))
            {
                if (selectedFields.Contains("Quiz_QuizActivityRecord"))
                {
                    // Add query to get quiz activity record
                    // Query for user attendance
                    userAttendanceQuery += $@"
                        SELECT
                        ""Quiz"".""Title"",
                        ""AspNetUsers"".""UserName"" AS ""User Name"",
                        ""Participants"".""ClockedAttendance"" AS ""Clocked Attendance""
                        FROM ""Quiz""
                        LEFT JOIN ""Participants"" ON ""Quiz"".""QuizID"" = ""Participants"".""QuizID""
                        LEFT JOIN ""AspNetUsers"" ON ""Participants"".""UserID"" = ""AspNetUsers"".""Id""
                        WHERE ""Quiz"".""QuizID"" = ANY(@guidIds)";

                    // Query for quiz attempt
                    quizAttemptQuery += $@"
                        WITH QuizAttempts AS (
                            SELECT
                                ""Quiz"".""Title"",
                                ""Quiz"".""QuizID"",
                                ""AspNetUsers"".""UserName"",
                                ""Attempt"".""AttemptID"",
                                ""Attempt"".""Score"",
                                ROW_NUMBER() OVER (PARTITION BY ""Participants"".""UserID"", ""Quiz"".""QuizID"" 
                                ORDER BY ""Attempt"".""AttemptID"" ASC) AS ""AttemptNumber""
                            FROM ""Quiz""
                            LEFT JOIN ""Participants"" ON ""Quiz"".""QuizID"" = ""Participants"".""QuizID""
                            LEFT JOIN ""AspNetUsers"" ON ""Participants"".""UserID"" = ""AspNetUsers"".""Id""
                            LEFT JOIN ""Attempt"" ON ""Quiz"".""QuizID"" = ""Attempt"".""QuizID"" AND ""Participants"".""UserID"" = ""Attempt"".""UserID""
                            WHERE ""Quiz"".""QuizID"" = ANY(@guidIds)
                        ),
                        QuizQuestions AS (
                            SELECT 
                                ""QuizID"",
                                COUNT(*) AS ""QuestionNum""
                            FROM ""Questions""
                            GROUP BY ""QuizID""
                        ),
                        FeedbackData AS (
                            SELECT DISTINCT
                                ""Quiz"".""Title"" AS ""Quiz"",
                                ""Quiz"".""QuizID"",
                                ""AspNetUsers"".""UserName"" AS ""User Name""
                            FROM ""FeedbackForms""
                            LEFT JOIN ""FeedbackQuestions"" ON ""FeedbackForms"".""FeedbackFormID"" = ""FeedbackQuestions"".""FeedbackFormID""
                            LEFT JOIN ""FeedbackResponses"" ON ""FeedbackQuestions"".""FeedbackQuestionID"" = ""FeedbackResponses"".""FeedbackQuestionID""
                            LEFT JOIN ""AspNetUsers"" ON ""FeedbackResponses"".""SubmittedBy"" = ""AspNetUsers"".""Id""
                            LEFT JOIN ""Quiz"" ON ""FeedbackResponses"".""QuizID"" = ""Quiz"".""QuizID""
                        ),
                        SelfAssessmentData AS (
                            SELECT DISTINCT
                                ""Quiz"".""Title"" AS ""Quiz"",
                                ""AspNetUsers"".""UserName"" AS ""User Name""
                            FROM ""SelfAssessmentForms""
                            LEFT JOIN ""SelfAssessmentQuestions"" ON ""SelfAssessmentForms"".""SelfAssessmentFormID"" = ""SelfAssessmentQuestions"".""SelfAssessmentFormID""
                            LEFT JOIN ""SelfAssessmentResponses"" ON ""SelfAssessmentQuestions"".""SelfAssessmentQuestionID"" = ""SelfAssessmentResponses"".""SelfAssessmentQuestionID""
                            LEFT JOIN ""AspNetUsers"" ON ""SelfAssessmentResponses"".""SubmittedBy"" = ""AspNetUsers"".""Id""
                            LEFT JOIN ""Quiz"" ON ""SelfAssessmentResponses"".""QuizID"" = ""Quiz"".""QuizID""
                        )
                        SELECT DISTINCT
                            QuizAttempts.""Title"",
                            QuizAttempts.""UserName"" AS ""User Name"",
                            QuizAttempts.""AttemptID"" AS ""Attempt ID"",
                            'Attempt ' || CAST(QuizAttempts.""AttemptNumber"" AS TEXT) AS ""Attempt"",
                            QuizAttempts.""Score"",
                            CASE
                                WHEN QuizAttempts.""Score"" IS NULL THEN 'No Attempt'
                                WHEN QuizAttempts.""Score"" >= (QuizQuestions.""QuestionNum"" * 0.8) THEN 'Pass'
                                ELSE 'Fail'
                            END AS ""Result"",
                            CASE
                                WHEN FeedbackData.""User Name"" IS NOT NULL THEN 'Submitted'
                            ELSE 'Not Submitted'
                            END AS ""Feedback Status"",
                            CASE
                                WHEN SelfAssessmentData.""User Name"" IS NOT NULL THEN 'Submitted'
                            ELSE 'Not Submitted'
                            END AS ""Self-Assessment Status""
                        FROM QuizAttempts
                        LEFT JOIN QuizQuestions ON QuizAttempts.""QuizID"" = QuizQuestions.""QuizID""
                        LEFT JOIN FeedbackData ON QuizAttempts.""UserName"" = FeedbackData.""User Name""
                        LEFT JOIN SelfAssessmentData ON QuizAttempts.""UserName"" = SelfAssessmentData.""User Name""
                        ORDER BY QuizAttempts.""UserName""";

                    // Query for quiz response
                    quizResponseQuery += $@"
                        SELECT
                        ""QuizResponse"".""AttemptID"" AS ""Attempt ID"",
                        ""Quiz"".""Title"",
                        ""Questions"".""Description"" AS ""Questions"",
                        ""QuizResponse"".""SelectedOption"" AS ""Selected Option"",
                        ""Questions"".""CorrectOptionText"" AS ""Correct Answer"",
                        CASE
                            WHEN ""QuizResponse"".""SelectedOption"" = ""Questions"".""CorrectOptionText"" THEN 'Correct'
                            ELSE 'Incorrect'
                        END AS ""Result""
                        FROM ""Questions""
                        LEFT JOIN ""QuizResponse"" ON ""Questions"".""QuestionID"" = ""QuizResponse"".""QuestionID""
                        LEFT JOIN ""Quiz"" ON ""Questions"".""QuizID"" = ""Quiz"".""QuizID""
                        WHERE ""Questions"".""QuizID"" = ANY(@guidIds)
                        ORDER BY ""QuizResponse"".""AttemptID"" ASC";

                    // Query for quiz feedback response
                    quizFeedbackResponseQuery += $@"
                        SELECT
                        ""Quiz"".""Title"" AS ""Quiz"",
                        ""AspNetUsers"".""UserName"" AS ""User Name"",
                        ""FeedbackForms"".""Title"",
                        ""FeedbackQuestions"".""QuestionText"" AS ""Question"",
                        ""FeedbackResponses"".""Response"" 
                        FROM ""FeedbackForms""
                        LEFT JOIN ""FeedbackQuestions"" ON ""FeedbackForms"".""FeedbackFormID"" = ""FeedbackQuestions"".""FeedbackFormID""
                        LEFT JOIN ""FeedbackResponses"" ON ""FeedbackQuestions"".""FeedbackQuestionID"" = ""FeedbackResponses"".""FeedbackQuestionID""
                        LEFT JOIN ""AspNetUsers"" ON ""FeedbackResponses"".""SubmittedBy"" = ""AspNetUsers"".""Id""
                        LEFT JOIN ""Quiz"" ON ""FeedbackResponses"".""QuizID"" = ""Quiz"".""QuizID""
                        WHERE ""Quiz"".""QuizID"" = ANY(@guidIds)
                        ORDER BY ""AspNetUsers"".""UserName"", ""FeedbackForms"".""Title""";

                    // Query for quiz self assessment
                    quizSelfAssessmentQuery += $@"
                        SELECT
                        ""Quiz"".""Title"" AS ""Quiz"",
                        ""AspNetUsers"".""UserName"" AS ""User Name"",
                        ""SelfAssessmentForms"".""Title"" AS ""Self-Assesment Title"",
                        ""SelfAssessmentQuestions"".""QuestionText"" AS ""Question"",
                        ""SelfAssessmentResponses"".""Rating"",
                        CASE
                            WHEN ""SelfAssessmentResponses"".""Stage"" = '1' THEN 'Submitted after quiz attempt'
                            ELSE 'Submitted before quiz attempt'
                        END AS ""Self-Assessment check""
                        FROM ""SelfAssessmentForms""
                        LEFT JOIN ""SelfAssessmentQuestions"" ON ""SelfAssessmentForms"".""SelfAssessmentFormID"" = ""SelfAssessmentQuestions"".""SelfAssessmentFormID""
                        LEFT JOIN ""SelfAssessmentResponses"" ON ""SelfAssessmentQuestions"".""SelfAssessmentQuestionID"" = ""SelfAssessmentResponses"".""SelfAssessmentQuestionID""
                        LEFT JOIN ""AspNetUsers"" ON ""SelfAssessmentResponses"".""SubmittedBy"" = ""AspNetUsers"".""Id""
                        LEFT JOIN ""Quiz"" ON ""SelfAssessmentResponses"".""QuizID"" = ""Quiz"".""QuizID""
                        WHERE ""Quiz"".""QuizID"" = ANY(@guidIds)
                        ORDER BY ""AspNetUsers"".""UserName"", ""SelfAssessmentForms"".""Title""";
                }


            }

            // DataTables to store the results of the queries
            var dataTables = new List<DataTable>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync(); // Open the connection asynchronously

                    // List of queries strings
                    var queries = new List<(string Query, object Parameters)> 
                    {   
                        (formQuery, new { intIds }),
                        (qc6FormStatusQuery, new { stringIds }),
                        (qc6FormApproversQuery, new { stringIds }),
                        (qc7FormStatusQuery, new { stringIds }),
                        (qc7FormApproversQuery, new { stringIds }),
                        (qc35FormStatusQuery, new { stringIds }),
                        (signedFSFormStatusQuery, new { stringIds }),
                        (userAttendanceQuery, new { guidIds }),
                        (quizAttemptQuery, new { guidIds }),
                        (quizResponseQuery, new { guidIds }),
                        (quizFeedbackResponseQuery, new { guidIds }),
                        (quizSelfAssessmentQuery, new { guidIds }) 
                    };
                    
                    // Execute each query and add to DataTables
                    foreach (var (query, parameters) in queries)
                    {
                        await DataHelper.ExecuteQueryAndAddToDataTablesAsync(query, connection, dataTables, parameters);
                    }                    

                }
                catch (SqlException ex)
                {
                    // Handle SQL exceptions
                    Console.WriteLine($"SQL error: {ex.Message}");
                    throw;
                }
                catch (Exception ex)
                {
                    // Handle other exceptions
                    Console.WriteLine($"General error: {ex.Message}");
                    throw;
                }
            }

            using (var workbook = new XLWorkbook())
            {
                if (selectedSections.Contains("Quiz"))
                {
                    var userAttendanceTable = dataTables[0];
                    var uniqueUsers = userAttendanceTable.AsEnumerable()
                        .Select(row => row.Field<string>("User Name"))
                        .Distinct()
                        .ToList();

                    // Count the unique users
                    int userCount = uniqueUsers.Count;

                    // Create a new worksheet of overview user quiz data
                    var overviewWorksheet = workbook.Worksheets.Add("Quiz Overview");
                    int currentRow = 1; // Starting row for the current DataTable
                    int currentColumn = 1; // Starting column for the first DataTable
                    int tableIndex = 0; // Index of the current DataTable

                    foreach (var table in dataTables)
                    {
                        // Create a copy of the table
                        var modifiedTable = table.Copy();

                        // Process only the first two tables
                        if (tableIndex >= 2)
                        {
                            break; // Exit the loop after processing the first two tables
                        }

                        // If it's the second table, remove a column
                        if (tableIndex == 1)
                        {
                            // Check if the column "Attempt ID" exists
                            if (modifiedTable.Columns.Contains("Attempt ID"))
                            {
                                modifiedTable.Columns.Remove("Attempt ID");  // Remove the column if it exists
                            }

                        }

                        DataHelper.AddHeadersToWorksheet(overviewWorksheet, modifiedTable, currentRow, currentColumn);
                        DataHelper.AddDataToWorksheet(overviewWorksheet, modifiedTable, currentRow + 1, currentColumn);

                        // Update the starting column for the next DataTable, leaving a one-column gap
                        currentColumn += modifiedTable.Columns.Count + 1;
                        // Increment the table index
                        tableIndex++;
                    }

                    foreach (var user in uniqueUsers)
                    {
                        // Create a new worksheet for each user
                        var userWorksheet = workbook.Worksheets.Add(user);
                        int userCurrentRow = 1; // Starting row for the current DataTable
                        int userCurrentColumn = 1; // Starting column for the first DataTable
                        
                        // Filter the user's attendance data, add headers and data to the worksheet, and update the current column leaving a one-column gap
                        var userAttendanceData = DataHelper.FilterDataTable(userAttendanceTable, "User Name", user);
                        DataHelper.AddHeadersToWorksheet(userWorksheet, userAttendanceData, userCurrentRow, userCurrentColumn);
                        DataHelper.AddDataToWorksheet(userWorksheet, userAttendanceData, userCurrentRow + 1, userCurrentColumn);
                        userCurrentColumn += userAttendanceData.Columns.Count + 1;

                        // Filter the user's quiz attempt data  
                        var quizAttemptTable = dataTables[1];
                        var quizAttemptData = DataHelper.FilterDataTable(quizAttemptTable, "User Name", user);
                        // Extract the Attempt IDs for the filtered user
                        var attemptIDs = quizAttemptData.AsEnumerable()
                            .Select(row => row.Field<string>("Attempt ID"))
                            .DefaultIfEmpty(null)
                            .ToList();

                        // Add headers and data for the current DataTable in the worksheet and update the current column leaving a one-column gap
                        DataHelper.AddHeadersToWorksheet(userWorksheet, quizAttemptData, userCurrentRow, userCurrentColumn);
                        DataHelper.AddDataToWorksheet(userWorksheet, quizAttemptData, userCurrentRow + 1, userCurrentColumn);
                        userCurrentColumn += quizAttemptData.Columns.Count + 1;

                        // Filter the user's quiz response data by the user's attempt IDs
                        var quizResponseTable = dataTables[2];
                        var filteredQuizResponseData = quizResponseTable.AsEnumerable()
                            .Where(row => attemptIDs.Contains(row.Field<string>("Attempt ID")))
                            .ToList();
                        // Initialize quizResponseData as a clone of quizResponseTable's schema
                        DataTable quizResponseData = quizResponseTable.Clone();

                        if (!filteredQuizResponseData.Any())
                        {
                            quizResponseData = DataHelper.HandleEmptyDataTable(quizResponseData, "Attempt ID", "No Attempts done");
                        }
                        else
                        {
                            quizResponseData = filteredQuizResponseData.CopyToDataTable();
                        }

                        // Add headers and data for the current DataTable in the worksheet and update the current column leaving a one-column gap
                        DataHelper.AddHeadersToWorksheet(userWorksheet, quizResponseData, userCurrentRow, userCurrentColumn);
                        DataHelper.AddDataToWorksheet(userWorksheet, quizResponseData, userCurrentRow + 1, userCurrentColumn);
                        userCurrentColumn += quizResponseData.Columns.Count + 1;

                        // Filter the user's quiz feedback response data
                        var quizFeedbackResponseTable = dataTables[3];
                        var quizFeedbackResponseData = DataHelper.FilterDataTable(quizFeedbackResponseTable, "User Name", user);

                        // Handle empty DataTable if no feedback responses are found
                        if (quizFeedbackResponseData.Rows.Count == 0)
                        {
                            quizFeedbackResponseData = DataHelper.HandleEmptyDataTable(
                                quizFeedbackResponseData, 
                                "Quiz", 
                                "No Feedback Responses found as user is either not a participant or has not submitted any feedback"
                                );
                        }

                        // Add headers and data for the current DataTable in the worksheet and update the current column leaving a one-column gap
                        DataHelper.AddHeadersToWorksheet(userWorksheet, quizFeedbackResponseData, userCurrentRow, userCurrentColumn);
                        DataHelper.AddDataToWorksheet(userWorksheet, quizFeedbackResponseData, userCurrentRow + 1, userCurrentColumn);
                        userCurrentColumn += quizFeedbackResponseData.Columns.Count + 1;

                        // Filter the user's quiz self assessment data
                        var quizSelfAssessmentTable = dataTables[4];
                        var quizSelfAssessmentData = DataHelper.FilterDataTable(quizSelfAssessmentTable, "User Name", user);

                        // Handle empty DataTable if no self assessment responses are found
                        if (quizSelfAssessmentData.Rows.Count == 0)
                        {
                            quizSelfAssessmentData = DataHelper.HandleEmptyDataTable(
                                quizSelfAssessmentData, 
                                "Quiz", 
                                "No Self-Assessment Responses found as user is either not a participant or has not submitted any self-assessment"
                                );
                        }

                        // Add headers and data for the current DataTable in the worksheet and update the current column leaving a one-column gap
                        DataHelper.AddHeadersToWorksheet(userWorksheet, quizSelfAssessmentData, userCurrentRow, userCurrentColumn);
                        DataHelper.AddDataToWorksheet(userWorksheet, quizSelfAssessmentData, userCurrentRow + 1, userCurrentColumn);

                    }

                }
                else
                {
                    var worksheet = workbook.Worksheets.Add("Report");
                    int currentRow = 1; // Starting row for the current DataTable
                    int currentColumn = 1; // Starting column for the first DataTable

                    // Loop through each DataTable in dataTables
                    foreach (var table in dataTables)
                    {
                        DataHelper.AddHeadersToWorksheet(worksheet, table, currentRow, currentColumn);
                        DataHelper.AddDataToWorksheet(worksheet, table, currentRow + 1, currentColumn);

                        // Update the starting column for the next DataTable, leaving a one-column gap
                        currentColumn += table.Columns.Count + 1;
                    }
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