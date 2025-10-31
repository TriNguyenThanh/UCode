using AssignmentService.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace AssignmentService.Domain.Entities;

public class Result
{
    [Key]
    [MaxLength(36)]
    [Column("testcase_id")]
    public Guid TestcaseId { get; set; }

    [Key]
    [MaxLength(36)]
    [Column("submission_id")]
    public Guid SubmissionId { get; set; }

    [Column("actual_output")]
    [AllowNull]
    public string ActualOutput { get; set; } = string.Empty;

    [Column("status")]
    public TestcaseStatus Status { get; set; } = TestcaseStatus.Pending;

    [Column("error_message")]
    [AllowNull]
    public string ErrorMessage { get; set; } = string.Empty;

    [Column("execution_time_ms")]
    public int ExecutionTimeMs { get; set; } = 0;

    [Column("memory_usage_kb")]
    public int MemoryUsageKb { get; set; } = 0;
}