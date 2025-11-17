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
        public string UserId { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string ProblemId { get; set; } = string.Empty;
        public string AssignmentUserId { get; set; } = string.Empty;
        public string SourceCode { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CompareResult { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public int TotalTestcase { get; set; }
        public int PassedTestcase { get; set; }
        public double Score { get; set; }
        public int TotalTime { get; set; }
        public int TotalMemory { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string ResultFileRef { get; set; } = string.Empty;
        public int TotalSubmission { get; set; }
        public string AssignmentId { get; set; } = string.Empty;

        // Calculated properties for display
        public string UserStudentCode => UserCode;
        
        public string StatusDisplay => Status switch
        {
            "Passed" => "Đạt",
            "Failed" => "Không đạt",
            "CompilationError" => "Lỗi biên dịch",
            "RuntimeError" => "Lỗi runtime",
            "TimeLimitExceeded" => "Quá thời gian",
            "MemoryLimitExceeded" => "Quá bộ nhớ",
            _ => Status
        };

        public string TestCaseDisplay => $"{PassedTestcase}/{TotalTestcase}";
        public string ExecutionTimeDisplay => $"{TotalTime}ms";
        public string MemoryUsedDisplay => $"{TotalMemory}KB";
        public string ScoreDisplay => $"{Score:F1}";
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

