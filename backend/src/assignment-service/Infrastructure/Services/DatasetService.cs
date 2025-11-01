using AssignmentService.Application.Interfaces.Repositories;
using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Domain.Entities;
using AssignmentService.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AssignmentService.Infrastructure.Services;
/// <summary>
/// Service implementation cho Dataset entity
/// </summary>
public class DatasetService : IDatasetService
{
    private readonly IDatasetRepository _datasetRepository;

    public DatasetService(IDatasetRepository datasetRepository)
    {
        _datasetRepository = datasetRepository;
    }

    public async Task<Dataset> CreateDatasetAsync(Dataset dataset)
    {
        return await _datasetRepository.AddAsync(dataset);
    }

    public async Task<bool> DeleteDatasetAsync(Guid id)
    {
        return await _datasetRepository.RemoveAsync(id);
    }

    public async Task<List<Dataset>> GetAllDatasetsAsync()
    {
        return await _datasetRepository.GetAllAsync();
    }

    public async Task<Dataset?> GetDatasetByIdAsync(Guid id)
    {
        return await _datasetRepository.GetByIdAsync(id);
    }

    public async Task<Dataset?> GetDatasetByIdWithDetailsAsync(Guid id)
    {
        return await _datasetRepository.GetByIdWithDetailsAsync(id);
    }

    public async Task<List<Dataset>> GetDatasetsByProblemIdAsync(Guid problemId)
    {
        return await _datasetRepository.GetByProblemIdAsync(problemId);
    }

    public async Task<Dataset> UpdateDatasetAsync(Dataset dataset)
    {
        return await _datasetRepository.UpdateAsync(dataset);
    }

    public async Task<bool> CheckDatasetExistsByUserIdAsync(Guid userId)
    {
        return await _datasetRepository.CheckDatasetExistsByUserIdAsync(userId);
    }
}