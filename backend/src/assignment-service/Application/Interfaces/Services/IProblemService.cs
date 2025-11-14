using AssignmentService.Application.Interfaces.Repositories;
using AssignmentService.Application.DTOs.Common;
using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;

namespace AssignmentService.Application.Interfaces.Services;

public interface IProblemService
{
    Task<Problem> CreateProblemAsync(
        string code,
        string title,
        Difficulty difficulty,
        Guid ownerId,
        Visibility visibility = Visibility.PRIVATE);

    Task<Problem> GetProblemByIdAsync(Guid problemId);
    Task<List<Problem>> GetProblemsByOwnerIdAsync(Guid ownerId);
    Task<(List<Problem> problems, int total)> GetProblemsByOwnerIdWithPaginationAsync(Guid ownerId, int page, int pageSize);
    Task<List<Problem>> GetPublicProblemsAsync();
    Task<bool> DeleteProblemAsync(Guid problemId);

    Task<Problem> UpdateProblemAsync(Problem problem);
    Task<List<Dataset>> GetDatasetsByProblemIdAsync(Guid problemId);
    
    /// <summary>
    /// Gets only OwnerId for a problem (lightweight query for ownership check)
    /// </summary>
    Task<Guid?> GetProblemOwnerIdAsync(Guid problemId);
    
    /// <summary>
    /// Checks if problem exists and returns its OwnerId
    /// </summary>
    Task<(bool exists, Guid? ownerId)> CheckExistsAndGetOwnerAsync(Guid problemId);
    
    /// <summary>
    /// Gets basic problem info for authorization check (ultra lightweight)
    /// Returns: (exists, ownerId, visibility)
    /// </summary>
    Task<(bool exists, Guid? ownerId, Visibility visibility)> GetProblemBasicInfoAsync(Guid problemId);
    
    // ProblemAsset methods
    Task<List<ProblemAsset>> GetProblemAssetsAsync(Guid problemId);
    Task<ProblemAsset> AddProblemAssetAsync(Guid problemId, CreateProblemAssetDto request);
    Task<ProblemAsset> UpdateProblemAssetAsync(Guid problemId, Guid assetId, UpdateProblemAssetDto request);
    Task<bool> DeleteProblemAssetAsync(Guid problemId, Guid assetId);
    
    // Tag methods
    Task AddTagsToProblemAsync(Guid problemId, List<Guid> tagIds);
    Task RemoveProblemTagAsync(Guid problemId, Guid tagId);
    Task<List<Tag>> GetAllTagsAsync();
    Task<List<Problem>> GetProblemsByTagAsync(string tagName);
    
    // ProblemLanguage methods (using new Language + ProblemLanguage schema)
    Task<List<ProblemLanguage>> GetProblemLanguagesAsync(Guid problemId);
    Task<ProblemLanguage?> GetProblemLanguageAsync(Guid problemId, Guid languageId);
    Task<List<ProblemLanguage>> AddOrUpdateProblemLanguagesAsync(Guid problemId, List<ProblemLanguageDto> requests);
    Task<bool> DeleteProblemLanguageAsync(Guid problemId, Guid languageId);
    
    // Search methods
    Task<(List<Problem> problems, int total)> SearchProblemsAsync(string? keyword, string? difficulty, int page, int pageSize);
}