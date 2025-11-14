using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExamLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exam_activity_log",
                columns: table => new
                {
                    activity_log_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    assignment_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    activity_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    metadata = table.Column<string>(type: "text", maxLength: 4000, nullable: true),
                    suspicion_level = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exam_activity_log", x => x.activity_log_id);
                    table.ForeignKey(
                        name: "fk_exam_activity_log_assignment_user_assignment_user_id",
                        column: x => x.assignment_user_id,
                        principalTable: "assignment_user",
                        principalColumn: "assignment_user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_exam_activity_logs_assignment_user_id",
                table: "exam_activity_log",
                column: "assignment_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_exam_activity_logs_timestamp",
                table: "exam_activity_log",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "idx_exam_activity_logs_user_timestamp",
                table: "exam_activity_log",
                columns: new[] { "assignment_user_id", "timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exam_activity_log");
        }
    }
}
