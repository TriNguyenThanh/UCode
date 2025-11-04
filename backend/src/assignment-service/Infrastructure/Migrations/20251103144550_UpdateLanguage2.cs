using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLanguage2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_problem_language",
                table: "problem_language");

            migrationBuilder.DropIndex(
                name: "ix_problem_language_problem_language",
                table: "problem_language");

            migrationBuilder.DropColumn(
                name: "problem_language_id",
                table: "problem_language");

            migrationBuilder.AddPrimaryKey(
                name: "pk_problem_language",
                table: "problem_language",
                columns: new[] { "problem_id", "language_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_problem_language",
                table: "problem_language");

            migrationBuilder.AddColumn<Guid>(
                name: "problem_language_id",
                table: "problem_language",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "pk_problem_language",
                table: "problem_language",
                column: "problem_language_id");

            migrationBuilder.CreateIndex(
                name: "ix_problem_language_problem_language",
                table: "problem_language",
                columns: new[] { "problem_id", "language_id" },
                unique: true);
        }
    }
}
