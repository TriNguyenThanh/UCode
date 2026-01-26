using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditAteendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "require_face_check",
                table: "attendance_sessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "require_face_check",
                table: "attendance_sessions");
        }
    }
}
