using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveAssignmentUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "assignment_user",
                type: "boolean",
                nullable: false,
                defaultValue: true);  // ← Changed from false to true
            
            // Update existing records to IsActive = true
            migrationBuilder.Sql("UPDATE assignment_user SET is_active = true WHERE is_active = false;");

            migrationBuilder.CreateIndex(
                name: "idx_assignment_users_isactive",
                table: "assignment_user",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_assignment_users_isactive",
                table: "assignment_user");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "assignment_user");
        }
    }
}
