using AssignmentService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AssignmentService.Application.DTOs.Common;

public class ProblemAssetDto
{
    public Guid ProblemAssetId { get; set; }
    public Guid ProblemId { get; set; }
    public AssetType Type { get; set; }
    public string ObjectRef { get; set; } = string.Empty;
    public string? Checksum { get; set; }
    public string? Title { get; set; }
    public ContentFormat Format { get; set; }
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating ProblemAsset
/// </summary>
public class CreateProblemAssetDto
{
    public AssetType Type { get; set; }
    public string ObjectRef { get; set; } = string.Empty;
    public string? Checksum { get; set; }
    public string? Title { get; set; }
    public ContentFormat Format { get; set; } = ContentFormat.MARKDOWN;
    public int OrderIndex { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for updating ProblemAsset
/// </summary>
public class UpdateProblemAssetDto
{
    public Guid ProblemAssetId { get; set; }
    public AssetType? Type { get; set; }
    public string? ObjectRef { get; set; }
    public string? Checksum { get; set; }
    public string? Title { get; set; }
    public ContentFormat? Format { get; set; }
    public int? OrderIndex { get; set; }
    public bool? IsActive { get; set; }
}