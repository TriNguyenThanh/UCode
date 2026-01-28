using System;
using System.Collections.Generic;

namespace UCode.Desktop.Models.Admin
{
    /// <summary>
    /// System statistics for admin dashboard
    /// </summary>
    public class SystemStatistics
    {
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalClasses { get; set; }
        public int TotalActiveClasses { get; set; }
        public int TotalArchivedClasses { get; set; }
        public int TotalAssignments { get; set; }
        public int TotalProblems { get; set; }
        public int TotalSubmissions { get; set; }
        public int TodayActiveUsers { get; set; }
        public int WeekActiveUsers { get; set; }
        public int MonthActiveUsers { get; set; }
    }

    /// <summary>
    /// Assignment system statistics response from /api/v1/assignments/statistics/system
    /// </summary>
    public class AssignmentSystemStatisticsResponse
    {
        public int TotalAssignments { get; set; }
        public int TotalProblems { get; set; }
        public int TotalSubmissions { get; set; }
        public int TotalUsers { get; set; } // Tổng số sinh viên được giao bài (AssignmentUsers)
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// User statistics for admin dashboard
    /// </summary>
    public class UserStatistics
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int BannedUsers { get; set; }
        public int Students { get; set; }
        public int Teachers { get; set; }
        public int Admins { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int NewUsersThisMonth { get; set; }

        // Backward compatibility properties
        public int StudentCount => Students;
        public int TeacherCount => Teachers;
        public int AdminCount => Admins;
    }

    /// <summary>
    /// Class statistics for admin dashboard
    /// </summary>
    public class ClassStatistics
    {
        public int TotalClasses { get; set; }
        public int ActiveClasses { get; set; }
        public int InactiveClasses { get; set; }
        public int ArchivedClasses { get; set; }
        public int TotalStudents { get; set; }
        public double AverageStudentsPerClass { get; set; }
        public int ClassesCreatedToday { get; set; }
        public int ClassesCreatedThisWeek { get; set; }
        public int ClassesCreatedThisMonth { get; set; }
    }

    /// <summary>
    /// Admin class response with detailed information
    /// </summary>
    public class AdminClassResponse
    {
        public string ClassId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string TeacherId { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string TeacherEmail { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public int ActiveStudentCount { get; set; }
        public int AssignmentCount { get; set; }
        public int SubmissionCount { get; set; }
        public bool IsActive { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ArchivedAt { get; set; }
    }

    /// <summary>
    /// User detail for admin view
    /// </summary>
    public class AdminUserResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? StudentCode { get; set; }
        public string? TeacherCode { get; set; }
        public int ClassCount { get; set; }
        public int EnrolledClassCount { get; set; }
    }

    /// <summary>
    /// User detail for admin view (extended)
    /// </summary>
    public class UserDetailAdmin
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsBanned { get; set; }
        public string? BanReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? StudentCode { get; set; }
        public string? Major { get; set; }
        public int? ClassYear { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }

        // Statistics
        public int ClassesEnrolled { get; set; }
        public int ClassesTeaching { get; set; }
        public int SubmissionsCount { get; set; }
        public int ProblemsCreated { get; set; }
        public int AcceptedSubmissions { get; set; }
    }

