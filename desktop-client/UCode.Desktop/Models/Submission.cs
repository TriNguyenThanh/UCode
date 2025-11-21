using System;

namespace UCode.Desktop.Models
{
    public class Submission
    {
        public string SubmissionId { get; set; } = string.Empty;
        public string ProblemId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string SourceCode { get; set; } = string.Empty;
        public string CompareResult { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string ResultFileRef { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public double Score { get; set; }
        public int TotalTime { get; set; }
        public int TotalMemory { get; set; }
        public int TotalTestcase { get; set; }
        public int PassedTestcase { get; set; }
        
        // Display property
        public string StatusDisplay
        {
            get => Status switch
            {
                "Passed" => "Đạt",
                "Failed" => "Không đạt",
                "CompilationError" => "Lỗi biên dịch",
                "RuntimeError" => "Lỗi runtime",
                "TimeLimitExceeded" => "Quá thời gian",
                "MemoryLimitExceeded" => "Quá bộ nhớ",
                _ => Status
            };
        }
    }
}

