using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConstrainAssignmentUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_submission_assignment_assignment_id",
                table: "submission");

            migrationBuilder.DropForeignKey(
                name: "fk_submission_dataset_dataset_id",
                table: "submission");

            migrationBuilder.DropForeignKey(
                name: "fk_submission_language_language_id",
                table: "submission");

            migrationBuilder.DropIndex(
                name: "ix_submission_status",
                table: "submission");

            migrationBuilder.DropIndex(
                name: "ix_submission_submitted_at",
                table: "submission");

            migrationBuilder.DropIndex(
                name: "ix_submission_user_id",
                table: "submission");

            migrationBuilder.AlterColumn<DateTime>(
                name: "submitted_at",
                table: "submission",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "SYSDATETIME()");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "submission",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "source_code_ref",
                table: "submission",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "error_message",
                table: "submission",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "error_code",
                table: "submission",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "compare_result",
                table: "submission",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_assignment_user_assignment_id_user_id",
                table: "assignment_user",
                columns: new[] { "assignment_id", "user_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_submission_assignment_assignment_id",
                table: "submission",
                column: "assignment_id",
                principalTable: "assignment",
                principalColumn: "assignment_id");

            migrationBuilder.AddForeignKey(
                name: "fk_submission_dataset_dataset_id",
                table: "submission",
                column: "dataset_id",
                principalTable: "dataset",
                principalColumn: "dataset_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_submission_language_language_id",
                table: "submission",
                column: "language_id",
                principalTable: "language",
                principalColumn: "language_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_submission_assignment_assignment_id",
                table: "submission");

            migrationBuilder.DropForeignKey(
                name: "fk_submission_dataset_dataset_id",
                table: "submission");

            migrationBuilder.DropForeignKey(
                name: "fk_submission_language_language_id",
                table: "submission");

            migrationBuilder.DropIndex(
                name: "ix_assignment_user_assignment_id_user_id",
                table: "assignment_user");

            migrationBuilder.AlterColumn<DateTime>(
                name: "submitted_at",
                table: "submission",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "submission",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "source_code_ref",
                table: "submission",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<string>(
                name: "error_message",
                table: "submission",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "error_code",
                table: "submission",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "compare_result",
                table: "submission",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "fk_submission_assignment_assignment_id",
                table: "submission",
                column: "assignment_id",
                principalTable: "assignment",
                principalColumn: "assignment_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_submission_dataset_dataset_id",
                table: "submission",
                column: "dataset_id",
                principalTable: "dataset",
                principalColumn: "dataset_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_submission_language_language_id",
                table: "submission",
                column: "language_id",
                principalTable: "language",
                principalColumn: "language_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
