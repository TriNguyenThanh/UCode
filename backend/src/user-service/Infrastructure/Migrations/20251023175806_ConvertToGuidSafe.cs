using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvertToGuidSafe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop all indexes first
            migrationBuilder.DropIndex(
                name: "ix_classes_teacher_id",
                table: "classes");

            migrationBuilder.DropIndex(
                name: "ix_user_classes_class_id",
                table: "user_classes");

            // Step 2: Drop all foreign key constraints
            migrationBuilder.DropForeignKey(
                name: "fk_teachers_users_id",
                table: "teachers");

            migrationBuilder.DropForeignKey(
                name: "fk_students_users_id",
                table: "students");

            migrationBuilder.DropForeignKey(
                name: "fk_admins_users_id",
                table: "admins");

            migrationBuilder.DropForeignKey(
                name: "fk_user_classes_students_student_id",
                table: "user_classes");

            migrationBuilder.DropForeignKey(
                name: "fk_user_classes_classes_class_id",
                table: "user_classes");

            migrationBuilder.DropForeignKey(
                name: "fk_classes_teachers_teacher_id",
                table: "classes");

            // Step 2: Drop primary key constraints
            migrationBuilder.DropPrimaryKey(
                name: "pk_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_teachers",
                table: "teachers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_students",
                table: "students");

            migrationBuilder.DropPrimaryKey(
                name: "pk_admins",
                table: "admins");

            migrationBuilder.DropPrimaryKey(
                name: "pk_classes",
                table: "classes");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_classes",
                table: "user_classes");

            // Step 3: Add temporary GUID columns
            migrationBuilder.AddColumn<Guid>(
                name: "id_new",
                table: "users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "id_new",
                table: "teachers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "id_new",
                table: "students",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "id_new",
                table: "admins",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "id_new",
                table: "classes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "teacher_id_new",
                table: "classes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "student_id_new",
                table: "user_classes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "class_id_new",
                table: "user_classes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Step 4: Generate new GUIDs for existing records
            migrationBuilder.Sql(@"
                UPDATE users SET id_new = NEWID();
                UPDATE teachers SET id_new = NEWID();
                UPDATE students SET id_new = NEWID();
                UPDATE admins SET id_new = NEWID();
                UPDATE classes SET id_new = NEWID();
            ");

            // Step 5: Update foreign key references
            migrationBuilder.Sql(@"
                UPDATE classes SET teacher_id_new = t.id_new
                FROM classes c
                INNER JOIN teachers t ON c.teacher_id = t.id;
            ");

            migrationBuilder.Sql(@"
                UPDATE user_classes SET student_id_new = s.id_new
                FROM user_classes uc
                INNER JOIN students s ON uc.student_id = s.id;
            ");

            migrationBuilder.Sql(@"
                UPDATE user_classes SET class_id_new = c.id_new
                FROM user_classes uc
                INNER JOIN classes c ON uc.class_id = c.id;
            ");

            // Step 6: Drop old columns
            migrationBuilder.DropColumn(
                name: "id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "id",
                table: "teachers");

            migrationBuilder.DropColumn(
                name: "id",
                table: "students");

            migrationBuilder.DropColumn(
                name: "id",
                table: "admins");

            migrationBuilder.DropColumn(
                name: "id",
                table: "classes");

            migrationBuilder.DropColumn(
                name: "teacher_id",
                table: "classes");

            migrationBuilder.DropColumn(
                name: "student_id",
                table: "user_classes");

            migrationBuilder.DropColumn(
                name: "class_id",
                table: "user_classes");

            // Step 7: Rename new columns to original names
            migrationBuilder.RenameColumn(
                name: "id_new",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "id_new",
                table: "teachers",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "id_new",
                table: "students",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "id_new",
                table: "admins",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "id_new",
                table: "classes",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "teacher_id_new",
                table: "classes",
                newName: "teacher_id");

            migrationBuilder.RenameColumn(
                name: "student_id_new",
                table: "user_classes",
                newName: "student_id");

            migrationBuilder.RenameColumn(
                name: "class_id_new",
                table: "user_classes",
                newName: "class_id");

            // Step 8: Recreate primary key constraints
            migrationBuilder.AddPrimaryKey(
                name: "pk_users",
                table: "users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_teachers",
                table: "teachers",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_students",
                table: "students",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_admins",
                table: "admins",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_classes",
                table: "classes",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_classes",
                table: "user_classes",
                columns: new[] { "student_id", "class_id" });

            // Step 9: Recreate foreign key constraints
            migrationBuilder.AddForeignKey(
                name: "fk_teachers_users_id",
                table: "teachers",
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
                name: "fk_admins_users_id",
                table: "admins",
                column: "id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_classes_students_student_id",
                table: "user_classes",
                column: "student_id",
                principalTable: "students",
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
                name: "fk_classes_teachers_teacher_id",
                table: "classes",
                column: "teacher_id",
                principalTable: "teachers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // Step 10: Recreate indexes
            migrationBuilder.CreateIndex(
                name: "ix_classes_teacher_id",
                table: "classes",
                column: "teacher_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_classes_class_id",
                table: "user_classes",
                column: "class_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This migration is not reversible due to data loss
            throw new NotSupportedException("This migration cannot be reversed due to data conversion from GUID to string.");
        }
    }
}