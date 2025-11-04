using AssignmentService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AssignmentService.Application.DTOs.Requests;

/// <summary>
/// DTO for creating a new tag
/// </summary>
public class CreateTagRequest
{
    /// <summary>
    /// Tag name (must be unique)
    /// </summary>
    [Required(ErrorMessage = "Tag name is required")]
    [StringLength(64, ErrorMessage = "Tag name cannot exceed 64 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tag category (e.g., Algorithm, DataStructure, Difficulty)
    /// </summary>
    [Required(ErrorMessage = "Tag category is required")]
    public TagCategory Category { get; set; }
}


/// <summary>
/// DTO for updating an existing tag
/// </summary>
public class UpdateTagRequest
{
    /// <summary>
    /// Updated tag name
    /// </summary>
    [Required(ErrorMessage = "Tag name is required")]
    [StringLength(64, ErrorMessage = "Tag name cannot exceed 64 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Updated tag category
    /// </summary>
    [Required(ErrorMessage = "Tag category is required")]
    public TagCategory Category { get; set; }
}
