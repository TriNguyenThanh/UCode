using AssignmentService.Application.Validators;
using AssignmentService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AssignmentService.Application.DTOs.Common;

public class DatasetDto
{
    public Guid? DatasetId { get; set; }

    [Required(ErrorMessage = "ProblemId is required")]
    [GuidNotEmpty(ErrorMessage = "ProblemId must be a valid non-empty GUID")]
    public Guid? ProblemId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public DatasetKind Kind { get; set; } = DatasetKind.SAMPLE;

    [Required]
    public List<TestCaseDto>? TestCases { get; set; }
}

public class UpdateDatasetDto
{
    [Required(ErrorMessage = "DatasetId is required")]
    [GuidNotEmpty(ErrorMessage = "DatasetId must be a valid non-empty GUID")]
    public Guid? DatasetId { get; set; }

    [Required(ErrorMessage = "ProblemId is required")]
    [GuidNotEmpty(ErrorMessage = "ProblemId must be a valid non-empty GUID")]
    public Guid? ProblemId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public DatasetKind Kind { get; set; } = DatasetKind.SAMPLE;

    [Required]
    public List<TestCaseDto>? TestCases { get; set; }
}