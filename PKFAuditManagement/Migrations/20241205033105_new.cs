using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PKFAuditManagement.Migrations
{
    /// <inheritdoc />
    public partial class @new : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatbotDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatbotDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackForms",
                columns: table => new
                {
                    FeedbackFormID = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    Title = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackForms", x => x.FeedbackFormID);
                });

            migrationBuilder.CreateTable(
                name: "QC35Forms",
                columns: table => new
                {
                    QC35FormID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    PreparedBy = table.Column<string>(type: "text", nullable: false),
                    FileReference = table.Column<string>(type: "text", nullable: true),
                    ClientName = table.Column<string>(type: "text", nullable: true),
                    ReportingYearEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PartnerName = table.Column<string>(type: "text", nullable: true),
                    ManagerName = table.Column<string>(type: "text", nullable: true),
                    ImageFileName = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    FirstApprover = table.Column<string>(type: "text", nullable: true),
                    SecondApprover = table.Column<string>(type: "text", nullable: true),
                    IsFirstApproved = table.Column<bool>(type: "boolean", nullable: true),
                    IsSecondApproved = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QC35Forms", x => x.QC35FormID);
                });

            migrationBuilder.CreateTable(
                name: "QC35FormTestDescriptions",
                columns: table => new
                {
                    QC35FormTestDescriptionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QC35FormID = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QC35FormTestDescriptions", x => x.QC35FormTestDescriptionID);
                });

            migrationBuilder.CreateTable(
                name: "QC6FormFeeDetails",
                columns: table => new
                {
                    QC6FormFeeDetailID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QC6FormID = table.Column<int>(type: "integer", nullable: false),
                    NatureOfService = table.Column<string>(type: "text", nullable: false),
                    Fee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OtherService = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QC6FormFeeDetails", x => x.QC6FormFeeDetailID);
                });

            migrationBuilder.CreateTable(
                name: "QC6Forms",
                columns: table => new
                {
                    QC6FormID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    FileReference = table.Column<string>(type: "text", nullable: false),
                    ProspectiveClient = table.Column<string>(type: "text", nullable: false),
                    PeriodEnded = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EngagementType = table.Column<string>(type: "text", nullable: false),
                    Industry = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    IsTemplate = table.Column<bool>(type: "boolean", nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    FormSubmissionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PKFEntityProposingService = table.Column<string>(type: "text", nullable: false),
                    SourceOfReferral = table.Column<string>(type: "text", nullable: false),
                    NatureOfServiceForEstimateFee = table.Column<string>(type: "text", nullable: false),
                    EstimatedFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BudgetedTimeCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BudgetedFeeRecoveryRate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BudgetedFeeRecoveryRateComment = table.Column<string>(type: "text", nullable: true),
                    OutstandingUnpaidFees = table.Column<bool>(type: "boolean", nullable: false),
                    OutstandingUnpaidFeesComment = table.Column<string>(type: "text", nullable: true),
                    AuditFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FeeConcentration = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ConflictsCheckDone = table.Column<bool>(type: "boolean", nullable: false),
                    TypeOfActivities = table.Column<string>(type: "text", nullable: false),
                    ComplexityOfEngagement = table.Column<string>(type: "text", nullable: false),
                    PredecessorAuditor = table.Column<bool>(type: "boolean", nullable: false),
                    ReasonsForDiscontinuance = table.Column<string>(type: "text", nullable: true),
                    PublicInterestEntity = table.Column<bool>(type: "boolean", nullable: false),
                    PublicInterestEntityType = table.Column<string>(type: "text", nullable: true),
                    IsSubForm2NotApplicable = table.Column<bool>(type: "boolean", nullable: false),
                    IsSubForm3NotApplicable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QC6Forms", x => x.QC6FormID);
                });

            migrationBuilder.CreateTable(
                name: "QC6SubForms",
                columns: table => new
                {
                    QC6SubFormID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubFormType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QC6SubForms", x => x.QC6SubFormID);
                });

            migrationBuilder.CreateTable(
                name: "QC7Forms",
                columns: table => new
                {
                    QC7FormID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    FileReference = table.Column<string>(type: "text", nullable: false),
                    Client = table.Column<string>(type: "text", nullable: false),
                    PeriodEnded = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EngagementType = table.Column<string>(type: "text", nullable: false),
                    Industry = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    FormSubmissionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PriorYearFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TimeCosts = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PriorYearRecoveryRate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PriorYearRecoveryRateComment = table.Column<string>(type: "text", nullable: true),
                    AnyOutstandingUnpaidAuditFees = table.Column<bool>(type: "boolean", nullable: false),
                    AnyOutstandingUnpaidAuditFeesComment = table.Column<string>(type: "text", nullable: true),
                    TypeOfClientActivities = table.Column<string>(type: "text", nullable: false),
                    RiskRatingPriorYear = table.Column<string>(type: "text", nullable: false),
                    AnySuspiciousTransactionReportFiled = table.Column<bool>(type: "boolean", nullable: false),
                    SuspiciousTransactionReportFiledComment = table.Column<string>(type: "text", nullable: true),
                    SafeguardReviewerName = table.Column<string>(type: "text", nullable: true),
                    AnyOutstandingUnpaidNonAuditFees = table.Column<bool>(type: "boolean", nullable: false),
                    AnyOutstandingUnpaidNonAuditFeesComment = table.Column<string>(type: "text", nullable: true),
                    AuditFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FeeConcentration = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ProposedFeeCurrentYear = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BudgetedTimeCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ProposedRecoveryRateCurrentYear = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsPublicInterestEntity = table.Column<bool>(type: "boolean", nullable: false),
                    PublicInterestEntityType = table.Column<string>(type: "text", nullable: true),
                    IsSubForm2NotApplicable = table.Column<bool>(type: "boolean", nullable: false),
                    IsSubForm3NotApplicable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QC7Forms", x => x.QC7FormID);
                });

            migrationBuilder.CreateTable(
                name: "QC7SubForms",
                columns: table => new
                {
                    QC7SubFormID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubFormType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QC7SubForms", x => x.QC7SubFormID);
                });

            migrationBuilder.CreateTable(
                name: "QCDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentType = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    QC6FormID = table.Column<int>(type: "integer", nullable: true),
                    QC7FormID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QCDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SelfAssessmentForms",
                columns: table => new
                {
                    SelfAssessmentFormID = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelfAssessmentForms", x => x.SelfAssessmentFormID);
                });

            migrationBuilder.CreateTable(
                name: "SignedFSForm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Client = table.Column<string>(type: "text", nullable: false),
                    AuditedReportDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FinancialYearEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PartnerEmail = table.Column<string>(type: "text", nullable: false),
                    UserEmail = table.Column<string>(type: "text", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignedFSForm", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackQuestions",
                columns: table => new
                {
                    FeedbackQuestionID = table.Column<Guid>(type: "uuid", nullable: false),
                    FeedbackFormID = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackQuestions", x => x.FeedbackQuestionID);
                    table.ForeignKey(
                        name: "FK_FeedbackQuestions_FeedbackForms_FeedbackFormID",
                        column: x => x.FeedbackFormID,
                        principalTable: "FeedbackForms",
                        principalColumn: "FeedbackFormID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QC35ChecklistItems",
                columns: table => new
                {
                    QC35ChecklistItemID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QC35FormID = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Response = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QC35ChecklistItems", x => x.QC35ChecklistItemID);
                    table.ForeignKey(
                        name: "FK_QC35ChecklistItems_QC35Forms_QC35FormID",
                        column: x => x.QC35FormID,
                        principalTable: "QC35Forms",
                        principalColumn: "QC35FormID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QC6FormConclusions",
                columns: table => new
                {
                    QC6FormConclusionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QC6FormID = table.Column<int>(type: "integer", nullable: true),
                    AnySignificantRisk = table.Column<bool>(type: "boolean", nullable: false),
                    SignificantRiskComment = table.Column<string>(type: "text", nullable: true),
                    NewEngagementRiskRating = table.Column<string>(type: "text", nullable: false),
                    NewEngagementRiskRatingReason = table.Column<string>(type: "text", nullable: true),
                    EngagementSubjectedTo = table.Column<string>(type: "text", nullable: false),
                    SafeguardReviewerAssigned = table.Column<string>(type: "text", nullable: false),
                    IsNewEngagementAcceptance = table.Column<string>(type: "text", nullable: false),
                    IsSuspiciousTransactionReportFiled = table.Column<bool>(type: "boolean", nullable: false),
                    SuspiciousTransactionReportFiledRationale = table.Column<string>(type: "text", nullable: true),
                    Satisfaction = table.Column<string>(type: "text", nullable: false),
                    PreparedBy = table.Column<string>(type: "text", nullable: false),
                    PreparedByDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EPHODApprovedBy = table.Column<string>(type: "text", nullable: true),
                    EPHODApprovedByDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    MPHODQMPApprovedBy = table.Column<string>(type: "text", nullable: true),
                    MPHODQMPApprovedByDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsFirstApproved = table.Column<bool>(type: "boolean", nullable: false),
                    IsSecondApproved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QC6FormConclusions", x => x.QC6FormConclusionID);
                    table.ForeignKey(
                        name: "FK_QC6FormConclusions_QC6Forms_QC6FormID",
                        column: x => x.QC6FormID,
                        principalTable: "QC6Forms",
                        principalColumn: "QC6FormID");
                });

            migrationBuilder.CreateTable(
                name: "TNATNEAssessments",
                columns: table => new
                {
                    TNATNEAssessmentID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QC6FormID = table.Column<int>(type: "integer", nullable: true),
                    QC7FormID = table.Column<int>(type: "integer", nullable: true),
                    SectionCEvaluation = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TNATNEAssessments", x => x.TNATNEAssessmentID);
                    table.ForeignKey(
                        name: "FK_TNATNEAssessments_QC6Forms_QC6FormID",
                        column: x => x.QC6FormID,
                        principalTable: "QC6Forms",
                        principalColumn: "QC6FormID");
                });

            migrationBuilder.CreateTable(
                name: "QC6FormObjectives",
                columns: table => new
                {
                    QC6FormObjectiveID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QC6SubFormID = table.Column<int>(type: "integer", nullable: false),
                    ObjectiveNo = table.Column<int>(type: "integer", nullable: false),
                    Objective = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QC6FormObjectives", x => x.QC6FormObjectiveID);
                    table.ForeignKey(
                        name: "FK_QC6FormObjectives_QC6SubForms_QC6SubFormID",
                        column: x => x.QC6SubFormID,
                        principalTable: "QC6SubForms",
                        principalColumn: "QC6SubFormID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QC7FormConclusions",
                columns: table => new
                {
                    QC7FormConclusionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QC7FormID = table.Column<int>(type: "integer", nullable: true),
                    AnyRiskAssociated = table.Column<bool>(type: "boolean", nullable: false),
                    RiskExplanationCurrentYearPriorYear = table.Column<string>(type: "text", nullable: true),
                    IsSafeguardApplied = table.Column<bool>(type: "boolean", nullable: false),
                    SafeguardsApplied = table.Column<string>(type: "text", nullable: true),
                    NatureOfSafeguard = table.Column<string>(type: "text", nullable: true),
                    ContinuingEngagementRiskRated = table.Column<string>(type: "text", nullable: false),
                    SafeguardReviewPartnerAssigned = table.Column<string>(type: "text", nullable: true),
                    IsSuspiciousTransactionReportFiled = table.Column<bool>(type: "boolean", nullable: false),
                    SuspiciousTransactionReportFiledRationale = table.Column<string>(type: "text", nullable: true),
                    EngagementRetainedRejected = table.Column<string>(type: "text", nullable: false),
                    EMPreparedBy = table.Column<string>(type: "text", nullable: true),
                    EMPreparedByDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EPHODApprovedBy = table.Column<string>(type: "text", nullable: true),
                    EPHODApprovedByDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    MPHODQMPApprovedBy = table.Column<string>(type: "text", nullable: true),
                    MPHODQMPApprovedByDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsFirstApproved = table.Column<bool>(type: "boolean", nullable: false),
                    IsSecondApproved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QC7FormConclusions", x => x.QC7FormConclusionID);
                    table.ForeignKey(
                        name: "FK_QC7FormConclusions_QC7Forms_QC7FormID",
                        column: x => x.QC7FormID,
                        principalTable: "QC7Forms",
                        principalColumn: "QC7FormID");
                });

            migrationBuilder.CreateTable(
                name: "QC7FormFeeDetails",
                columns: table => new
                {
                    QC7FormFeeDetailID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QC7FormID = table.Column<int>(type: "integer", nullable: false),
                    NatureOfService = table.Column<string>(type: "text", nullable: false),
                    Fee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OtherService = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QC7FormFeeDetails", x => x.QC7FormFeeDetailID);
                    table.ForeignKey(
                        name: "FK_QC7FormFeeDetails_QC7Forms_QC7FormID",
                        column: x => x.QC7FormID,
                        principalTable: "QC7Forms",
                        principalColumn: "QC7FormID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QC7FormObjectives",
                columns: table => new
                {
                    QC7FormObjectiveID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QC7SubFormID = table.Column<int>(type: "integer", nullable: false),
                    ObjectiveNo = table.Column<int>(type: "integer", nullable: false),
                    Objective = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QC7FormObjectives", x => x.QC7FormObjectiveID);
                    table.ForeignKey(
                        name: "FK_QC7FormObjectives_QC7SubForms_QC7SubFormID",
                        column: x => x.QC7SubFormID,
                        principalTable: "QC7SubForms",
                        principalColumn: "QC7SubFormID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Quiz",
                columns: table => new
                {
                    QuizID = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    QuizStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    QuizEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FeedbackFormID = table.Column<Guid>(type: "uuid", nullable: false),
                    SelfAssessmentFormID = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quiz", x => x.QuizID);
                    table.ForeignKey(
                        name: "FK_Quiz_FeedbackForms_FeedbackFormID",
                        column: x => x.FeedbackFormID,
                        principalTable: "FeedbackForms",
                        principalColumn: "FeedbackFormID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quiz_SelfAssessmentForms_SelfAssessmentFormID",
                        column: x => x.SelfAssessmentFormID,
                        principalTable: "SelfAssessmentForms",
                        principalColumn: "SelfAssessmentFormID");
                });

            migrationBuilder.CreateTable(
                name: "SelfAssessmentQuestions",
                columns: table => new
                {
                    SelfAssessmentQuestionID = table.Column<Guid>(type: "uuid", nullable: false),
                    SelfAssessmentFormID = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelfAssessmentQuestions", x => x.SelfAssessmentQuestionID);
                    table.ForeignKey(
                        name: "FK_SelfAssessmentQuestions_SelfAssessmentForms_SelfAssessmentF~",
                        column: x => x.SelfAssessmentFormID,
                        principalTable: "SelfAssessmentForms",
                        principalColumn: "SelfAssessmentFormID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TNATNESectionBs",
                columns: table => new
                {
                    TNATNESectionBID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TNATNEAssessmentID = table.Column<int>(type: "integer", nullable: false),
                    IsAudit = table.Column<string>(type: "text", nullable: false),
                    Q1 = table.Column<bool>(type: "boolean", nullable: false),
                    Q2 = table.Column<bool>(type: "boolean", nullable: false),
                    Q3 = table.Column<bool>(type: "boolean", nullable: false),
                    Q4 = table.Column<bool>(type: "boolean", nullable: false),
                    Q5 = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TNATNESectionBs", x => x.TNATNESectionBID);
                    table.ForeignKey(
                        name: "FK_TNATNESectionBs_TNATNEAssessments_TNATNEAssessmentID",
                        column: x => x.TNATNEAssessmentID,
                        principalTable: "TNATNEAssessments",
                        principalColumn: "TNATNEAssessmentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TNATNESectionDs",
                columns: table => new
                {
                    TNATNESectionDID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TNATNEAssessmentID = table.Column<int>(type: "integer", nullable: false),
                    Q1Comment = table.Column<string>(type: "text", nullable: true),
                    Q2Comment = table.Column<string>(type: "text", nullable: true),
                    Q3Comment = table.Column<string>(type: "text", nullable: true),
                    Q4Comment = table.Column<string>(type: "text", nullable: true),
                    Q5Comment = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TNATNESectionDs", x => x.TNATNESectionDID);
                    table.ForeignKey(
                        name: "FK_TNATNESectionDs_TNATNEAssessments_TNATNEAssessmentID",
                        column: x => x.TNATNEAssessmentID,
                        principalTable: "TNATNEAssessments",
                        principalColumn: "TNATNEAssessmentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QC6FormTestDescriptions",
                columns: table => new
                {
                    QC6FormTestDescriptionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QC6FormObjectiveID = table.Column<int>(type: "integer", nullable: false),
                    DescriptionNo = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QC6FormTestDescriptions", x => x.QC6FormTestDescriptionID);
                    table.ForeignKey(
                        name: "FK_QC6FormTestDescriptions_QC6FormObjectives_QC6FormObjectiveID",
                        column: x => x.QC6FormObjectiveID,
                        principalTable: "QC6FormObjectives",
                        principalColumn: "QC6FormObjectiveID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QC7FormTestDescriptions",
                columns: table => new
                {
                    QC7FormTestDescriptionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QC7FormObjectiveID = table.Column<int>(type: "integer", nullable: false),
                    DescriptionNo = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QC7FormTestDescriptions", x => x.QC7FormTestDescriptionID);
                    table.ForeignKey(
                        name: "FK_QC7FormTestDescriptions_QC7FormObjectives_QC7FormObjectiveID",
                        column: x => x.QC7FormObjectiveID,
                        principalTable: "QC7FormObjectives",
                        principalColumn: "QC7FormObjectiveID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attempt",
                columns: table => new
                {
                    AttemptID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<string>(type: "text", nullable: false),
                    QuizID = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attempt", x => x.AttemptID);
                    table.ForeignKey(
                        name: "FK_Attempt_Quiz_QuizID",
                        column: x => x.QuizID,
                        principalTable: "Quiz",
                        principalColumn: "QuizID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackResponses",
                columns: table => new
                {
                    FeedbackResponseID = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    QuizID = table.Column<Guid>(type: "uuid", nullable: false),
                    FeedbackQuestionID = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedBy = table.Column<string>(type: "text", nullable: false),
                    Response = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackResponses", x => x.FeedbackResponseID);
                    table.ForeignKey(
                        name: "FK_FeedbackResponses_FeedbackQuestions_FeedbackQuestionID",
                        column: x => x.FeedbackQuestionID,
                        principalTable: "FeedbackQuestions",
                        principalColumn: "FeedbackQuestionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeedbackResponses_Quiz_QuizID",
                        column: x => x.QuizID,
                        principalTable: "Quiz",
                        principalColumn: "QuizID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    ParticipantID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<string>(type: "text", nullable: false),
                    QuizID = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    ClockedAttendance = table.Column<bool>(type: "boolean", nullable: false),
                    QuizDone = table.Column<bool>(type: "boolean", nullable: false),
                    FeedbackDone = table.Column<bool>(type: "boolean", nullable: false),
                    OverallCompletion = table.Column<bool>(type: "boolean", nullable: false),
                    OverallCompletionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => x.ParticipantID);
                    table.ForeignKey(
                        name: "FK_Participants_Quiz_QuizID",
                        column: x => x.QuizID,
                        principalTable: "Quiz",
                        principalColumn: "QuizID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    QuestionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuizID = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CorrectOptionText = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.QuestionID);
                    table.ForeignKey(
                        name: "FK_Questions_Quiz_QuizID",
                        column: x => x.QuizID,
                        principalTable: "Quiz",
                        principalColumn: "QuizID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SelfAssessmentResponses",
                columns: table => new
                {
                    SelfAssessmentResponseID = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizID = table.Column<Guid>(type: "uuid", nullable: false),
                    SelfAssessmentQuestionID = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedBy = table.Column<string>(type: "text", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Stage = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelfAssessmentResponses", x => x.SelfAssessmentResponseID);
                    table.ForeignKey(
                        name: "FK_SelfAssessmentResponses_Quiz_QuizID",
                        column: x => x.QuizID,
                        principalTable: "Quiz",
                        principalColumn: "QuizID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SelfAssessmentResponses_SelfAssessmentQuestions_SelfAssessm~",
                        column: x => x.SelfAssessmentQuestionID,
                        principalTable: "SelfAssessmentQuestions",
                        principalColumn: "SelfAssessmentQuestionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QC6FormTests",
                columns: table => new
                {
                    QC6FormTestID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QC6FormID = table.Column<int>(type: "integer", nullable: false),
                    QC6FormTestDescriptionID = table.Column<int>(type: "integer", nullable: false),
                    SignOffBy = table.Column<string>(type: "text", nullable: true),
                    SignOffDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Comments = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QC6FormTests", x => x.QC6FormTestID);
                    table.ForeignKey(
                        name: "FK_QC6FormTests_QC6FormTestDescriptions_QC6FormTestDescription~",
                        column: x => x.QC6FormTestDescriptionID,
                        principalTable: "QC6FormTestDescriptions",
                        principalColumn: "QC6FormTestDescriptionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QC6FormTests_QC6Forms_QC6FormID",
                        column: x => x.QC6FormID,
                        principalTable: "QC6Forms",
                        principalColumn: "QC6FormID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QC7FormTests",
                columns: table => new
                {
                    QC7FormTestID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QC7FormID = table.Column<int>(type: "integer", nullable: false),
                    QC7FormTestDescriptionID = table.Column<int>(type: "integer", nullable: false),
                    SignOffBy = table.Column<string>(type: "text", nullable: true),
                    SignOffDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Comments = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QC7FormTests", x => x.QC7FormTestID);
                    table.ForeignKey(
                        name: "FK_QC7FormTests_QC7FormTestDescriptions_QC7FormTestDescription~",
                        column: x => x.QC7FormTestDescriptionID,
                        principalTable: "QC7FormTestDescriptions",
                        principalColumn: "QC7FormTestDescriptionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QC7FormTests_QC7Forms_QC7FormID",
                        column: x => x.QC7FormID,
                        principalTable: "QC7Forms",
                        principalColumn: "QC7FormID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Option",
                columns: table => new
                {
                    OptionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuestionID = table.Column<int>(type: "integer", nullable: false),
                    OptionText = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Option", x => x.OptionID);
                    table.ForeignKey(
                        name: "FK_Option_Questions_QuestionID",
                        column: x => x.QuestionID,
                        principalTable: "Questions",
                        principalColumn: "QuestionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizResponse",
                columns: table => new
                {
                    QuizResponseID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AttemptID = table.Column<int>(type: "integer", nullable: false),
                    QuestionID = table.Column<int>(type: "integer", nullable: false),
                    SelectedOption = table.Column<string>(type: "text", nullable: false),
                    QuestionsQuestionID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizResponse", x => x.QuizResponseID);
                    table.ForeignKey(
                        name: "FK_QuizResponse_Attempt_AttemptID",
                        column: x => x.AttemptID,
                        principalTable: "Attempt",
                        principalColumn: "AttemptID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizResponse_Questions_QuestionID",
                        column: x => x.QuestionID,
                        principalTable: "Questions",
                        principalColumn: "QuestionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuizResponse_Questions_QuestionsQuestionID",
                        column: x => x.QuestionsQuestionID,
                        principalTable: "Questions",
                        principalColumn: "QuestionID");
                });

            migrationBuilder.InsertData(
                table: "QC35FormTestDescriptions",
                columns: new[] { "QC35FormTestDescriptionID", "Description", "QC35FormID" },
                values: new object[,]
                {
                    { 1, "No. of working paper files", 1 },
                    { 2, "Working papers are transferred from arch files to paper files", 1 },
                    { 3, "Working paper files are numbered sequentially", 1 },
                    { 4, "All working papers in each file is complete (Manager to initial all working papers)", 1 },
                    { 5, "Date of Audit Report", 1 },
                    { 6, "Date of approval of files for archival is within 60 days from date of Audit Report", 1 },
                    { 7, "Date of approval and confirmation that CaseWare Audit files has been locked down within 60 days from the date of the Audit Report (if applicable). Refer to the screenshot of CaseWare below.", 1 }
                });

            migrationBuilder.InsertData(
                table: "QC6Forms",
                columns: new[] { "QC6FormID", "AuditFee", "BudgetedFeeRecoveryRate", "BudgetedFeeRecoveryRateComment", "BudgetedTimeCost", "ComplexityOfEngagement", "ConflictsCheckDone", "CreatedBy", "EngagementType", "EstimatedFee", "FeeConcentration", "FileReference", "FormSubmissionDate", "GrandTotal", "Industry", "IsSubForm2NotApplicable", "IsSubForm3NotApplicable", "IsTemplate", "NatureOfServiceForEstimateFee", "OutstandingUnpaidFees", "OutstandingUnpaidFeesComment", "PKFEntityProposingService", "PeriodEnded", "PredecessorAuditor", "ProspectiveClient", "PublicInterestEntity", "PublicInterestEntityType", "ReasonsForDiscontinuance", "RejectionReason", "SourceOfReferral", "Status", "TypeOfActivities" },
                values: new object[] { 1, 0.00m, 0.00m, null, 0.00m, "", true, "", "", 0.00m, 0.00m, "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.00m, "", false, false, true, "", false, null, "", null, false, "", false, "", "", null, "", "Pending", "" });

            migrationBuilder.InsertData(
                table: "QC6SubForms",
                columns: new[] { "QC6SubFormID", "SubFormType" },
                values: new object[,]
                {
                    { 1, "NEW ENGAGEMENT" },
                    { 2, "AUDIT AND REVIEW ENGAGEMENTS ONLY - FIRM AND NETWORK INDEPENDENCE (Confirm that no conflict exist. If this section is not applicable, please indicate “NA”)" },
                    { 3, "NON-ASSURANCE ENGAGEMENTS ONLY - FIRM AND NETWORK INDEPENDENCE (Confirm that no conflict exist. If this section is not applicable, please indicate “NA”)" }
                });

            migrationBuilder.InsertData(
                table: "QC7SubForms",
                columns: new[] { "QC7SubFormID", "SubFormType" },
                values: new object[,]
                {
                    { 1, "CONTINUING ENGAGEMENT" },
                    { 2, "AUDIT AND REVIEW ENGAGEMENTS ONLY - FIRM AND NETWORK INDEPENDENCE (Confirm that no conflict exist. If this section is not applicable, please indicate “NA”)" },
                    { 3, "NON-ASSURANCE ENGAGEMENTS ONLY - FIRM AND NETWORK INDEPENDENCE (Confirm that no conflict exist. If this section is not applicable, please indicate “NA”)" }
                });

            migrationBuilder.InsertData(
                table: "QC6FormConclusions",
                columns: new[] { "QC6FormConclusionID", "AnySignificantRisk", "EPHODApprovedBy", "EPHODApprovedByDate", "EngagementSubjectedTo", "IsFirstApproved", "IsNewEngagementAcceptance", "IsSecondApproved", "IsSuspiciousTransactionReportFiled", "MPHODQMPApprovedBy", "MPHODQMPApprovedByDate", "NewEngagementRiskRating", "NewEngagementRiskRatingReason", "PreparedBy", "PreparedByDate", "QC6FormID", "SafeguardReviewerAssigned", "Satisfaction", "SignificantRiskComment", "SuspiciousTransactionReportFiledRationale" },
                values: new object[] { 1, false, null, null, "Full safeguard review (H1)", false, "Accepted", false, false, null, null, "High Risk (H1)", "The Company being a limited liability company incorporated and domiciled in Singapore and is listed on the Main Board of the Stock Exchange of Singapore Limited (the “SGX”) is automatically marked as high risk client.", "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "", "Yes.", null, null });

            migrationBuilder.InsertData(
                table: "QC6FormObjectives",
                columns: new[] { "QC6FormObjectiveID", "Objective", "ObjectiveNo", "QC6SubFormID" },
                values: new object[,]
                {
                    { 1, "Objective – To ensure that the work for the client does not involve unacceptable risks", 1, 1 },
                    { 2, "Objective – To ensure that the firm can comply with relevant ethical requirements", 2, 1 },
                    { 3, "Objective - To ensure that the firm has the ability to perform the professional services properly", 3, 1 },
                    { 4, "Objective - To ensure that our appointment is properly effected and that the scope of our work is acknowledged by the client", 4, 1 },
                    { 5, "Objective – To ensure that the entity has been properly constituted in compliance with relevant legislation.", 5, 1 },
                    { 6, "Objective – To establish the identity of the client to assist in identifying possible instances of money laundering (“AML”) and terrorism financing activities (“CTF”).", 6, 1 },
                    { 7, "Objective – Acceptance Procedures", 7, 2 },
                    { 8, "Objective – Acceptance Procedures", 8, 3 }
                });

            migrationBuilder.InsertData(
                table: "QC7FormObjectives",
                columns: new[] { "QC7FormObjectiveID", "Objective", "ObjectiveNo", "QC7SubFormID" },
                values: new object[,]
                {
                    { 1, "Objective – To ensure that recurring work for the client does not involve unacceptable risks", 1, 1 },
                    { 2, "Objective – To ensure that the firm can comply with relevant ethical requirements", 2, 1 },
                    { 3, "Objective - To ensure that the firm has the ability to perform the professional services properly", 3, 1 },
                    { 4, "Objective – To ensure that the fees will be adequate and collectible", 4, 1 },
                    { 5, "Objective - To ensure that the client's understanding of the scope and terms of the engagement agrees with our own and no events have occurred to adversely affect our relationship with the client", 5, 1 },
                    { 6, "Objective - To ensure that the client’s understanding of the scope and terms of the engagement is agreed", 6, 1 },
                    { 7, "Objective – To establish the identity of the client to assist in identifying possible instances of money laundering (“AML”) and terrorism financing activities (“CTF”)", 7, 1 },
                    { 8, "Objective – Continuance Procedures", 8, 2 },
                    { 9, "Objective – Continuance Procedures", 9, 3 }
                });

            migrationBuilder.InsertData(
                table: "QC6FormTestDescriptions",
                columns: new[] { "QC6FormTestDescriptionID", "Description", "DescriptionNo", "QC6FormObjectiveID" },
                values: new object[,]
                {
                    { 1, "<p>1. Decide whether we can rely on the management's integrity and whether association with the client may damage the firm's reputation. Consider:</p><ul><li>discussion with the party who introduced the client;</li><li>knowledge gained through work done by other departments within the firm;</li><li>media and any other reports;</li><li>client's relationship with any regulatory authority; and</li><li>client's attitude to fairness in financial reporting.</li></ul>", 1, 1 },
                    { 2, "<p>2. If we have doubts resulting from the considerations above, decide whether the potential risks can be mitigated by introducing additional precautions, such as:</p><ul><li>additional procedures, especially independent third-party confirmations; or</li><li>additional review by an engagement quality reviewer (i.e. safeguard reviewer).</li></ul>\r\n", 2, 1 },
                    { 3, "<p>3. Consider whether the firm can comply with relevant ethical requirements, including the firm’s independence. Include in your consideration potential independence or conflicts of interest problems arising from relationships with other clients, firm members, or their families; providing non-assurance services; or unpaid fees.</p>\r\n<p>4. Regarding the proposed relationship between the firm and the client or any associated companies, decide whether we are, and can be seen to be, independent of the client.  Consider:</p><ul><li>Undue dependence</li><li>Proposed contingent fees</li><li>Preparation of accounting records</li><li>Receipt of hospitality, goods, or services</li><li>Voting on audit appointments</li><li>Director or senior employee joining client</li><li>Participation in affairs of client</li><li>Influences from the outside practice (including associated firms)</li><li>Litigation (actual or threatened)</li><li>Family or professional relationships</li><li>Prohibited services</li><li>Financial involvement, including trustee investments, mutual business interests, loans, beneficial interests- shares/trusts, overdue fees</li><li>Network form independence conflict checking procedures performed (i.e. conflict of interest to be considered by completing either “Audit and Review Engagements – Firm and Network Independence” or “Non-assurance Engagements – Firm and Network Independence,” which are part of this form.</li></ul>\r\n", 3, 2 },
                    { 4, "<p>5. Decide whether we have the competence to carry out the professional services properly. Consider whether special skills are needed to deal with the particular features or specialised reporting requirements of the client.</p>\r\n", 4, 3 },
                    { 5, "<p>6. Is there any reason to believe that management will not provide access to all information of which management is aware that is relevant to the preparation of the financial statements including access to information relevant to disclosures?</p>\r\n", 5, 3 },
                    { 6, "<p>7. Confirm that the firm will have adequate resources to be able to do the work properly at the time the client needs it. <br /> Note:  For example, to indicate if the engagement will be performed by outsourced staff.</p>\r\n", 6, 3 },
                    { 7, "<p>8. Send, and obtain a response to, a professional enquiry letter to the previous auditors/reporting accountants <b>(if any)</b>.</p>\r\n", 7, 4 },
                    { 8, "<p>9. Obtain and review the previous auditor’s letter of resignation <b>(if applicable)</b> and statement of circumstances connected with their leaving office.</p>\r\n", 8, 4 },
                    { 9, "<p>10. Obtain a copy of the minutes appointing PKF as auditors.</p>\r\n", 9, 4 },
                    { 10, "<p>11. Send an engagement letter and obtain the client’s agreement to its terms before starting work.</p>\r\n", 10, 4 },
                    { 11, "<p>12. Carry out other tests necessary to meet this objective and document such additional considerations.</p>\r\n", 11, 4 },
                    { 12, "<p>13. Obtain and file documents of constitution and certificate of incorporation <b>(where applicable)</b>.</p>\r\n", 12, 5 },
                    { 13, "<p>14. Where possible, verify that appropriate documents have been filed with the relevant authorities and that these support the information provided by the entity’s records.</p>\r\n", 13, 5 },
                    { 14, "<p>15. Carry out other tests if necessary to achieve this objective.</p>\r\n", 14, 5 },
                    { 15, "<p>16. Carry out procedures for establishing/verifying the identity of new clients for possible instances of money laundering if there are indicators or suspicion of such activities based on the evaluation of integrity of the new client set out above. <ol class=\"list-decimal pl-4\"><li>Perform a search of the company’s name, all directors and ultimate beneficial owners owning more than 25% equity interest in the ultimate holding company using LexisNexis. This screening covers not only identifying any PEPs but also terrorist names, terrorist organisations and names sanctioned by United States / United Nations. A copy of the screening should be attached to QC6(3).</li> <br> <li>Carry out a google check on the company’s name, names of the directors and ultimate BOs owning more than 25% equity interest in the ultimate holding company. A copy of the google check should be attached to QC6(3).</li> <br> <li>For audit clients whereby the companies are incorporated by our related entities, PKF-CAP Corporate Services Pte Ltd (“CS”) or PKF-Khoo Management Services Pte Ltd (“KMS”), then carry out a review of the screening and google checking that have been carried out by CS or KMS. If the result is satisfactory, then document the review that you have performed. No further screening or checking is required unless work performed is considered not adequate for our purposes. This placing of reliance is acceptable so long as the screening carried out by CS or KMS is current within 12 months from our client/engagement evaluation date.</li></ol> <i><b>Note:</b> If the prospective client is - (a) an entity listed on the Singapore Exchange; (b) an entity listed on a stock exchange outside Singapore; (c) a Singapore financial institution; (d) a financial institution incorporated or established outside Singapore; or (e) an investment vehicle, the managers of which are — (i) Singapore financial institutions; or (ii) financial institutions incorporated or established outside Singapore, under the Accountants Act 2004, Accountants (Prevention of Money Laundering and Financing of Terrorism) Rules 2023, an accounting entity need not inquire if there exists any beneficial owner unless the accounting entity has doubts about the veracity of the information obtained by the accounting entity in carrying out customer due diligence measures under these Rules or suspects that the client is carrying out or facilitating money laundering or the financing of terrorism. <br><br> “Beneficial owner” means — (a) an individual who ultimately owns all of the assets or undertakings of the client (whether or not the client is a body corporate); (b) an individual who has ultimate control or ultimate effective control over, or has executive authority in, the client; or (c) an individual on whose behalf the client has employed or engaged the services of an accounting entity.</i></p>", 15, 6 },
                    { 16, "<p>Are the directors identified to be a Political Exposed Person(“PEP”)? If yes,please establish where the source of funds are obtained?</p>\r\n", 16, 6 },
                    { 17, "<p>Are the ultimate beneficial owners (individuals owning more than 25% equity interest in the ultimate holding company) a PEP? If yes, please establish where the source of funds are obtained?</p>\r\n", 17, 6 },
                    { 18, "<p><b>When proposing for an audit or review engagement: </b><br><br>a) Is the potential client listed (public)? If yes, proceed to (c). If not, research the family tree and ascertain if there is a listed (or publicly traded) entity or entities in the group.<br><br>If there are listed entities in the group, proceed to (b). If the potential client is not listed and there are no listed entities in the group, proceed to (d).</p>\r\n", 18, 7 },
                    { 19, "<p>b) If there are listed entities in the group, investigate whether the firm or other PKF Member Firms provide audit or review services to these listed entities.<br><br>This investigation shall include direct enquiry of client management and review of the transnational entity listing and may be supplemented by an email to the Member Firm(s) geographically close to these listed entities (this is not required).<br><br>If the firm or other Member Firms provide audit or review services to listed entities in the group, proceed to (c). If not, proceed to (d).</p>\r\n", 19, 7 },
                    { 20, "<p>c) If the potential client is listed, or the firm or other Member Firms provide audit or review services to listed entities in the group, independence considerations must include all related entities as defined in the IESBA Code for Professional Accountants.<br><br>Enquire as to whether the firm or other Member Firms provide non-assurance services to the potential client or any related entities. This enquiry shall be direct enquiry of client management and review of the transnational entity listing and may be supplemented by an email to the Member Firm(s) geographically close to these related entities.<br><br>If the firm or other Member Firms provide non-assurance services to the potential client or any related entities, proceed to (f). If not, no further action is required.</p>\r\n", 20, 7 },
                    { 21, "<p>d) If the potential client is not listed and there are no listed entities in the group or the firm or other Member Firms do not provide audit or review services to any listed entities in the group, enquire as to whether the firm or other Member Firms provide non-assurance services to the potential client or entities over which the potential client has direct or indirect control.<br><br>This enquiry shall be direct enquiry of client management and review of the transnational entity listing and may be supplemented by an email to the Member Firm(s) geographically close to these entities.<br><br>If the firm or other Member Firms provide non-assurance services to the potential client or entities over which the potential client has direct or indirect control, proceed to (f). If not, proceed to (e).</p>\r\n", 21, 7 },
                    { 22, "<p>e) Is there reason to believe that a relationship or circumstance involving any other related entity of the potential client is relevant to the evaluation of the firm’s independence from the client? If yes, apply step (f) below to that related entity. If not, no further action is required.</p>\r\n", 22, 7 },
                    { 23, "<p>f) Evaluate the significance of the threats identified and apply safeguards when necessary to eliminate the threat or reduce it to an acceptable level. This may include not accepting the engagement.<br><br>Carry out the evaluation under the IESBA International Code of Ethics for Professional Accountants, including International Independence Standards and any local ethical code if it is stricter.</p>\r\n", 23, 7 },
                    { 24, "<p><b>When proposing for a non-assurance engagement:</b><br><br>a) Is the potential client listed (public)? If not, research the family tree and ascertain if there is a listed (or publicly traded) entity or entities in the group.<br><br>If the potential client is listed or there are other listed entities in the group, proceed to (b). If the potential client is not listed and there are no listed entities in the group, proceed to (c).</p>\r\n", 24, 8 },
                    { 25, "<p>b) If the potential client is listed or there are other listed entities in the group, investigate whether the firms or other PKF Member Firms provide audit or review services to these listed entities. This investigation shall include direct enquiry of client management and review of the transnational entity listing and may be supplemented by an email to the Member Firm(s) geographically close to these listed entities.<br><br>If the firm or other Member Firms do not provide audit or review services to listed entities in the group, proceed to (c).<br><br>If the firm or other Member Firms provide audit or review services to these listed entities, independence considerations must include all related entities as defined in the IESBA International Code for Professional Accountants, including International Independence Standards. Proceed to (d).</p>\r\n", 25, 8 },
                    { 26, "<p>c) If the potential client is not listed and there are no other listed entities in the group, or the firm or other Member Firms do not provide audit or review services to the listed entities in the group, enquire as to whether the firm or other Member Firms provide audit or review services to the potential client or entities that have direct or indirect control over the potential client.<br><br>This enquiry shall be direct enquiry of the client management and review of the transnational entity listing and may be supplemented by an email to the Member Firm(s) geographically close to these entities.<br><br>If the firm or other Member Firms provide audit or review services to the potential client or entities that have direct or indirect control over the potential client, proceed to (d). If not, no further action is required.</p>\r\n", 26, 8 },
                    { 27, "<p>d) Evaluate the significance of the threats identified and apply safeguards when necessary to eliminate the threat or reduce it to an acceptable level. This may include not accepting the engagement.<br><br>Carry out the evaluation under the IESBA International Code for Professional Accountants, including International Independence Standards and any local ethical code if it is stricter.</p>", 27, 8 }
                });

            migrationBuilder.InsertData(
                table: "QC7FormTestDescriptions",
                columns: new[] { "QC7FormTestDescriptionID", "Description", "DescriptionNo", "QC7FormObjectiveID" },
                values: new object[,]
                {
                    { 1, "<p>1. Decide whether we can rely on the management's integrity and whether our continued association with the client may damage the firm's reputation. Consider:</p><ul><li>previous year's experience;</li><li>knowledge gained through work done by other service areas within the firm;</li><li>media reports; and</li><li>client's relationship with any regulatory authority.</li></ul>\r\n", 1, 1 },
                    { 2, "<p>2. If we have doubts resulting from the considerations above, decide whether the potential risks can be mitigated by introducing additional precautions, such as:</p><ul><li>additional procedures; or</li><li>additional review by an engagement quality reviewer (i.e. safeguard reviewer)</li></ul>\r\n", 2, 1 },
                    { 3, "<p>3. Consider whether the firm can comply with relevant ethical requirements, including whether there have been any changes in the firm’s independence. Include in your consideration potential independence or conflicts of interest problems arising from relationships with other clients, firm members, or their families; providing non-assurance services; or unpaid fees.</p>\r\n", 3, 2 },
                    { 4, "<p>4. If there has been any change in the relationship between the firm and the client or any associated companies, decide whether we are still, and can be seen still to be, independent of the client. Consider:</p><ul><li>Conflicts of interest</li><li>Undue dependence</li><li>Concerns about losing the engagement</li><li>Contingent fees</li><li>Preparation of accounting records including information obtained from outside of the general and subsidiary ledger</li><li>Receipt of hospitality, goods or services</li><li>Voting on audit appointments</li><li>Director or senior employee joining client</li><li>Participation in affairs of client</li><li>Acting as auditor for a long time</li><li>Influences from outside the practice (including associated firms)</li><li>Litigation (actual or threatened)</li><li>Family or personal relationships</li><li>Provision of other services</li><li>Financial involvement: <ul><li>Trustee investments</li><li>Mutual business interests</li><li>Loans</li><li>Beneficial interests - shares/trusts</li><li>Overdue fees</li><li>Network firm independence threats do not exist </li></ul></li></ul><p>This consideration should include independence in relation to network firms.</p>\r\n", 4, 2 },
                    { 5, "<p>5. Decide whether we have the competence to carry out the professional services properly. Consider whether special skills are needed to deal with the particular features or specialised reporting requirements of the client.</p>\r\n", 5, 3 },
                    { 6, "<p>6. Confirm that the firm will have adequate resources to be able to do the work properly at the time the client needs it.</p>\r\n", 6, 3 },
                    { 7, "<p>7. Decide whether the level of fees agreed will be adequate to allow us to carry out the work deem necessary, properly and with integrity.</p>\r\n", 7, 4 },
                    { 8, "<p>8. Consider the client’s ability to pay the agreed fees.</p>\r\n", 8, 4 },
                    { 9, "<p>9. Consider whether events have occurred which may affect the firm’s relationship with the client. This should be done if there is:<ul><li>a significant change in management or ownership;</li><li>a change in the client’s legal advisers;</li><li>an adverse change in the client’s financial condition;</li><li>significant litigation against the client by a third party;</li><li>a change in the nature of the business;</li><li>a change in the scope of work to be carried out; or</li><li>a change in the firm’s responsibilities to any regulatory authority.</li></ul>Ensure appropriate procedures are carried out in response to the above matters including a re-evaluation of the firm’s independence, a reconsideration of management’s integrity and a reconsideration of responsibilities in relation to money laundering if relevant.</p>\r\n", 9, 5 },
                    { 10, "<p>10. Decide whether we need to renew our engagement letter. If yes, send a revised letter and obtain the client's agreement to its terms before starting work.</p>\r\n", 10, 6 },
                    { 11, "<p>11. For anti-money laundering (“AML”) and counter-terrorism financing (“CTF”) activities, please carry out the following procedures:<br><br>&ensp;&ensp;1. Perform a search of the company’s name, all directors and ultimate beneficial owners owning more than 25% equity interest in the ultimate holding company using LexisNexis. This screening covers not only identifying any PEPs but also terrorist names, terrorist organisations and names sanctioned by United States / United Nations. A copy of the screening should be attached to QC7(3).<br><br>&ensp;&ensp;2. Carry out a google check on the company’s name, the names of the directors and ultimate BOs owning more than 25% equity interest in the ultimate holding company. A copy of the google check should be attached to QC7(3).<br><br>&ensp;&ensp;3. For audit clients whereby the companies are incorporated by our related entities, PKF-CAP Corporate Services Pte Ltd (“CS”) or PKF-Khoo Management Services Pte Ltd (“KMS”), then carry out a review of the screening and google checking that have been carried out by CS or KMS. If the result is satisfactory, then document the review that you have performed. No further screening or checking is required unless work performed is considered not adequate for our purposes. This placing of reliance is acceptable so long as the screening carried out by CS or KMS is current within 12 months from our client/engagement evaluation date.<br><br><b><u>Note</u></b><br><br><i>i. <b>Note:</b> If the prospective client is - (a) an entity listed on the Singapore Exchange; (b) an entity listed on a stock exchange outside Singapore; (c) a Singapore financial institution; (d) a financial institution incorporated or established outside Singapore; or (e) an investment vehicle, the managers of which are — (i) Singapore financial institutions; or (ii) financial institutions incorporated or established outside Singapore, under the Accountants Act 2004, Accountants (Prevention of Money Laundering and Financing of Terrorism) Rules 2023, an accounting entity need not inquire if there exists any beneficial owner unless the accounting entity has doubts about the veracity of the information obtained by the accounting entity in carrying out customer due diligence measures under these Rules or suspects that the client is carrying out or facilitating money laundering or the financing of terrorism.<br><br>“Beneficial owner” means — (a) an individual who ultimately owns all of the assets or undertakings of the client (whether or not the client is a body corporate); (b) an individual who has ultimate control or ultimate effective control over, or has executive authority in, the client; or (c) an individual on whose behalf the client has employed or engaged the services of an accounting entity.<br><br>ii. If PEP identified, please refer to (a) any work performed in QC6 and (b) the services we provided in prior year such as audit to ascertain whether there are any suspicious transactions relating to AML or CTF that require us to extend our procedures for these areas.<br><br>iii. For PEPs that are linked to Peoples’ Action Party in Singapore and PEPs where their wealth is known publicly, there is no need to establish their source of funds for their operations.</i></p>", 11, 7 },
                    { 12, "<p><b>When deciding on whether or not to continue with an audit or review engagement:</b><br><br>Consider changes that may alter the original assessment of the acceptability of the client and engagement, including network firm independence.<br><br><i>Examples of changes that shall cause a careful reconsideration are:<ul><li>a significant change in the size, structure or nature of the client's business;</li><li>a major change in the ownership or management of the client; and</li><li>new regulatory reporting requirements.</li></ul></i><br>Network conflict checking shall require direct enquiry of the client management to determine whether there were any:<ul><li>changes in the controlling ownership interests over the entity;</li><li>changes in any controlling interests held by the entity;</li><li>new listings in the group;</li><li>any known change in service providers that may impact independence; or</li><li>changes in classification of the entity as public interest.</li></ul><br>Any such changes will require reconsideration of the relevant firm and network conflict checking procedures as outlined under acceptance above.</p>\r\n", 12, 8 },
                    { 13, "<p><b>When deciding on whether or not to continue with an audit or review engagement:</b><br><br>Consider changes that may alter the original assessment of the acceptability of the client and engagement, including network firm independence.<br><br><i>Examples of changes that shall cause a careful reconsideration are:<ul><li>a significant change in the size, structure or nature of the client's business;</li><li>a major change in the ownership or management of the client; and</li><li>new regulatory reporting requirements.</li></ul></i><br>Network conflict checking shall require direct enquiry of the client management to determine whether there were any:<ul><li>changes in the controlling ownership interests over the entity;</li><li>changes in any controlling interests held by the entity;</li><li>new listings in the group;</li><li>any known change in service providers that may impact independence; or</li><li>changes in classification of the entity as public interest.</li></ul><br>Any such changes will require reconsideration of the relevant firm and network conflict checking procedures as outlined under acceptance above.</p>\r\n", 13, 9 }
                });

            migrationBuilder.InsertData(
                table: "QC6FormTests",
                columns: new[] { "QC6FormTestID", "Comments", "QC6FormID", "QC6FormTestDescriptionID", "SignOffBy", "SignOffDate" },
                values: new object[,]
                {
                    { 1, "We have carried out the following procedures during our evaluation: <br><br>1) Public Record Review - Verified the company profile with the Accounting and Corporate Regulatory Authority (ACRA) business register and found no negative information. <br><br>2) Web Screening - Conducted web searches on the company name and its directors and authorised representatives, identifying no adverse news or concerns.<br><br>3) Database Screening - Conducted Lexis Nexis searches on the company name and its directors and authorised representatives, identifying no adverse news or concerns.<br><br>4) Conflict of Interest Check - Our internal TREND network conflict check confirmed no conflicts of interest.<br><br>5) Internal Conflict of Interest Check - Addressed and cleared any potential conflicts within PKF Singapore through email communication.<br><br>Based on these due diligence procedures, including discussions with management, we have identified no significant issues that could raise concerns regarding the company's integrity or potential reputational risks for PKF Singapore in association with them.", 1, 1, null, null },
                    { 2, "Our due diligence review, including a review of the company profile with ACRA, web and database screening, conflict checks, and discussions with management, identified no significant concerns that would hinder our ability to accept this engagement.", 1, 2, null, null },
                    { 3, "Our due diligence procedures, outlined above, identified no threats to our independence or conflicts of interest with the Company. Based on this assessment, we confirm compliance with all relevant ethical requirements, including those regarding firm independence.", 1, 3, null, null },
                    { 4, "Our team possesses the necessary skills and experience to successfully complete this engagement.", 1, 4, null, null },
                    { 5, "None.", 1, 5, null, null },
                    { 6, "Our team is well-resourced and prepared to handle this engagement effectively.", 1, 6, null, null },
                    { 7, "We have obtained professional clearance from _______.", 1, 7, null, null },
                    { 8, "Not applicable. <br><br> For audit engagement, if applicable: The reasons for the change of auditor were made public in their Company announcement.", 1, 8, null, null },
                    { 9, "Not applicable. <br><br> For audit engagement, if applicable: The reasons for the change of auditor were made public in their Company announcement.", 1, 9, null, null },
                    { 10, "The engagement letter will be prepared and sent to management to be signed upon the approval of QC6(3).", 1, 10, null, null },
                    { 11, "Having completed our due diligence procedures, we are confident that no further steps are necessary. Our robust acceptance process, as outlined above, ensures we can effectively serve the company.", 1, 11, null, null },
                    { 12, "Our client acceptance evaluation, including a review of the company's ACRA Business Profile, identified no concerns. A detailed review of the Articles and Memorandum of Association will be conducted as part of our standard statutory audit procedures after our appointment is confirmed.", 1, 12, null, null },
                    { 13, "A review of the Articles and Memorandum of Association is a standard part of our statutory audit procedures and will be conducted after our appointment is confirmed.", 1, 13, null, null },
                    { 14, "Not applicable since no exceptions are noted in our procedures performed above.", 1, 14, null, null },
                    { 15, "We have carried out the following procedures during our evaluation:<br><br>1) Public Record Review - Verified the company profile with the Accounting and Corporate Regulatory Authority (ACRA) business register and found no negative information.<br><br>2) Web Screening - Conducted web searches on the company name and its directors and authorised representatives, identifying no adverse news or concerns.<br><br>3) Database Screening - Conducted Lexis Nexis searches on the company name and its directors and authorised representatives, identifying no adverse news or concerns.<br><br>4) Conflict of Interest Check - Our internal TREND network conflict check confirmed no conflicts of interest.<br><br>5) Internal Conflict of Interest Check - Addressed and cleared any potential conflicts within PKF Singapore through email communication.<br><br>Our due diligence process includes a thorough review of public records, web screening, and database checks. This specifically focuses on identifying and screening ultimate beneficial owners with over 25% ownership in the ultimate holding company.  We screen for potential red flags, including Politically Exposed Persons (PEPs), individuals or organisations on terrorist watchlists, and those sanctioned by authorities.<br><br>Based on these thorough due diligence procedures, including discussions with management, we have identified no significant issues that could raise concerns regarding the company's integrity or potential reputational risks for PKF Singapore in association with them.", 1, 15, null, null },
                    { 16, "", 1, 16, null, null },
                    { 17, "For listed companies: Given the Company's listing on the Stock Exchange of __________, identifying ultimate beneficial owners beyond the information available in their publicly accessible annual report is not necessary.  We recommend referring to the Company's most recent annual report for details on significant shareholders.", 1, 17, null, null },
                    { 18, "Yes.", 1, 18, null, null },
                    { 19, "N/A.", 1, 19, null, null },
                    { 20, "Our due diligence included inquiries with the Company to confirm that neither PKF Singapore nor any other member firm provides non-assurance services to them or any related entities. We also conducted a comprehensive check through our TREND network database to verify this. <br><br> None.", 1, 20, null, null },
                    { 21, "NA.", 1, 21, null, null },
                    { 22, "NA.", 1, 22, null, null },
                    { 23, "Our due diligence procedures identified no threats to our independence or conflicts of interest with the Company or the Group. This assessment confirms our compliance with all relevant ethical requirements, including the International Code of Ethics for Professional Accountants (IESBA Code) and International Independence Standards, along with any stricter local ethical codes.", 1, 23, null, null },
                    { 24, "NA", 1, 24, null, null },
                    { 25, "NA", 1, 25, null, null },
                    { 26, "NA", 1, 26, null, null },
                    { 27, "NA", 1, 27, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attempt_QuizID",
                table: "Attempt",
                column: "QuizID");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackQuestions_FeedbackFormID",
                table: "FeedbackQuestions",
                column: "FeedbackFormID");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackResponses_FeedbackQuestionID",
                table: "FeedbackResponses",
                column: "FeedbackQuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackResponses_QuizID",
                table: "FeedbackResponses",
                column: "QuizID");

            migrationBuilder.CreateIndex(
                name: "IX_Option_QuestionID",
                table: "Option",
                column: "QuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_QuizID",
                table: "Participants",
                column: "QuizID");

            migrationBuilder.CreateIndex(
                name: "IX_QC35ChecklistItems_QC35FormID",
                table: "QC35ChecklistItems",
                column: "QC35FormID");

            migrationBuilder.CreateIndex(
                name: "IX_QC6FormConclusions_QC6FormID",
                table: "QC6FormConclusions",
                column: "QC6FormID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QC6FormObjectives_QC6SubFormID",
                table: "QC6FormObjectives",
                column: "QC6SubFormID");

            migrationBuilder.CreateIndex(
                name: "IX_QC6FormTestDescriptions_QC6FormObjectiveID",
                table: "QC6FormTestDescriptions",
                column: "QC6FormObjectiveID");

            migrationBuilder.CreateIndex(
                name: "IX_QC6FormTests_QC6FormID",
                table: "QC6FormTests",
                column: "QC6FormID");

            migrationBuilder.CreateIndex(
                name: "IX_QC6FormTests_QC6FormTestDescriptionID",
                table: "QC6FormTests",
                column: "QC6FormTestDescriptionID");

            migrationBuilder.CreateIndex(
                name: "IX_QC7FormConclusions_QC7FormID",
                table: "QC7FormConclusions",
                column: "QC7FormID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QC7FormFeeDetails_QC7FormID",
                table: "QC7FormFeeDetails",
                column: "QC7FormID");

            migrationBuilder.CreateIndex(
                name: "IX_QC7FormObjectives_QC7SubFormID",
                table: "QC7FormObjectives",
                column: "QC7SubFormID");

            migrationBuilder.CreateIndex(
                name: "IX_QC7FormTestDescriptions_QC7FormObjectiveID",
                table: "QC7FormTestDescriptions",
                column: "QC7FormObjectiveID");

            migrationBuilder.CreateIndex(
                name: "IX_QC7FormTests_QC7FormID",
                table: "QC7FormTests",
                column: "QC7FormID");

            migrationBuilder.CreateIndex(
                name: "IX_QC7FormTests_QC7FormTestDescriptionID",
                table: "QC7FormTests",
                column: "QC7FormTestDescriptionID");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_QuizID",
                table: "Questions",
                column: "QuizID");

            migrationBuilder.CreateIndex(
                name: "IX_Quiz_FeedbackFormID",
                table: "Quiz",
                column: "FeedbackFormID");

            migrationBuilder.CreateIndex(
                name: "IX_Quiz_SelfAssessmentFormID",
                table: "Quiz",
                column: "SelfAssessmentFormID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizResponse_AttemptID",
                table: "QuizResponse",
                column: "AttemptID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizResponse_QuestionID",
                table: "QuizResponse",
                column: "QuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizResponse_QuestionsQuestionID",
                table: "QuizResponse",
                column: "QuestionsQuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_SelfAssessmentQuestions_SelfAssessmentFormID",
                table: "SelfAssessmentQuestions",
                column: "SelfAssessmentFormID");

            migrationBuilder.CreateIndex(
                name: "IX_SelfAssessmentResponses_QuizID",
                table: "SelfAssessmentResponses",
                column: "QuizID");

            migrationBuilder.CreateIndex(
                name: "IX_SelfAssessmentResponses_SelfAssessmentQuestionID",
                table: "SelfAssessmentResponses",
                column: "SelfAssessmentQuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_TNATNEAssessments_QC6FormID",
                table: "TNATNEAssessments",
                column: "QC6FormID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TNATNESectionBs_TNATNEAssessmentID",
                table: "TNATNESectionBs",
                column: "TNATNEAssessmentID");

            migrationBuilder.CreateIndex(
                name: "IX_TNATNESectionDs_TNATNEAssessmentID",
                table: "TNATNESectionDs",
                column: "TNATNEAssessmentID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ChatbotDocuments");

            migrationBuilder.DropTable(
                name: "FeedbackResponses");

            migrationBuilder.DropTable(
                name: "Option");

            migrationBuilder.DropTable(
                name: "Participants");

            migrationBuilder.DropTable(
                name: "QC35ChecklistItems");

            migrationBuilder.DropTable(
                name: "QC35FormTestDescriptions");

            migrationBuilder.DropTable(
                name: "QC6FormConclusions");

            migrationBuilder.DropTable(
                name: "QC6FormFeeDetails");

            migrationBuilder.DropTable(
                name: "QC6FormTests");

            migrationBuilder.DropTable(
                name: "QC7FormConclusions");

            migrationBuilder.DropTable(
                name: "QC7FormFeeDetails");

            migrationBuilder.DropTable(
                name: "QC7FormTests");

            migrationBuilder.DropTable(
                name: "QCDocuments");

            migrationBuilder.DropTable(
                name: "QuizResponse");

            migrationBuilder.DropTable(
                name: "SelfAssessmentResponses");

            migrationBuilder.DropTable(
                name: "SignedFSForm");

            migrationBuilder.DropTable(
                name: "TNATNESectionBs");

            migrationBuilder.DropTable(
                name: "TNATNESectionDs");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "FeedbackQuestions");

            migrationBuilder.DropTable(
                name: "QC35Forms");

            migrationBuilder.DropTable(
                name: "QC6FormTestDescriptions");

            migrationBuilder.DropTable(
                name: "QC7FormTestDescriptions");

            migrationBuilder.DropTable(
                name: "QC7Forms");

            migrationBuilder.DropTable(
                name: "Attempt");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "SelfAssessmentQuestions");

            migrationBuilder.DropTable(
                name: "TNATNEAssessments");

            migrationBuilder.DropTable(
                name: "QC6FormObjectives");

            migrationBuilder.DropTable(
                name: "QC7FormObjectives");

            migrationBuilder.DropTable(
                name: "Quiz");

            migrationBuilder.DropTable(
                name: "QC6Forms");

            migrationBuilder.DropTable(
                name: "QC6SubForms");

            migrationBuilder.DropTable(
                name: "QC7SubForms");

            migrationBuilder.DropTable(
                name: "FeedbackForms");

            migrationBuilder.DropTable(
                name: "SelfAssessmentForms");
        }
    }
}
