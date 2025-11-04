using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_language_limit_problem_problem_id",
                table: "language_limit");

            migrationBuilder.DropPrimaryKey(
                name: "pk_language_limit",
                table: "language_limit");

            migrationBuilder.DropIndex(
                name: "ix_languagelimits_lang",
                table: "language_limit");

            migrationBuilder.DropIndex(
                name: "ix_languagelimits_problemid_lang",
                table: "language_limit");

            migrationBuilder.DropColumn(
                name: "lang",
                table: "language_limit");

            migrationBuilder.DropColumn(
                name: "time_factor",
                table: "language_limit");

            migrationBuilder.RenameTable(
                name: "language_limit",
                newName: "problem_language");

            migrationBuilder.RenameColumn(
                name: "language_limit_id",
                table: "problem_language",
                newName: "problem_language_id");

            migrationBuilder.RenameColumn(
                name: "tail",
                table: "problem_language",
                newName: "tail_override");

            migrationBuilder.RenameColumn(
                name: "head",
                table: "problem_language",
                newName: "head_override");

            migrationBuilder.RenameColumn(
                name: "body",
                table: "problem_language",
                newName: "body_override");

            migrationBuilder.RenameIndex(
                name: "ix_languagelimits_problemid",
                table: "problem_language",
                newName: "ix_problem_language_problem");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "problem_language",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<bool>(
                name: "is_allowed",
                table: "problem_language",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "language_id",
                table: "problem_language",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "time_factor_override",
                table: "problem_language",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_problem_language",
                table: "problem_language",
                column: "problem_language_id");

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

            migrationBuilder.CreateIndex(
                name: "ix_problem_language_language",
                table: "problem_language",
                column: "language_id");

            migrationBuilder.CreateIndex(
                name: "ix_problem_language_problem_language",
                table: "problem_language",
                columns: new[] { "problem_id", "language_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_language_code",
                table: "language",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_language_display_order",
                table: "language",
                column: "display_order");

            migrationBuilder.AddForeignKey(
                name: "fk_problem_language_language",
                table: "problem_language",
                column: "language_id",
                principalTable: "language",
                principalColumn: "language_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_problem_language_problem",
                table: "problem_language",
                column: "problem_id",
                principalTable: "problem",
                principalColumn: "problem_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_problem_language_language",
                table: "problem_language");

            migrationBuilder.DropForeignKey(
                name: "fk_problem_language_problem",
                table: "problem_language");

            migrationBuilder.DropTable(
                name: "language");

            migrationBuilder.DropPrimaryKey(
                name: "pk_problem_language",
                table: "problem_language");

            migrationBuilder.DropIndex(
                name: "ix_problem_language_language",
                table: "problem_language");

            migrationBuilder.DropIndex(
                name: "ix_problem_language_problem_language",
                table: "problem_language");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "problem_language");

            migrationBuilder.DropColumn(
                name: "is_allowed",
                table: "problem_language");

            migrationBuilder.DropColumn(
                name: "language_id",
                table: "problem_language");

            migrationBuilder.DropColumn(
                name: "time_factor_override",
                table: "problem_language");

            migrationBuilder.RenameTable(
                name: "problem_language",
                newName: "language_limit");

            migrationBuilder.RenameColumn(
                name: "problem_language_id",
                table: "language_limit",
                newName: "language_limit_id");

            migrationBuilder.RenameColumn(
                name: "tail_override",
                table: "language_limit",
                newName: "tail");

            migrationBuilder.RenameColumn(
                name: "head_override",
                table: "language_limit",
                newName: "head");

            migrationBuilder.RenameColumn(
                name: "body_override",
                table: "language_limit",
                newName: "body");

            migrationBuilder.RenameIndex(
                name: "ix_problem_language_problem",
                table: "language_limit",
                newName: "ix_languagelimits_problemid");

            migrationBuilder.AddColumn<string>(
                name: "lang",
                table: "language_limit",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "time_factor",
                table: "language_limit",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_language_limit",
                table: "language_limit",
                column: "language_limit_id");

            migrationBuilder.CreateIndex(
                name: "ix_languagelimits_lang",
                table: "language_limit",
                column: "lang");

            migrationBuilder.CreateIndex(
                name: "ix_languagelimits_problemid_lang",
                table: "language_limit",
                columns: new[] { "problem_id", "lang" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_language_limit_problem_problem_id",
                table: "language_limit",
                column: "problem_id",
                principalTable: "problem",
                principalColumn: "problem_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
