using AssignmentService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AssignmentService.Application.DTOs.Common;

public class ProblemAssetDto
{
    public Guid? ProblemAssetId { get; set; }
    
    [Required]
    public AssetType Type { get; set; }
    
    [Required]
    [StringLength(500)]
    public string ObjectRef { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? Checksum { get; set; }
}