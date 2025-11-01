using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    AssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalPoints = table.Column<int>(type: "int", nullable: true),
                    AllowLateSubmission = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "DRAFT")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.AssignmentId);
                });

            migrationBuilder.CreateTable(
                name: "Problems",
                columns: table => new
                {
                    ProblemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Difficulty = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Visibility = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    StatementMdRef = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IoMode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false, defaultValue: "STDIO"),
                    InputFormat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OutputFormat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Constraints = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaxScore = table.Column<int>(type: "int", nullable: true),
                    TimeLimitMs = table.Column<int>(type: "int", nullable: false, defaultValue: 1000),
                    MemoryLimitKb = table.Column<int>(type: "int", nullable: false, defaultValue: 262144),
                    SourceLimitKb = table.Column<int>(type: "int", nullable: false, defaultValue: 65536),
                    StackLimitKb = table.Column<int>(type: "int", nullable: false, defaultValue: 8192),
                    ValidatorRef = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Changelog = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Description = table.Column<string>(type: "NVARCHAR(MAX)", maxLength: 4000, nullable: true),
                    SampleInput = table.Column<string>(type: "NVARCHAR(MAX)", maxLength: 4000, nullable: true),
                    SampleOutput = table.Column<string>(type: "NVARCHAR(MAX)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Problems", x => x.ProblemId);
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProblemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DatasetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceCodeRef = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Language = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CompareResult = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ErrorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TotalTestcase = table.Column<int>(type: "int", nullable: false),
                    PassedTestcase = table.Column<int>(type: "int", nullable: false),
                    TotalTime = table.Column<long>(type: "bigint", nullable: false),
                    TotalMemory = table.Column<long>(type: "bigint", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    ResultFileRef = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.SubmissionId);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    TagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.TagId);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentUsers",
                columns: table => new
                {
                    AssignmentUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: true),
                    MaxScore = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentUsers", x => x.AssignmentUserId);
                    table.ForeignKey(
                        name: "FK_AssignmentUsers_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "AssignmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentProblems",
                columns: table => new
                {
                    AssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProblemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentProblems", x => new { x.AssignmentId, x.ProblemId });
                    table.ForeignKey(
                        name: "FK_AssignmentProblems_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "AssignmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignmentProblems_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "ProblemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CodeTemplates",
                columns: table => new
                {
                    CodeTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProblemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Lang = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StarterRef = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeTemplates", x => x.CodeTemplateId);
                    table.ForeignKey(
                        name: "FK_CodeTemplates_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "ProblemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Datasets",
                columns: table => new
                {
                    DatasetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProblemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Datasets", x => x.DatasetId);
                    table.ForeignKey(
                        name: "FK_Datasets_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "ProblemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LanguageLimits",
                columns: table => new
                {
                    LanguageLimitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProblemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Lang = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TimeFactor = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    MemoryKbOverride = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanguageLimits", x => x.LanguageLimitId);
                    table.ForeignKey(
                        name: "FK_LanguageLimits_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "ProblemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProblemAssets",
                columns: table => new
                {
                    ProblemAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProblemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ObjectRef = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Checksum = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProblemAssets", x => x.ProblemAssetId);
                    table.ForeignKey(
                        name: "FK_ProblemAssets_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "ProblemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProblemTags",
                columns: table => new
                {
                    ProblemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProblemTags", x => new { x.ProblemId, x.TagId });
                    table.ForeignKey(
                        name: "FK_ProblemTags_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "ProblemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProblemTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "TagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BestSubmissions",
                columns: table => new
                {
                    BestSubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProblemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    MaxScore = table.Column<int>(type: "int", nullable: false),
                    TotalTime = table.Column<long>(type: "bigint", nullable: false),
                    TotalMemory = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BestSubmissions", x => x.BestSubmissionId);
                    table.ForeignKey(
                        name: "FK_BestSubmissions_AssignmentUsers_AssignmentUserId",
                        column: x => x.AssignmentUserId,
                        principalTable: "AssignmentUsers",
                        principalColumn: "AssignmentUserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BestSubmissions_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "ProblemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BestSubmissions_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "SubmissionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TestCases",
                columns: table => new
                {
                    TestCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DatasetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IndexNo = table.Column<int>(type: "int", nullable: false),
                    InputRef = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OutputRef = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Score = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "100")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCases", x => x.TestCaseId);
                    table.ForeignKey(
                        name: "FK_TestCases_Datasets_DatasetId",
                        column: x => x.DatasetId,
                        principalTable: "Datasets",
                        principalColumn: "DatasetId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentProblems_AssignmentId",
                table: "AssignmentProblems",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentProblems_ProblemId",
                table: "AssignmentProblems",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_AssignedBy",
                table: "Assignments",
                column: "AssignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_ClassId",
                table: "Assignments",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_CreatedAt",
                table: "Assignments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_Status",
                table: "Assignments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentUsers_AssignmentId",
                table: "AssignmentUsers",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentUsers_UserId",
                table: "AssignmentUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BestSubmissions_AssignmentUserId",
                table: "BestSubmissions",
                column: "AssignmentUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BestSubmissions_AssignmentUserId_ProblemId",
                table: "BestSubmissions",
                columns: new[] { "AssignmentUserId", "ProblemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BestSubmissions_ProblemId",
                table: "BestSubmissions",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_BestSubmissions_SubmissionId",
                table: "BestSubmissions",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "ix_codetemplates_lang",
                table: "CodeTemplates",
                column: "Lang");

            migrationBuilder.CreateIndex(
                name: "ix_codetemplates_problemid",
                table: "CodeTemplates",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "ix_datasets_problemid",
                table: "Datasets",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "ix_languagelimits_lang",
                table: "LanguageLimits",
                column: "Lang");

            migrationBuilder.CreateIndex(
                name: "ix_languagelimits_problemid",
                table: "LanguageLimits",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "ix_languagelimits_problemid_lang",
                table: "LanguageLimits",
                columns: new[] { "ProblemId", "Lang" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_problemassets_problemid",
                table: "ProblemAssets",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "ix_problemassets_type",
                table: "ProblemAssets",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "ix_problems_code",
                table: "Problems",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_problems_difficulty",
                table: "Problems",
                column: "Difficulty");

            migrationBuilder.CreateIndex(
                name: "ix_problems_islocked",
                table: "Problems",
                column: "IsLocked");

            migrationBuilder.CreateIndex(
                name: "ix_problems_ownerid",
                table: "Problems",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "ix_problems_slug",
                table: "Problems",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_problems_status_visibility",
                table: "Problems",
                columns: new[] { "Status", "Visibility" });

            migrationBuilder.CreateIndex(
                name: "ix_problems_title",
                table: "Problems",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "ix_problems_updatedat",
                table: "Problems",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProblemTags_TagId",
                table: "ProblemTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_AssignmentId",
                table: "Submissions",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_DatasetId",
                table: "Submissions",
                column: "DatasetId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ProblemId",
                table: "Submissions",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_Status",
                table: "Submissions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_SubmittedAt",
                table: "Submissions",
                column: "SubmittedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_UserId",
                table: "Submissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Category",
                table: "Tags",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestCases_DatasetId_IndexNo",
                table: "TestCases",
                columns: new[] { "DatasetId", "IndexNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignmentProblems");

            migrationBuilder.DropTable(
                name: "BestSubmissions");

            migrationBuilder.DropTable(
                name: "CodeTemplates");

            migrationBuilder.DropTable(
                name: "LanguageLimits");

            migrationBuilder.DropTable(
                name: "ProblemAssets");

            migrationBuilder.DropTable(
                name: "ProblemTags");

            migrationBuilder.DropTable(
                name: "TestCases");

            migrationBuilder.DropTable(
                name: "AssignmentUsers");

            migrationBuilder.DropTable(
                name: "Submissions");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Datasets");

            migrationBuilder.DropTable(
                name: "Assignments");

            migrationBuilder.DropTable(
                name: "Problems");
        }
    }
}
