using System;
using UCode.Desktop.Models.Enums;

namespace UCode.Desktop.Models.Admin
{
    public class ProblemManagement
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Difficulty Difficulty { get; set; }
        public Guid CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public AdminProblemStatus Status { get; set; }
        public ProblemVisibility Visibility { get; set; }
        
        // Statistics
        public int TotalSubmissions { get; set; }
        public int SuccessfulSubmissions { get; set; }
        public decimal SuccessRate { get; set; }
        public int UsedInAssignments { get; set; }
        public int TotalDatasets { get; set; }
        public int TotalLanguages { get; set; }
        public string Tags { get; set; } = string.Empty;

        // Display properties
        public string DifficultyText => Difficulty.ToString();
        public string StatusText => Status.ToString();
        public string VisibilityText => Visibility.ToString();
        public string SuccessRateText => $"{SuccessRate:F1}%";
        public string SubmissionsText => $"{TotalSubmissions} submissions";
    }

    public enum AdminProblemStatus
    {
        Draft,
        Pending,
        Approved,
        Rejected,
        Archived
    }

    public enum ProblemVisibility
    {
        Private,
        Public,
        ClassOnly
    }

    public class ApproveProblemRequest
    {
        public Guid ProblemId { get; set; }
        public bool IsApproved { get; set; }
        public string? ReviewNotes { get; set; }
    }

    public class BulkProblemActionRequest
    {
        public Guid[] ProblemIds { get; set; } = Array.Empty<Guid>();
        public ProblemBulkActionType ActionType { get; set; }
        public ProblemVisibility? Visibility { get; set; }
        public ProblemStatus? Status { get; set; }
    }

    public enum ProblemBulkActionType
    {
        ChangeVisibility,
        ChangeStatus,
        Delete,
        Archive
    }
}
