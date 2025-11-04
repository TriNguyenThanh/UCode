using AssignmentService.Domain.Entities;

namespace AssignmentService.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Tag entity
/// </summary>
public interface ITagRepository : IRepository<Tag>
{
    Task<List<Problem>> GetProblemsByTagIdAsync(Guid tagId);
}
