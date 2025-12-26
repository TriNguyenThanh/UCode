using System;
using System.Linq;

namespace UCode.Desktop.Models
{
    public class Submission
    {
        private string _compareResult = string.Empty;

        public string SubmissionId { get; set; } = string.Empty;
        public string ProblemId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string SourceCode { get; set; } = string.Empty;
        
        public string CompareResult
        {
            get => _compareResult;
            set
            {
                _compareResult = value;
                ParseTestCaseResults();
            }
        }
        
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
        public bool IsSubmitLate { get; set; }
        public System.Collections.Generic.List<TestCaseResult> TestCaseResults { get; set; } = new();

        private void ParseTestCaseResults()
        {
            TestCaseResults.Clear();
            
            if (string.IsNullOrEmpty(_compareResult))
                return;

            for (int i = 0; i < _compareResult.Length; i++)
            {
                int statusCode = int.Parse(_compareResult[i].ToString());
                TestCaseResults.Add(new TestCaseResult
                {
                    Number = i + 1,
                    StatusCode = statusCode,
                    StatusText = GetStatusText(statusCode)
                });
            }
        }

        private static string GetStatusText(int statusCode)
        {
            return statusCode switch
            {
                0 => "Passed",
                1 => "Time Limit Exceeded",
                2 => "Memory Limit Exceeded",
                3 => "Runtime Error",
                4 => "Internal Error",
                5 => "Wrong Answer",
                6 => "Compilation Error",
                7 => "Skipped",
                _ => "Unknown"
            };
        }
        
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


    public class TestCaseResult
    {
        public int Number { get; set; }
        public int StatusCode { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public int ExecutionTime { get; set; }
        public int MemoryUsed { get; set; }
    }
