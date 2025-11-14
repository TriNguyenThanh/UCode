using System;

namespace UCode.Desktop.Models
{
    public class User
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }

        // Student specific
        public string StudentCode { get; set; }
        public string Major { get; set; }
        public int? EnrollmentYear { get; set; }
        public int? ClassYear { get; set; }

        // Teacher specific
        public string TeacherCode { get; set; }
        public string Department { get; set; }
        public string Title { get; set; }

        // Common
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public enum UserRole
    {
        Student,
        Teacher,
        Admin
    }

    public enum UserStatus
    {
        Active,
        Inactive,
        Banned
    }
}
