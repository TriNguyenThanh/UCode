using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;

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
    Task<List<Dataset>> GetDatasetsByProblemIdAsync(Guid problemId, DatasetKind? datasetKind=null);
    Task<bool> CheckDatasetExistsByUserIdAsync(Guid userId);

}