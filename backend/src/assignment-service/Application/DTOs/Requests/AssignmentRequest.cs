using AssignmentService.Domain.Enums;
using AssignmentService.Application.DTOs.Common;
using System.ComponentModel.DataAnnotations;
using AssignmentService.Application.Validators;

namespace AssignmentService.Application.DTOs.Requests;

public class AssignmentRequest
{
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
    
    [DateRange(nameof(StartTime), ErrorMessage = "EndTime must be after StartTime")]
    public DateTime? EndTime { get; set; }
    
    public bool AllowLateSubmission { get; set; } = false;
    
    public AssignmentStatus? Status { get; set; } = AssignmentStatus.DRAFT;
    /// <summary>
    /// Danh sách problems với điểm số và thứ tự
    /// VD: [
    ///   { "problemId": "guid-1", "points": 100, "orderIndex": 0 },
    ///   { "problemId": "guid-2", "points": 150, "orderIndex": 1 }
    /// ]
    /// </summary>
    [Required]
    public List<AssignmentProblemDto> Problems { get; set; } = new();
}
