using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;

namespace AssignmentService.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface cho Problem entity
/// </summary>
public interface IProblemRepository : IRepository<Problem>
{
    Task<Problem?> GetByIdWithDetailsAsync(Guid id);
    Task<Problem?> GetBySlugAsync(string slug);
    Task<Problem?> GetByCodeAsync(string code);
    Task<List<Problem>> GetByOwnerIdAsync(Guid ownerId);
    Task<List<Problem>> GetByTagNameAsync(string tagName);
    Task<List<Problem>> GetPublishedProblemsAsync(int page, int pageSize);
    Task<List<Problem>> SearchProblemsAsync(string? keyword, string? difficulty, int page, int pageSize);
    Task<string> GetNextCodeSequenceAsync();
    Task<bool> SlugExistsAsync(string slug, Guid? excludeProblemId = null);
    Task<bool> CodeExistsAsync(string code, Guid? excludeProblemId = null);
    Task<List<Dataset>> GetDatasetsByProblemIdAsync(Guid problemId);
    Task<Guid?> GetProblemOwnerIdAsync(Guid problemId);
    Task<(bool exists, Guid? ownerId)> CheckExistsAndGetOwnerAsync(Guid problemId);
    Task<(bool exists, Guid? ownerId, Visibility visibility)> GetProblemBasicInfoAsync(Guid problemId);
}