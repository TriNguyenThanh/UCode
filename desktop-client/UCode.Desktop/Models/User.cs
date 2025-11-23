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

    /// <summary>
    /// Request model for updating user profile information
    /// </summary>
    public class UpdateUserRequest
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        
        // For Student
        public string Major { get; set; }
        public int? ClassYear { get; set; }
        
        // For Teacher
        public string Department { get; set; }
        public string Title { get; set; }
    }

    /// <summary>
    /// Request model for changing password
    /// </summary>
    public class ChangePasswordRequest
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Current password is required")]
        public string OldPassword { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "New password is required")]
        [System.ComponentModel.DataAnnotations.MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string NewPassword { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Confirm password is required")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Password and confirmation do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
