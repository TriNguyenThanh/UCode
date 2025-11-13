using System;

namespace UCode.Desktop.Models
{
    public class Submission
    {
        public string SubmissionId { get; set; } = string.Empty;
        public string ProblemId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string SolutionCode { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public double? Score { get; set; }
        public int? ExecutionTime { get; set; }
        public int? MemoryUsed { get; set; }
        public int? TotalTestCases { get; set; }
        public int? PassedTestCases { get; set; }
    }
}

