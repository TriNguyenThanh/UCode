using System;
using System.Collections.Generic;

namespace UCode.Desktop.Models
{
    public class Assignment
    {
        public string AssignmentId { get; set; } = string.Empty;
        public AssignmentType AssignmentType { get; set; }
        public AssignmentStatus Status { get; set; }
        public string ClassId { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string AssignedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
        public int TotalPoints { get; set; }
        public int? TotalProblems { get; set; }
        public bool AllowLateSubmission { get; set; }
        public List<AssignmentProblemDetail> Problems { get; set; } = new();
        public AssignmentStatistics? Statistics { get; set; }
    }

    public class AssignmentProblemDetail
    {
        public string ProblemId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public int Points { get; set; }
        public int OrderIndex { get; set; }
    }

    public class AssignmentStatistics
    {
        public int TotalStudents { get; set; }
        public int NotStarted { get; set; }
        public int InProgress { get; set; }
        public int Submitted { get; set; }
        public int Graded { get; set; }
        public double AverageScore { get; set; }
        public double CompletionRate { get; set; }
    }

    public class AssignmentUser
    {
        public string AssignmentUserId { get; set; } = string.Empty;
        public string AssignmentId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public AssignmentUserStatus Status { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public double? Score { get; set; }
        public double? MaxScore { get; set; }
        public UserInfo User { get; set; }
    }

    public class UserInfo
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class BestSubmission
    {
        public string SubmissionId { get; set; } = string.Empty;
        public string AssignmentUserId { get; set; } = string.Empty;
        public string ProblemId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public double? Score { get; set; }
        public double MaxScore { get; set; }
        public string SolutionCode { get; set; } = string.Empty;
        public string TeacherFeedback { get; set; } = string.Empty;
        public int AttemptCount { get; set; }
        public int? TotalTestCases { get; set; }
        public int? PassedTestCases { get; set; }
        public int? ExecutionTime { get; set; }
        public int? MemoryUsed { get; set; }
    }

    public enum AssignmentType
    {
        HOMEWORK,
        EXAMINATION,
        PRACTICE
    }

    public enum AssignmentStatus
    {
        DRAFT,
        PUBLISHED,
        CLOSED
    }

    public enum AssignmentUserStatus
    {
        NOT_STARTED,
        IN_PROGRESS,
        SUBMITTED,
        GRADED
    }

    public class CreateAssignmentRequest
    {
        public string ClassId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AssignmentType { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; }
        public bool AllowLateSubmission { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<AssignmentProblem> Problems { get; set; } = new();
    }

    public class AssignmentProblem
    {
        public string ProblemId { get; set; } = string.Empty;
        public int Points { get; set; }
        public int OrderIndex { get; set; }
    }
}

