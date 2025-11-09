using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assignment",
                columns: table => new
                {
                    assignment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    assignment_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    class_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    start_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    end_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    assigned_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    total_points = table.Column<int>(type: "int", nullable: true),
                    allow_late_submission = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "DRAFT")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assignment", x => x.assignment_id);
                });

            migrationBuilder.CreateTable(
                name: "language",
                columns: table => new
                {
                    language_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    display_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    default_time_factor = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 1.0m),
                    default_memory_kb = table.Column<int>(type: "int", nullable: true),
                    default_head = table.Column<string>(type: "NVARCHAR(MAX)", maxLength: 4000, nullable: true),
                    default_body = table.Column<string>(type: "NVARCHAR(MAX)", maxLength: 4000, nullable: true),
                    default_tail = table.Column<string>(type: "NVARCHAR(MAX)", maxLength: 4000, nullable: true),
                    is_enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_language", x => x.language_id);
                });

            migrationBuilder.CreateTable(
                name: "problem",
                columns: table => new
                {
                    problem_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    difficulty = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    owner_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    visibility = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    statement = table.Column<string>(type: "NVARCHAR(MAX)", maxLength: 4000, nullable: true),
                    solution = table.Column<string>(type: "NVARCHAR(MAX)", maxLength: 4000, nullable: true),
                    io_mode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false, defaultValue: "STDIO"),
                    input_format = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    output_format = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    constraints = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    max_score = table.Column<int>(type: "int", nullable: true),
                    time_limit_ms = table.Column<int>(type: "int", nullable: false, defaultValue: 1000),
                    memory_limit_kb = table.Column<int>(type: "int", nullable: false, defaultValue: 262144),
                    source_limit_kb = table.Column<int>(type: "int", nullable: false, defaultValue: 65536),
                    stack_limit_kb = table.Column<int>(type: "int", nullable: false, defaultValue: 8192),
                    validator_ref = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    changelog = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    is_locked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    description = table.Column<string>(type: "NVARCHAR(MAX)", maxLength: 4000, nullable: true),
                    sample_input = table.Column<string>(type: "NVARCHAR(MAX)", maxLength: 4000, nullable: true),
                    sample_output = table.Column<string>(type: "NVARCHAR(MAX)", maxLength: 4000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_problem", x => x.problem_id);
                });

            migrationBuilder.CreateTable(
                name: "tag",
                columns: table => new
                {
                    tag_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    category = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tag", x => x.tag_id);
                });

            migrationBuilder.CreateTable(
                name: "assignment_user",
                columns: table => new
                {
                    assignment_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    assignment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    assigned_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    tab_switch_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    captured_ai_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    started_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    score = table.Column<int>(type: "int", nullable: true),
                    max_score = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assignment_user", x => x.assignment_user_id);
                    table.ForeignKey(
                        name: "fk_assignment_user_assignment_assignment_id",
                        column: x => x.assignment_id,
                        principalTable: "assignment",
                        principalColumn: "assignment_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "assignment_problem",
                columns: table => new
                {
                    assignment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    problem_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    points = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    order_index = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assignment_problem", x => new { x.assignment_id, x.problem_id });
                    table.ForeignKey(
                        name: "fk_assignment_problem_assignment_assignment_id",
                        column: x => x.assignment_id,
                        principalTable: "assignment",
                        principalColumn: "assignment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_assignment_problem_problem_problem_id",
                        column: x => x.problem_id,
                        principalTable: "problem",
                        principalColumn: "problem_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "dataset",
                columns: table => new
                {
                    dataset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    problem_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    kind = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dataset", x => x.dataset_id);
                    table.ForeignKey(
                        name: "fk_dataset_problem_problem_id",
                        column: x => x.problem_id,
                        principalTable: "problem",
                        principalColumn: "problem_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "problem_asset",
                columns: table => new
                {
                    problem_asset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    problem_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    type = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    object_ref = table.Column<string>(type: "nvarchar(max)", maxLength: 4000, nullable: false),
                    checksum = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    title = table.Column<string>(type: "nvarchar(max)", maxLength: 4000, nullable: true),
                    format = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false, defaultValue: "MARKDOWN"),
                    order_index = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_problem_asset", x => x.problem_asset_id);
                    table.ForeignKey(
                        name: "fk_problem_asset_problem_problem_id",
                        column: x => x.problem_id,
                        principalTable: "problem",
                        principalColumn: "problem_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "problem_language",
                columns: table => new
                {
                    problem_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    language_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    time_factor_override = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    memory_kb_override = table.Column<int>(type: "int", nullable: true),
                    head_override = table.Column<string>(type: "NVARCHAR(MAX)", maxLength: 4000, nullable: true),
                    body_override = table.Column<string>(type: "NVARCHAR(MAX)", maxLength: 4000, nullable: true),
                    tail_override = table.Column<string>(type: "NVARCHAR(MAX)", maxLength: 4000, nullable: true),
                    is_allowed = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_problem_language", x => new { x.problem_id, x.language_id });
                    table.ForeignKey(
                        name: "fk_problem_language_language",
                        column: x => x.language_id,
                        principalTable: "language",
                        principalColumn: "language_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_problem_language_problem",
                        column: x => x.problem_id,
                        principalTable: "problem",
                        principalColumn: "problem_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "problem_tag",
                columns: table => new
                {
                    problem_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tag_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_problem_tag", x => new { x.problem_id, x.tag_id });
                    table.ForeignKey(
                        name: "fk_problem_tag_problem_problem_id",
                        column: x => x.problem_id,
                        principalTable: "problem",
                        principalColumn: "problem_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_problem_tag_tag_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tag",
                        principalColumn: "tag_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "submission",
                columns: table => new
                {
                    submission_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    assignment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    problem_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    dataset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    source_code = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    source_code_ref = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    language = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    compare_result = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    error_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    error_message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    total_testcase = table.Column<int>(type: "int", nullable: false),
                    passed_testcase = table.Column<int>(type: "int", nullable: false),
                    total_time = table.Column<long>(type: "bigint", nullable: false),
                    total_memory = table.Column<long>(type: "bigint", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    result_file_ref = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    assignment_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_submission", x => x.submission_id);
                    table.ForeignKey(
                        name: "fk_submission_assignment_assignment_id",
                        column: x => x.assignment_id,
                        principalTable: "assignment",
                        principalColumn: "assignment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_submission_assignment_user_assignment_user_id",
                        column: x => x.assignment_user_id,
                        principalTable: "assignment_user",
                        principalColumn: "assignment_user_id");
                    table.ForeignKey(
                        name: "fk_submission_problem_problem_id",
                        column: x => x.problem_id,
                        principalTable: "problem",
                        principalColumn: "problem_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "test_case",
                columns: table => new
                {
                    test_case_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    dataset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    index_no = table.Column<int>(type: "int", nullable: false),
                    input_ref = table.Column<string>(type: "NVARCHAR(MAX)", maxLength: 4000, nullable: false),
                    output_ref = table.Column<string>(type: "NVARCHAR(MAX)", maxLength: 4000, nullable: false),
                    score = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "100")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_test_case", x => x.test_case_id);
                    table.ForeignKey(
                        name: "fk_test_case_dataset_dataset_id",
                        column: x => x.dataset_id,
                        principalTable: "dataset",
                        principalColumn: "dataset_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_assignment_assigned_by",
                table: "assignment",
                column: "assigned_by");

            migrationBuilder.CreateIndex(
                name: "ix_assignment_class_id",
                table: "assignment",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "ix_assignment_created_at",
                table: "assignment",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_assignment_status",
                table: "assignment",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_assignment_problem_assignment_id",
                table: "assignment_problem",
                column: "assignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_assignment_problem_problem_id",
                table: "assignment_problem",
                column: "problem_id");

            migrationBuilder.CreateIndex(
                name: "ix_assignment_user_assignment_id",
                table: "assignment_user",
                column: "assignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_assignment_user_user_id",
                table: "assignment_user",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_datasets_problemid",
                table: "dataset",
                column: "problem_id");

            migrationBuilder.CreateIndex(
                name: "ix_language_code",
                table: "language",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_language_display_order",
                table: "language",
                column: "display_order");

            migrationBuilder.CreateIndex(
                name: "ix_problems_code",
                table: "problem",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_problems_difficulty",
                table: "problem",
                column: "difficulty");

            migrationBuilder.CreateIndex(
                name: "ix_problems_islocked",
                table: "problem",
                column: "is_locked");

            migrationBuilder.CreateIndex(
                name: "ix_problems_ownerid",
                table: "problem",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_problems_slug",
                table: "problem",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_problems_status_visibility",
                table: "problem",
                columns: new[] { "status", "visibility" });

            migrationBuilder.CreateIndex(
                name: "ix_problems_title",
                table: "problem",
                column: "title");

            migrationBuilder.CreateIndex(
                name: "ix_problems_updatedat",
                table: "problem",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "ix_problemassets_problemid",
                table: "problem_asset",
                column: "problem_id");

            migrationBuilder.CreateIndex(
                name: "ix_problemassets_problemid_type_orderindex",
                table: "problem_asset",
                columns: new[] { "problem_id", "type", "order_index" });

            migrationBuilder.CreateIndex(
                name: "ix_problem_language_language",
                table: "problem_language",
                column: "language_id");

            migrationBuilder.CreateIndex(
                name: "ix_problem_language_problem",
                table: "problem_language",
                column: "problem_id");

            migrationBuilder.CreateIndex(
                name: "ix_problem_tag_tag_id",
                table: "problem_tag",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_submission_assignment_id",
                table: "submission",
                column: "assignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_submission_assignment_user_id",
                table: "submission",
                column: "assignment_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_submission_dataset_id",
                table: "submission",
                column: "dataset_id");

            migrationBuilder.CreateIndex(
                name: "ix_submission_problem_id",
                table: "submission",
                column: "problem_id");

            migrationBuilder.CreateIndex(
                name: "ix_submission_status",
                table: "submission",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_submission_submitted_at",
                table: "submission",
                column: "submitted_at");

            migrationBuilder.CreateIndex(
                name: "ix_submission_user_id",
                table: "submission",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tag_category",
                table: "tag",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_tag_name",
                table: "tag",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_test_case_dataset_id_index_no",
                table: "test_case",
                columns: new[] { "dataset_id", "index_no" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assignment_problem");

            migrationBuilder.DropTable(
                name: "problem_asset");

            migrationBuilder.DropTable(
                name: "problem_language");

            migrationBuilder.DropTable(
                name: "problem_tag");

            migrationBuilder.DropTable(
                name: "submission");

            migrationBuilder.DropTable(
                name: "test_case");

            migrationBuilder.DropTable(
                name: "language");

            migrationBuilder.DropTable(
                name: "tag");

            migrationBuilder.DropTable(
                name: "assignment_user");

            migrationBuilder.DropTable(
                name: "dataset");

            migrationBuilder.DropTable(
                name: "assignment");

            migrationBuilder.DropTable(
                name: "problem");
        }
    }
}
