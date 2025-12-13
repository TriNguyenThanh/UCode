using AssignmentService.Application.Interfaces.Repositories;
using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Domain.Entities;
using AssignmentService.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using AssignmentService.Application.DTOs.Common;
using System.Data.Common;
using AssignmentService.Domain.Enums;

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
        try
        {
            // Check if a dataset of the same kind already exists for this problem
            var existingDatasets = await _datasetRepository.GetByProblemIdAsync(dataset.ProblemId, dataset.Kind);
            
            if (existingDatasets != null && existingDatasets.Any())
            {
                var kindName = dataset.Kind == DatasetKind.SAMPLE ? "SAMPLE" : "OFFICIAL";
                throw new ApiException($"A {kindName} dataset already exists for this problem. Only one {kindName} dataset is allowed per problem. ", 400);
            }
            
            return await _datasetRepository.AddAsync(dataset);
        }
        catch (ApiException)
        {
            // Re-throw ApiException as-is
            throw;
        }
        catch (DbException ex)
        {
            throw new ApiException($"Database error while creating dataset: {ex.Message}", 500);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error creating dataset: {ex.Message}", 500);
        }
    }

    public async Task<bool> DeleteDatasetAsync(Guid id)
    {
        try
        {
            return await _datasetRepository.RemoveAsync(id);
        }
        catch (DbException ex)
        {
            throw new ApiException($"Database error while deleting dataset: {ex.Message}", 500);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error deleting dataset: {ex.Message}", 500);
        }
    }

    public async Task<List<Dataset>> GetAllDatasetsAsync()
    {
        try
        {
            return await _datasetRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving all datasets: {ex.Message}", 500);
        }
    }

    public async Task<Dataset?> GetDatasetByIdAsync(Guid id)
    {
        try
        {
            return await _datasetRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving dataset: {ex.Message}", 500);
        }
    }

    public async Task<Dataset?> GetDatasetByIdWithDetailsAsync(Guid id)
    {
        try
        {
            return await _datasetRepository.GetByIdWithDetailsAsync(id);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving dataset with details: {ex.Message}", 500);
        }
    }

    public async Task<List<Dataset>> GetDatasetsByProblemIdAsync(Guid problemId, DatasetKind? datasetKind)
    {
        try
        {
            return await _datasetRepository.GetByProblemIdAsync(problemId, datasetKind);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving datasets for problem: {ex.Message}", 500);
        }
    }

    public async Task<Dataset> UpdateDatasetAsync(Dataset dataset)
    {
        try
        {
            // Get the existing dataset to check if DatasetKind is being changed
            var existingDataset = await _datasetRepository.GetByIdAsync(dataset.DatasetId);
            
            if (existingDataset == null)
            {
                throw new ApiException("Dataset not found", 404);
            }
            
            // If DatasetKind is being changed, check if the new kind already exists for this problem
            if (existingDataset.Kind != dataset.Kind)
            {
                var datasetsWithNewKind = await _datasetRepository.GetByProblemIdAsync(dataset.ProblemId, dataset.Kind);
                
                if (datasetsWithNewKind != null && datasetsWithNewKind.Any())
                {
                    var kindName = dataset.Kind == DatasetKind.SAMPLE ? "SAMPLE" : "OFFICIAL";
                    throw new ApiException($"A {kindName} dataset already exists for this problem. Only one {kindName} dataset is allowed per problem. ", 400);
                }
            }
            
            return await _datasetRepository.UpdateAsync(dataset);
        }
        catch (ApiException)
        {
            // Re-throw ApiException as-is
            throw;
        }
        catch (DbException ex)
        {
            throw new ApiException($"Database error while updating dataset: {ex.Message}", 500);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error updating dataset: {ex.Message}", 500);
        }
    }

    public async Task<bool> CheckDatasetExistsByUserIdAsync(Guid userId)
    {
        try
        {
            return await _datasetRepository.CheckDatasetExistsByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error checking dataset existence for user: {ex.Message}", 500);
        }
    }
}