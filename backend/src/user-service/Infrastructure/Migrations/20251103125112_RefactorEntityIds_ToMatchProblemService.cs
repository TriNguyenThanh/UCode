using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorEntityIds_ToMatchProblemService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_admins_users_id",
                table: "admins");

            migrationBuilder.DropForeignKey(
                name: "fk_students_users_id",
                table: "students");

            migrationBuilder.DropForeignKey(
                name: "fk_teachers_users_id",
                table: "teachers");

            migrationBuilder.DropIndex(
                name: "ix_teachers_employee_id",
                table: "teachers");

            migrationBuilder.DropIndex(
                name: "ix_students_student_id",
                table: "students");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "users",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "employee_id",
                table: "teachers",
                newName: "teacher_code");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "teachers",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "student_id",
                table: "students",
                newName: "student_code");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "students",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "refresh_tokens",
                newName: "refresh_token_id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "classes",
                newName: "class_id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "admins",
                newName: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_teachers_teacher_code",
                table: "teachers",
                column: "teacher_code",
                unique: true,
                filter: "[teacher_code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_students_student_code",
                table: "students",
                column: "student_code",
                unique: true,
                filter: "[student_code] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "fk_admins_users_user_id",
                table: "admins",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_students_users_user_id",
                table: "students",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_teachers_users_user_id",
                table: "teachers",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_admins_users_user_id",
                table: "admins");

            migrationBuilder.DropForeignKey(
                name: "fk_students_users_user_id",
                table: "students");

            migrationBuilder.DropForeignKey(
                name: "fk_teachers_users_user_id",
                table: "teachers");

            migrationBuilder.DropIndex(
                name: "ix_teachers_teacher_code",
                table: "teachers");

            migrationBuilder.DropIndex(
                name: "ix_students_student_code",
                table: "students");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "teacher_code",
                table: "teachers",
                newName: "employee_id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "teachers",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "student_code",
                table: "students",
                newName: "student_id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "students",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "refresh_token_id",
                table: "refresh_tokens",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "class_id",
                table: "classes",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "admins",
                newName: "id");

            migrationBuilder.CreateIndex(
                name: "ix_teachers_employee_id",
                table: "teachers",
                column: "employee_id",
                unique: true,
                filter: "[employee_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_students_student_id",
                table: "students",
                column: "student_id",
                unique: true,
                filter: "[student_id] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "fk_admins_users_id",
                table: "admins",
                column: "id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_students_users_id",
                table: "students",
                column: "id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_teachers_users_id",
                table: "teachers",
                column: "id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
