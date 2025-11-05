using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;

namespace AssignmentService.Application.Interfaces.Services;

/// <summary>
/// Service interface for Tag management
/// </summary>
public interface ITagService
{
    Task<List<Tag>> GetAllTagsAsync(string? category = null);
    Task<Tag> GetTagByIdAsync(Guid tagId);
    Task<Tag?> GetTagByNameAsync(string name);
    Task<Tag> CreateTagAsync(string name, TagCategory category);
    Task<Tag> UpdateTagAsync(Guid tagId, string name, TagCategory category);
    Task<bool> DeleteTagAsync(Guid tagId);
    Task<List<Problem>> GetProblemsByTagIdAsync(Guid tagId);
    Task<bool> TagExistsAsync(string name);
}
