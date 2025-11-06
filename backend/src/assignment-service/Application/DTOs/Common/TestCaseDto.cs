using System.ComponentModel.DataAnnotations;

namespace AssignmentService.Application.DTOs.Common;

public class TestCaseDto
{
    public Guid? TestCaseId { get; set; }
    
    [Required]
    [StringLength(500)]
    public string InputRef { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string OutputRef { get; set; } = string.Empty;

    [Required]
    [Range(1, 200)]
    public int IndexNo { get; set; } = 1;

    [Range(0, 100)]
    public decimal Score { get; set; } = 1.0m;
    
    [StringLength(100)]
    public string? InputChecksum { get; set; }
    
    [StringLength(100)]
    public string? OutputChecksum { get; set; }
}