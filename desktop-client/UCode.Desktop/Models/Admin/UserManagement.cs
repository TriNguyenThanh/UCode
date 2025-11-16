using System;
using UCode.Desktop.Models.Enums;

namespace UCode.Desktop.Models.Admin
{
    public class UserManagement
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        
        // Additional info based on role
        public string? StudentCode { get; set; } // For students
        public string? Department { get; set; } // For teachers
        
        // Statistics
        public int TotalClasses { get; set; }
        public int TotalSubmissions { get; set; }
        public int TotalAssignments { get; set; }

        // Display properties
        public string StatusText => IsActive ? "Active" : "Inactive";
        public string RoleText => Role.ToString();
        public string LastLoginText => LastLoginAt?.ToString("dd/MM/yyyy HH:mm") ?? "Never";
    }

    public class CreateUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? StudentCode { get; set; }
        public string? Department { get; set; }
    }

    public class UpdateUserRequest
    {
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? StudentCode { get; set; }
        public string? Department { get; set; }
    }

    public class BulkUserActionRequest
    {
        public Guid[] UserIds { get; set; } = Array.Empty<Guid>();
        public BulkActionType ActionType { get; set; }
    }

    public enum BulkActionType
    {
        Activate,
        Deactivate,
        Delete
    }
}
