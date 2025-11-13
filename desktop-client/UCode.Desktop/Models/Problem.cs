using System;
using System.Collections.Generic;
using UCode.Desktop.Models.Enums;

namespace UCode.Desktop.Models
{
    public class Problem
    {
        public string ProblemId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public Difficulty Difficulty { get; set; } = Difficulty.EASY;
        public Visibility Visibility { get; set; } = Visibility.PRIVATE;
        public ProblemStatus Status { get; set; } = ProblemStatus.DRAFT;
        public string Statement { get; set; } = string.Empty;
        public string InputFormat { get; set; } = string.Empty;
        public string OutputFormat { get; set; } = string.Empty;
        public string Constraints { get; set; } = string.Empty;
        public string Solution { get; set; } = string.Empty;
        public int TimeLimitMs { get; set; } = 1000;
        public int MemoryLimitKb { get; set; } = 262144;
        public IoMode IoMode { get; set; } = IoMode.STDIO;
        public int SourceLimitKb { get; set; }
        public int StackLimitKb { get; set; }
        public string ValidatorRef { get; set; } = string.Empty;
        public string Changelog { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public List<string> TagNames { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class Tag
    {
        public string TagId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#007bff";
    }

    public class Language
    {
        public string LanguageId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public double DefaultTimeFactor { get; set; } = 1.0;
        public int DefaultMemoryKb { get; set; } = 262144;
        public string DefaultHead { get; set; } = string.Empty;
        public string DefaultBody { get; set; } = string.Empty;
        public string DefaultTail { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
    }

    public class ProblemLanguage
    {
        public string LanguageId { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;
        public string LanguageDisplayName { get; set; } = string.Empty;
        public double TimeFactor { get; set; } = 1.0;
        public int MemoryKb { get; set; } = 262144;
        public string Head { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Tail { get; set; } = string.Empty;
    }

    public class ProblemLanguageRequest
    {
        public string ProblemId { get; set; } = string.Empty;
        public string LanguageId { get; set; } = string.Empty;
        public bool IsAllowed { get; set; } = true;
        public double TimeFactor { get; set; } = 1.0;
        public int MemoryKb { get; set; } = 262144;
        public string Head { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Tail { get; set; } = string.Empty;
    }

    public class CreateProblemRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Code { get; set; }
        public Difficulty Difficulty { get; set; } = Difficulty.EASY;
        public Visibility Visibility { get; set; } = Visibility.PRIVATE;
    }

    public class UpdateProblemRequest
    {
        public string ProblemId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Code { get; set; }
        public Difficulty Difficulty { get; set; } = Difficulty.EASY;
        public Visibility Visibility { get; set; } = Visibility.PRIVATE;
        public ProblemStatus Status { get; set; } = ProblemStatus.DRAFT;
        public string Statement { get; set; }
        public string InputFormat { get; set; }
        public string OutputFormat { get; set; }
        public string Constraints { get; set; }
        public string Solution { get; set; }
        public int TimeLimitMs { get; set; } = 1000;
        public int MemoryLimitKb { get; set; } = 262144;
        public IoMode IoMode { get; set; } = IoMode.STDIO;
    }
}
