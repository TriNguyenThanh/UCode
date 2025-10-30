namespace ProblemService.Domain.Enums;

/// <summary>
/// Loại assignment
/// </summary>
public enum AssignmentTargetType
{
    /// <summary>
    /// Giao cho cả lớp
    /// </summary>
    CLASS,

    /// <summary>
    /// Giao cho student cụ thể (remedial work)
    /// </summary>
    STUDENT
}