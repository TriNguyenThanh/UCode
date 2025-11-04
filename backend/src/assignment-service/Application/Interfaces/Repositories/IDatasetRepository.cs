using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;

namespace AssignmentService.Application.Interfaces.Repositories;
/// <summary>
/// Repository interface cho Dataset entity
/// </summary>
///
public interface IDatasetRepository : IRepository<Dataset>
{
    Task<Dataset?> GetByIdWithDetailsAsync(Guid id);
    Task<List<Dataset>> GetByProblemIdAsync(Guid problemId);
    Task<bool> CheckDatasetExistsByUserIdAsync(Guid userId);
}