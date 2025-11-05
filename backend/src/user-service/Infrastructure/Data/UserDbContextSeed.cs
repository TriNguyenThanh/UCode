using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Infrastructure.Data;

public static class UserDbContextSeed
{
    public static async Task SeedAsync(UserDbContext context)
    {
        // Check if database is already seeded
        if (await context.Admins.AnyAsync())
            return;

        // Seed default admin account
        var admin = new Admin
        {
            UserId = Guid.NewGuid(),
            Username = "admin",
            Email = "admin@ucode.io.vn",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
            FullName = "System Administrator",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await context.Admins.AddAsync(admin);

        // Seed sample teacher
        var teacher1 = new Teacher
        {
            UserId = Guid.NewGuid(),
            TeacherCode = "GV001",
            Username = "teacher01",
            Email = "teacher01@ucode.io.vn",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
            FullName = "Nguyễn Văn Giáo",
            Department = "Khoa Công Nghệ Thông Tin",
            Title = "Giảng viên",
            Phone = "0901234567",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var teacher2 = new Teacher
        {
            UserId = Guid.NewGuid(),
            TeacherCode = "GV002",
            Username = "teacher02",
            Email = "teacher02@ucode.io.vn",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
            FullName = "Trần Thị Hương",
            Department = "Khoa Công Nghệ Thông Tin",
            Title = "Giảng viên chính",
            Phone = "0907654321",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await context.Teachers.AddRangeAsync(teacher1, teacher2);

        // Seed sample students
        var student1 = new Student
        {
            UserId = Guid.NewGuid(),
            StudentCode = "SV001",
            Username = "student01",
            Email = "student01@ucode.io.vn",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
            FullName = "Lê Văn An",
            Major = "Công nghệ phần mềm",
            EnrollmentYear = 2023,
            ClassYear = 2,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var student2 = new Student
        {
            UserId = Guid.NewGuid(),
            StudentCode = "SV002",
            Username = "student02",
            Email = "student02@ucode.io.vn",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
            FullName = "Phạm Thị Bình",
            Major = "Công nghệ phần mềm",
            EnrollmentYear = 2023,
            ClassYear = 2,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var student3 = new Student
        {
            UserId = Guid.NewGuid(),
            StudentCode = "SV003",
            Username = "student03",
            Email = "student03@ucode.io.vn",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
            FullName = "Hoàng Văn Cường",
            Major = "Khoa học máy tính",
            EnrollmentYear = 2023,
            ClassYear = 2,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await context.Students.AddRangeAsync(student1, student2, student3);

        // Seed sample classes
        var class1 = new Class
        {
            ClassId = Guid.NewGuid(),
            Name = "Lập trình C# nâng cao",
            Description = "Khóa học về lập trình C# nâng cao với ASP.NET Core",
            TeacherId = teacher1.UserId,
            ClassCode = "CLS2401",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var class2 = new Class
        {
            ClassId = Guid.NewGuid(),
            Name = "Cấu trúc dữ liệu và giải thuật",
            Description = "Khóa học về cấu trúc dữ liệu và giải thuật cơ bản",
            TeacherId = teacher2.UserId,
            ClassCode = "CLS2402",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Classes.AddRangeAsync(class1, class2);

        // Seed student-class relationships
        var userClass1 = new UserClass
        {
            StudentId = student1.UserId,
            ClassId = class1.ClassId,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        var userClass2 = new UserClass
        {
            StudentId = student2.UserId,
            ClassId = class1.ClassId,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        var userClass3 = new UserClass
        {
            StudentId = student3.UserId,
            ClassId = class2.ClassId,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        await context.UserClasses.AddRangeAsync(userClass1, userClass2, userClass3);

        await context.SaveChangesAsync();
    }
}

