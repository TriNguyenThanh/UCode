using AssignmentService.Application.Interfaces.Repositories;
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
}