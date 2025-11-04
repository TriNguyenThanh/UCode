namespace AssignmentService.Domain.Entities;

/// <summary>
/// Entity đại diện cho bảng ProblemTags
/// Bảng trung gian (Junction Table) cho Many-to-Many relationship
/// giữa Problems và Tags
/// Composite Primary Key: (ProblemId, TagId)
/// </summary>
public class ProblemTag
{
    /// <summary>
    /// Foreign Key đến Problem
    /// Là phần của Composite Primary Key
    /// </summary>
    public Guid ProblemId { get; set; }
    
    /// <summary>
    /// Foreign Key đến Tag
    /// Là phần của Composite Primary Key
    /// </summary>
    public Guid TagId { get; set; }
    
    /* ===== NAVIGATION PROPERTIES ===== */
    
    /// <summary>
    /// Reference về Problem
    /// </summary>
    public Problem Problem { get; set; } = null!;
    
    /// <summary>
    /// Reference về Tag
    /// </summary>
    public Tag Tag { get; set; } = null!;
}