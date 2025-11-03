using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserServiceModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_admins_users_id",
                table: "admins");

            migrationBuilder.DropForeignKey(
                name: "FK_classes_teachers_teacher_id",
                table: "classes");

            migrationBuilder.DropForeignKey(
                name: "FK_students_users_id",
                table: "students");

            migrationBuilder.DropForeignKey(
                name: "FK_teachers_users_id",
                table: "teachers");

            migrationBuilder.DropForeignKey(
                name: "FK_user_classes_classes_class_id",
                table: "user_classes");

            migrationBuilder.DropForeignKey(
                name: "FK_user_classes_students_student_id",
                table: "user_classes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_classes",
                table: "user_classes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_classes",
                table: "classes");

            migrationBuilder.RenameIndex(
                name: "IX_user_classes_class_id",
                table: "user_classes",
                newName: "ix_user_classes_class_id");

            migrationBuilder.RenameIndex(
                name: "IX_classes_teacher_id",
                table: "classes",
                newName: "ix_classes_teacher_id");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "user_classes",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "classes",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_classes",
                table: "user_classes",
                columns: new[] { "student_id", "class_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_classes",
                table: "classes",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "ix_classes_class_code",
                table: "classes",
                column: "class_code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_admins_users_id",
                table: "admins",
                column: "id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_classes_teachers_teacher_id",
                table: "classes",
                column: "teacher_id",
                principalTable: "teachers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

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

            migrationBuilder.AddForeignKey(
                name: "fk_user_classes_classes_class_id",
                table: "user_classes",
                column: "class_id",
                principalTable: "classes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_classes_students_student_id",
                table: "user_classes",
                column: "student_id",
                principalTable: "students",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_admins_users_id",
                table: "admins");

            migrationBuilder.DropForeignKey(
                name: "fk_classes_teachers_teacher_id",
                table: "classes");

            migrationBuilder.DropForeignKey(
                name: "fk_students_users_id",
                table: "students");

            migrationBuilder.DropForeignKey(
                name: "fk_teachers_users_id",
                table: "teachers");

            migrationBuilder.DropForeignKey(
                name: "fk_user_classes_classes_class_id",
                table: "user_classes");

            migrationBuilder.DropForeignKey(
                name: "fk_user_classes_students_student_id",
                table: "user_classes");

            migrationBuilder.DropIndex(
                name: "ix_users_email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_username",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_classes",
                table: "user_classes");

            migrationBuilder.DropIndex(
                name: "ix_teachers_employee_id",
                table: "teachers");

            migrationBuilder.DropIndex(
                name: "ix_students_student_id",
                table: "students");

            migrationBuilder.DropPrimaryKey(
                name: "pk_classes",
                table: "classes");

            migrationBuilder.DropIndex(
                name: "ix_classes_class_code",
                table: "classes");

            migrationBuilder.RenameIndex(
                name: "ix_user_classes_class_id",
                table: "user_classes",
                newName: "IX_user_classes_class_id");

            migrationBuilder.RenameIndex(
                name: "ix_classes_teacher_id",
                table: "classes",
                newName: "IX_classes_teacher_id");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "user_classes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "classes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_classes",
                table: "user_classes",
                columns: new[] { "student_id", "class_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_classes",
                table: "classes",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_admins_users_id",
                table: "admins",
                column: "id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_classes_teachers_teacher_id",
                table: "classes",
                column: "teacher_id",
                principalTable: "teachers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_students_users_id",
                table: "students",
                column: "id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_teachers_users_id",
                table: "teachers",
                column: "id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_classes_classes_class_id",
                table: "user_classes",
                column: "class_id",
                principalTable: "classes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_classes_students_student_id",
                table: "user_classes",
                column: "student_id",
                principalTable: "students",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
