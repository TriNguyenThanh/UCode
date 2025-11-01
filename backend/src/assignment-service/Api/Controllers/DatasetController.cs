using AutoMapper;
using AssignmentService.Application.DTOs.Requests;
using AssignmentService.Application.DTOs.Responses;
using AssignmentService.Application.DTOs.Common;
using AssignmentService.Domain.Entities;
using AssignmentService.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using AssignmentService.Api.Middlewares;

namespace AssignmentService.Api.Controllers;
/// <summary>
/// Controller for managing datasets (test cases) in the ucode.io.vn platform
/// </summary>
[ApiController]
[Route("api/v1/datasets")]
[RequireRole("teacher")]
[ValidateUserId]
public class DatasetController : ControllerBase
{
    private readonly IDatasetService _datasetService;
    private readonly IProblemService _problemService;
    private readonly IMapper _mapper;

    public DatasetController(IDatasetService datasetService, IProblemService problemService, IMapper mapper)
    {
        _datasetService = datasetService;
        _problemService = problemService;
        _mapper = mapper;
    }

    #region Helper Methods
    
    /// <summary>
    /// Gets authenticated user ID from X-User-Id header
    /// </summary>
    private Guid GetAuthenticatedUserId()
    {
        var userId = HttpContext.Items["X-User-Id"]?.ToString();
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
            throw new ApiException("X-User-Id header is missing or invalid");
        
        return userIdGuid;
    }

    /// <summary>
    /// Helper method to verify user owns the problem using lightweight query
    /// </summary>
    private async Task VerifyProblemOwnershipLightweightAsync(Guid problemId, Guid userId)
    {
        var ownerId = await _problemService.GetProblemOwnerIdAsync(problemId);
        if (ownerId == null)
            throw new ApiException("Problem not found");
        
        if (ownerId != userId)
            throw new ApiException("You do not have permission to access this dataset");
    }

    /// <summary>
    /// Helper method to verify user owns the problem associated with a dataset (lightweight)
    /// Returns only the ProblemId without fetching full Dataset
    /// </summary>
    private async Task<Guid> GetDatasetProblemIdAndVerifyOwnershipAsync(Guid datasetId, Guid userId)
    {
        var dataset = await _datasetService.GetDatasetByIdAsync(datasetId);
        if (dataset == null)
            throw new ApiException("Dataset not found");
        
        await VerifyProblemOwnershipLightweightAsync(dataset.ProblemId, userId);
        
        return dataset.ProblemId;
    }

    /// <summary>
    /// Helper method to verify user owns the problem associated with an existing dataset
    /// Fetches full Dataset with details - use only when full entity is needed
    /// </summary>
    private async Task<Dataset> GetDatasetAndVerifyOwnershipAsync(Guid datasetId, Guid userId)
    {
        var dataset = await _datasetService.GetDatasetByIdWithDetailsAsync(datasetId);
        if (dataset == null)
            throw new ApiException("Dataset not found");
        
        await VerifyProblemOwnershipLightweightAsync(dataset.ProblemId, userId);
        
        return dataset;
    }

    #endregion

    /// <summary>
    /// Creates a new dataset (test case)
    /// </summary>
    /// <param name="datasetDto">Dataset creation request containing dataset details</param>
    /// <returns>Created dataset information</returns>
    /// <response code="200">Dataset created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized - Teacher role required</response>
    /// <response code="422">Validation errors</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<DatasetDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ValidationErrorResponse), 422)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> CreateDataset([FromBody] DatasetDto datasetDto)
    {
        var userId = GetAuthenticatedUserId();
        
        // Verify user owns the problem before creating dataset (lightweight)
        if (datasetDto.ProblemId == null)
            throw new ApiException("ProblemId is required");
            
        await VerifyProblemOwnershipLightweightAsync(datasetDto.ProblemId.Value, userId);

        var dataset = _mapper.Map<Dataset>(datasetDto);
        var createdDataset = await _datasetService.CreateDatasetAsync(dataset);
        var createdDatasetDto = _mapper.Map<DatasetDto>(createdDataset);
        return Ok(ApiResponse<DatasetDto>.SuccessResponse(createdDatasetDto, "Dataset created successfully"));
    }

    /// <summary>
    /// Updates an existing dataset (test case)
    /// </summary>
    /// <param name="updateDatasetDto">Dataset update request containing updated dataset details</param>
    /// <returns>Updated dataset information</returns>
    /// <response code="200">Dataset updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized - Teacher role required</response>
    /// <response code="404">Dataset not found</response>
    /// <response code="422">Validation errors</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("update")]
    [ProducesResponseType(typeof(ApiResponse<DatasetDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ValidationErrorResponse), 422)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> UpdateDataset([FromBody] UpdateDatasetDto updateDatasetDto)
    {
        var userId = GetAuthenticatedUserId();
        
        // Verify ownership before updating (lightweight - don't need full Dataset)
        if (updateDatasetDto.DatasetId == null)
            throw new ApiException("DatasetId is required");
            
        await GetDatasetProblemIdAndVerifyOwnershipAsync(updateDatasetDto.DatasetId.Value, userId);

        var dataset = _mapper.Map<Dataset>(updateDatasetDto);
        var updatedDataset = await _datasetService.UpdateDatasetAsync(dataset);
        var updatedUpdateDatasetDto = _mapper.Map<DatasetDto>(updatedDataset);
        return Ok(ApiResponse<DatasetDto>.SuccessResponse(updatedUpdateDatasetDto, "Dataset updated successfully"));
    }

    /// <summary>
    /// Deletes a dataset by ID
    /// </summary>
    /// <param name="uid">The unique identifier of the dataset to delete</param>
    /// <returns>Success confirmation</returns>
    /// <response code="200">Dataset deleted successfully</response>
    /// <response code="401">Unauthorized - Teacher role required</response>
    /// <response code="404">Dataset not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("del")]
    [ProducesResponseType(typeof(ApiResponse<DatasetDto>), 200)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> DeleteDataset([FromQuery] Guid uid)
    {
        var userId = GetAuthenticatedUserId();
        
        // Verify ownership before deleting (lightweight - don't need full Dataset)
        await GetDatasetProblemIdAndVerifyOwnershipAsync(uid, userId);

        await _datasetService.DeleteDatasetAsync(uid);
        return Ok(ApiResponse<DatasetDto>.SuccessResponse(new DatasetDto(), "Dataset deleted successfully"));
    }

    /// <summary>
    /// Retrieves a specific dataset by ID with details
    /// </summary>
    /// <param name="uid">The unique identifier of the dataset</param>
    /// <returns>Dataset details including test cases</returns>
    /// <response code="200">Dataset retrieved successfully</response>
    /// <response code="401">Unauthorized - Teacher role required</response>
    /// <response code="404">Dataset not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("get")]
    [ProducesResponseType(typeof(ApiResponse<DatasetDto>), 200)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GetDatasetById([FromQuery] Guid uid)
    {
        var userId = GetAuthenticatedUserId();
        
        // Need full Dataset with details to return, so use full query
        var dataset = await GetDatasetAndVerifyOwnershipAsync(uid, userId);
        
        var datasetDto = _mapper.Map<DatasetDto>(dataset);
        return Ok(ApiResponse<DatasetDto>.SuccessResponse(datasetDto, "Dataset retrieved successfully"));
    }
}