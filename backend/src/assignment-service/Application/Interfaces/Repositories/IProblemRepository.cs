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
    Task<(List<Problem> problems, int total)> GetByOwnerIdWithPaginationAsync(Guid ownerId, int page, int pageSize);
    Task<List<Problem>> GetByTagNameAsync(string tagName);
    Task<List<Problem>> GetPublishedProblemsAsync(int page, int pageSize);
    Task<(List<Problem> problems, int total)> SearchProblemsAsync(string? keyword, string? difficulty, int page, int pageSize);
    Task<string> GetNextCodeSequenceAsync();
    Task<bool> SlugExistsAsync(string slug, Guid? excludeProblemId = null);
    Task<bool> CodeExistsAsync(string code, Guid? excludeProblemId = null);
    Task<List<Dataset>> GetDatasetsByProblemIdAsync(Guid problemId);
    Task<Guid?> GetProblemOwnerIdAsync(Guid problemId);
    Task<(bool exists, Guid? ownerId)> CheckExistsAndGetOwnerAsync(Guid problemId);
    Task<(bool exists, Guid? ownerId, Visibility visibility)> GetProblemBasicInfoAsync(Guid problemId);
    
    // ProblemAsset methods
    Task<List<ProblemAsset>> GetProblemAssetsAsync(Guid problemId);
    Task<ProblemAsset> AddProblemAssetAsync(ProblemAsset asset);
    Task<ProblemAsset?> GetProblemAssetByIdAsync(Guid assetId);
    Task<bool> UpdateProblemAssetAsync(ProblemAsset asset);
    Task<bool> DeleteProblemAssetAsync(Guid assetId);
    
    // Tag methods
    Task<List<Tag>> GetAllTagsAsync();
    Task AddProblemTagsAsync(Guid problemId, List<Guid> tagIds);
    Task RemoveProblemTagAsync(Guid problemId, Guid tagId);
    
    // ProblemLanguage methods (using new Language + ProblemLanguage schema)
    Task<Problem?> GetByIdWithLanguagesAsync(Guid problemId);
    Task<List<ProblemLanguage>> GetProblemLanguagesAsync(Guid problemId);
    Task<ProblemLanguage?> GetProblemLanguageAsync(Guid problemId, Guid languageId);
    Task<ProblemLanguage> AddProblemLanguageAsync(ProblemLanguage problemLanguage);
    Task<bool> UpdateProblemLanguageAsync(ProblemLanguage problemLanguage);
    Task<bool> DeleteProblemLanguageAsync(Guid problemId, Guid languageId);

    Task SaveChangesAsync();
}