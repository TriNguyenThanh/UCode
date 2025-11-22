namespace UCode.Desktop.Models
{
    public class RunCodeRequest
    {
        public string ProblemId { get; set; } = string.Empty;
        public string LanguageId { get; set; } = string.Empty;
        public string SourceCode { get; set; } = string.Empty;
        public string? AssignmentId { get; set; }
    }

    public class SubmitCodeRequest
    {
        public string ProblemId { get; set; } = string.Empty;
        public string LanguageId { get; set; } = string.Empty;
        public string SourceCode { get; set; } = string.Empty;
        public string? AssignmentId { get; set; }
    }
}
