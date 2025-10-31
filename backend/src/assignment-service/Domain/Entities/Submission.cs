using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssignmentService.Domain.Enums;

namespace AssignmentService.EF.Entities
{
    public class Submission
    {
        public Guid SubmissionId { get; set; }
        // public Guid UserId { get; set; }
        public Guid AssignmentStudentId { get; set; }
        public Guid ProblemId { get; set; }
        public string Code { get; set; } = string.Empty;
        public Language Language { get; set; } = Language.cpp;
        public bool IsSamplePassed { get; set; } = false;
        public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public int SampleTestcase { get; set; } = 0;
        public int TotalTestCase { get; set; } = 0;
        public int PassedTestCase { get; set; } = 0;
        public long TotalTime { get; set; } = 0;
        public long TotalMem { get; set; } = 0;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        // Navigation property for related Results
        public List<Result> Results { get; set; } = null!;
        public AssignmentProblemSubmission assignmentProblemSubmission { get; set;  } = null!;
    }
}