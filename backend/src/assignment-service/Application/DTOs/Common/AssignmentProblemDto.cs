using System.ComponentModel.DataAnnotations;

namespace AssignmentService.Application.DTOs.Common;

public class AssignmentProblemDto
{
    [Required]
    public Guid ProblemId { get; set; }
    public string ProblemTitle { get; set; } = string.Empty;
    
    [Range(1, 1000)]
    public int Points { get; set; } = 100;
    
    public int OrderIndex { get; set; }
}
