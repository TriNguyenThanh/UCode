namespace AssignmentService.Domain.Entities;

/// <summary>
/// Bảng trung gian Many-to-Many giữa Assignment và Problem
/// </summary>
public class AssignmentProblem
{
    /// <summary>
    /// FK to Assignment
    /// </summary>
    public Guid AssignmentId { get; set; }
    
    /// <summary>
    /// FK to Problem
    /// </summary>
    public Guid ProblemId { get; set; }
    
    /// <summary>
    /// Điểm tối đa cho problem này trong assignment
    /// </summary>
    public int Points { get; set; } = 100;
    
    /// <summary>
    /// Thứ tự hiển thị problem trong assignment (0, 1, 2, ...)
    /// </summary>
    public int OrderIndex { get; set; }
    
    // Navigation properties
    public Assignment Assignment { get; set; } = null!;
    public Problem Problem { get; set; } = null!;
}
