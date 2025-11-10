using System;

namespace UCode.Desktop.Models
{
    public class Problem
    {
        public string ProblemId { get; set; }
        public string Code { get; set; }
        public string Slug { get; set; }
        public string Title { get; set; }
        public Difficulty Difficulty { get; set; }
        public string OwnerId { get; set; }
        public Visibility Visibility { get; set; }
        public ProblemStatus Status { get; set; }
        public int TimeLimitMs { get; set; }
        public int MemoryLimitKb { get; set; }
        public string Statement { get; set; }
        public string InputFormat { get; set; }
        public string OutputFormat { get; set; }
        public string Constraints { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public enum Difficulty
    {
        EASY,
        MEDIUM,
        HARD
    }

    public enum Visibility
    {
        PUBLIC,
        PRIVATE
    }

    public enum ProblemStatus
    {
        DRAFT,
        PUBLISHED,
        ARCHIVED
    }

    public class Submission
    {
        public string SubmissionId { get; set; }
        public string ProblemId { get; set; }
        public string UserId { get; set; }
        public string SourceCodeRef { get; set; }
        public string Language { get; set; }
        public SubmissionStatus Status { get; set; }
        public string CompareResult { get; set; }
        public string ErrorMessage { get; set; }
        public int TotalTime { get; set; }
        public int TotalMemory { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    public enum SubmissionStatus
    {
        Pending,
        Judging,
        Accepted,
        WrongAnswer,
        TimeLimitExceeded,
        MemoryLimitExceeded,
        RuntimeError,
        CompilationError,
        SystemError
    }
}
