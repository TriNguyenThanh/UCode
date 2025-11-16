using System;
using System.Collections.Generic;

namespace UCode.Desktop.Models.Admin
{
    public class ClassManagement
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string TeacherEmail { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public int AssignmentCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }

        // Statistics
        public int TotalSubmissions { get; set; }
        public decimal AverageSuccessRate { get; set; }
        public DateTime? LastActivityAt { get; set; }

        // Display properties
        public string StatusText => IsActive ? "Active" : "Inactive";
        public string StudentsText => $"{StudentCount} students";
        public string AssignmentsText => $"{AssignmentCount} assignments";
    }

    public class CreateClassRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid TeacherId { get; set; }
    }

    public class UpdateClassRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid TeacherId { get; set; }
        public bool IsActive { get; set; }
    }

    public class ClassStatistics
    {
        public Guid ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int TotalAssignments { get; set; }
        public int CompletedAssignments { get; set; }
        public int TotalSubmissions { get; set; }
        public decimal AverageScore { get; set; }
        public decimal CompletionRate { get; set; }
        public List<StudentProgress> StudentProgresses { get; set; } = new();
    }

    public class StudentProgress
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int CompletedAssignments { get; set; }
        public int TotalAssignments { get; set; }
        public decimal AverageScore { get; set; }
        public int TotalSubmissions { get; set; }
    }
}