    /// <summary>
    /// Class detail for admin view
    /// </summary>
    public class ClassDetailAdmin
    {
        public Guid ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CoverImage { get; set; }
        public string Semester { get; set; } = string.Empty;
        public string TeacherId { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string? TeacherEmail { get; set; }
        public int StudentCount { get; set; }
        public int AssignmentCount { get; set; }
        public bool IsActive { get; set; }
        public bool IsArchived { get; set; }
        public string? ArchiveReason { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Request for bulk actions on classes
    /// </summary>
    public class BulkActionRequest
    {
        public string Action { get; set; } = string.Empty; // "archive", "unarchive", "delete"
        public List<string> ClassIds { get; set; } = new();
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Request for bulk actions on users
    /// </summary>
    public class BulkUserActionRequest
    {
        public string Action { get; set; } = string.Empty; // "activate", "deactivate", "delete", "changeRole", "ban"
        public List<string> UserIds { get; set; } = new();
        public string? NewRole { get; set; }
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Result of bulk operations
    /// </summary>
    public class BulkActionResult
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> SuccessIds { get; set; } = new();
        public List<string> FailedIds { get; set; } = new();
    }

    /// <summary>
    /// Request to create user by admin
    /// </summary>
    public class CreateUserByAdminRequest
    {
        public string Username { get; set; } = string.Empty; // MSSV cho Student, Mã GV cho Teacher
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Student"; // Student, Teacher, Admin
        public bool IsActive { get; set; } = true;
        public string? StudentCode { get; set; }
        public string? TeacherCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Phone { get; set; } // Alias for PhoneNumber (some APIs use this)
    }

    /// <summary>
    /// Request to update user by admin
    /// </summary>
    public class UpdateUserByAdminRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
        public string? StudentCode { get; set; }
        public string? Major { get; set; }
        public int? ClassYear { get; set; }
        public string? AvatarUrl { get; set; }
    }

    /// <summary>
    /// Request to update class by admin
    /// </summary>
    public class UpdateClassByAdminRequest
    {
        public Guid ClassId { get; set; }
        public string? Name { get; set; }
        public string? ClassCode { get; set; }
        public string? Description { get; set; }
        public string? CoverImage { get; set; }
        public string? Subject { get; set; }
        public string? Semester { get; set; }
        public string? AcademicYear { get; set; }
        public string? TeacherId { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Request to archive class
    /// </summary>
    public class ArchiveClassRequest
    {
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Activity log entry
    /// </summary>
    public class ActivityLog
    {
        public string ActivityId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public DateTime Timestamp { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    /// <summary>
    /// System log entry
    /// </summary>
    public class SystemLog
    {
        public string LogId { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty; // Info, Warning, Error
        public string Service { get; set; } = string.Empty; // user-service, assignment-service, etc.
        public string Message { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public string? UserId { get; set; }
        public string? IpAddress { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Dashboard card data for admin home
    /// </summary>
    public class DashboardCard
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
    }

    /// <summary>
    /// User export request
    /// </summary>
    public class ExportUsersRequest
    {
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
        public string? SearchTerm { get; set; }
        public string Format { get; set; } = "xlsx"; // xlsx, csv, pdf
    }

    /// <summary>
    /// Class export request
    /// </summary>
    public class ExportClassesRequest
    {
        public string? TeacherId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsArchived { get; set; }
        public string? SearchTerm { get; set; }
        public string Format { get; set; } = "xlsx"; // xlsx, csv, pdf
    }

    /// <summary>
    /// System settings for admin
    /// </summary>
    public class SystemSettings
    {
        public GeneralSettings General { get; set; } = new();
        public SecuritySettings Security { get; set; } = new();
        public EmailSettings Email { get; set; } = new();
        public StorageSettings Storage { get; set; } = new();
    }

    public class GeneralSettings
    {
        public string SystemName { get; set; } = "UCode";
        public string SystemDescription { get; set; } = string.Empty;
        public string SupportEmail { get; set; } = string.Empty;
        public bool MaintenanceMode { get; set; } = false;
        public string? MaintenanceMessage { get; set; }
    }

    public class SecuritySettings
    {
        public bool RequireEmailVerification { get; set; } = true;
        public int SessionTimeout { get; set; } = 30; // minutes
        public int MaxLoginAttempts { get; set; } = 5;
        public int LockoutDuration { get; set; } = 15; // minutes
        public bool EnableTwoFactor { get; set; } = false;
    }

    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
    }

    public class StorageSettings
    {
        public long MaxFileSize { get; set; } = 10485760; // 10MB in bytes
        public List<string> AllowedExtensions { get; set; } = new();
        public string StorageProvider { get; set; } = "Local"; // Local, Azure, AWS
        public long TotalStorageQuota { get; set; } = 107374182400; // 100GB in bytes
    }
}
