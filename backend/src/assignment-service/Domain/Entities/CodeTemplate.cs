namespace AssignmentService.Domain.Entities;

/// <summary>
/// Entity đại diện cho bảng CodeTemplates
/// Chứa starter code cho các ngôn ngữ
/// </summary>
public class CodeTemplate
{
    public Guid CodeTemplateId { get; set; }
    
    /// <summary>
    /// Foreign Key đến Problem (thay vì ProblemVersion)
    /// </summary>
    public Guid ProblemId { get; set; }
    
    public string Lang { get; set; } = string.Empty;
    public string StarterRef { get; set; } = string.Empty;

    // Navigation Properties
    public Problem Problem { get; set; } = null!;
}