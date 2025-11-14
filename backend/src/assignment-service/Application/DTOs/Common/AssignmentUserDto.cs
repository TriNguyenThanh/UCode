using AssignmentService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AssignmentService.Application.DTOs.Common;

public class AssignmentUserDto
{
    public Guid? AssignmentUserId { get; set; }
    
    [Required]
    public Guid AssignmentId { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    public AssignmentUserStatus Status { get; set; }
    
    public DateTime AssignedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    
    public int? Score { get; set; }
    public int? MaxScore { get; set; }
    
    /// <summary>
    /// Number of times student switched tabs during examination
    /// </summary>
    public int TabSwitchCount { get; set; }
    
    /// <summary>
    /// Number of times AI usage was detected during examination
    /// </summary>
    public int CapturedAICount { get; set; }
}
