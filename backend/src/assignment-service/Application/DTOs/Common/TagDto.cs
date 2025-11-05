using AssignmentService.Domain.Enums;

namespace AssignmentService.Application.DTOs.Common;

/// <summary>
/// DTO for Tag entity
/// </summary>
public class TagDto
{
    /// <summary>
    /// Unique identifier for the tag
    /// </summary>
    public Guid TagId { get; set; }
    
    /// <summary>
    /// Tag name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Tag category (e.g., Algorithm, DataStructure, Difficulty)
    /// </summary>
    public TagCategory Category { get; set; }
    
    /// <summary>
    /// Display color for the tag (optional, for UI)
    /// </summary>
    public string? Color { get; set; }
    
    /// <summary>
    /// Number of problems using this tag (optional, for statistics)
    /// </summary>
    public int? ProblemCount { get; set; }
}