using AssignmentService.Domain.Entities;

namespace AssignmentService.Application.Interfaces.Services;
/// <summary>
/// Service interface cho Dataset entity
/// </summary>
public interface IDatasetService
{
    Task<Dataset> CreateDatasetAsync(Dataset dataset);
    Task<Dataset> UpdateDatasetAsync(Dataset dataset);
    Task<bool> DeleteDatasetAsync(Guid id);
    Task<Dataset?> GetDatasetByIdAsync(Guid id);
    Task<Dataset?> GetDatasetByIdWithDetailsAsync(Guid id);
    Task<List<Dataset>> GetAllDatasetsAsync();
    Task<List<Dataset>> GetDatasetsByProblemIdAsync(Guid problemId);
    Task<bool> CheckDatasetExistsByUserIdAsync(Guid userId);

}