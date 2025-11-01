using AssignmentService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AssignmentService.Application.DTOs.Common;

public class AssignmentDto
{
    public Guid? AssignmentId { get; set; }
    
    [Required]
    public AssignmentType AssignmentType { get; set; }
    
    [Required]
    public Guid ClassId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(2000)]
    public string? Description { get; set; }
    
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    
    public int? TotalPoints { get; set; }
    public bool AllowLateSubmission { get; set; } = false;
    public AssignmentStatus Status { get; set; } = AssignmentStatus.DRAFT;
    
    /// <summary>
    /// Danh sách problems trong assignment
    /// </summary>
    public List<AssignmentProblemDto>? Problems { get; set; }
}
