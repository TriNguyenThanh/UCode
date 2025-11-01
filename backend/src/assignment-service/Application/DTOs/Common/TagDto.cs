namespace AssignmentService.Application.DTOs.Common;

public class TagDto
{
    public Guid TagId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
}