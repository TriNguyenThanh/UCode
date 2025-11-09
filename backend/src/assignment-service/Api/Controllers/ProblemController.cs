using AssignmentService.Application.DTOs.Requests;
using AssignmentService.Application.DTOs.Responses;
using AssignmentService.Application.Interfaces.Repositories;
using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using AssignmentService.Application.DTOs.Common;
using AutoMapper;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using AssignmentService.Api.Middlewares;
using AssignmentService.Domain.Entities;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

namespace AssignmentService.Api.Controllers;

/// <summary>
/// Controller for managing problems in the ucode.io.vn platform
/// </summary>
[ApiController]
[Route("api/v1/problems")]
[ValidateUserId]
public class ProblemController : ControllerBase
{
    private readonly IProblemService _problemService;
    private readonly ILanguageService _languageService;
    private readonly IDatasetService _datasetService;
    private readonly IMapper _mapper;

    public ProblemController(IProblemService problemService, ILanguageService languageService, IDatasetService datasetService, IMapper mapper)
    {
        _problemService = problemService;
        _languageService = languageService;
        _mapper = mapper;
        _datasetService = datasetService;
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
    /// Gets problem by ID and throws exception if not found
    /// </summary>
    private async Task<Problem> GetProblemOrThrowAsync(Guid problemId)
    {
        var problem = await _problemService.GetProblemByIdAsync(problemId);
        if (problem == null)
            throw new ApiException("Problem not found");
        
        return problem;
    }

    /// <summary>
    /// Verifies that the authenticated user owns the problem
    /// </summary>
    private void VerifyOwnership(Problem problem, Guid userId)
    {
        if (problem.OwnerId != userId)
            throw new ApiException("You do not have permission to access this problem");
    }

    /// <summary>
    /// Verifies ownership using lightweight query (only fetches OwnerId)
    /// </summary>
    private async Task VerifyOwnershipLightweightAsync(Guid problemId, Guid userId)
    {
        var ownerId = await _problemService.GetProblemOwnerIdAsync(problemId);
        if (ownerId == null)
            throw new ApiException("Problem not found");

        if (ownerId.Value != userId)
            throw new ApiException("You do not have permission to access this problem");
    }

    /// <summary>
    /// Gets problem and verifies ownership in one call
    /// </summary>
    private async Task<Problem> GetProblemAndVerifyOwnershipAsync(Guid problemId, Guid userId)
    {
        var problem = await GetProblemOrThrowAsync(problemId);
        VerifyOwnership(problem, userId);
        return problem;
    }

    #endregion

    /// <summary>
    /// Creates a new problem
    /// </summary>
    /// <param name="request">Problem creation request containing problem details</param>
    /// <returns>Created problem information</returns>
    /// <response code="200">Problem created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized - Teacher role required</response>
    /// <response code="500">Internal server error</response>
    [RequireRole("teacher,admin")]
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<ProblemResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> CreateProblem([FromBody] ProblemRequest request)
    {
        if (request == null)
            throw new ApiException("Request body is null");
        
        var ownerId = GetAuthenticatedUserId();

        var problem = await _problemService.CreateProblemAsync(
            request.Code ?? string.Empty,
            request.Title,
            request.Difficulty,
            ownerId,
            request.Visibility);

        return Ok(ApiResponse<ProblemResponse>.SuccessResponse(_mapper.Map<ProblemResponse>(problem), "Problem created successfully"));
    }

    /// <summary>
    /// Retrieves a specific problem by ID
    /// </summary>
    /// <param name="problemId">The unique identifier of the problem</param>
    /// <returns>Problem details</returns>
    /// <response code="200">Problem retrieved successfully</response>
    /// <response code="404">Problem not found</response>
    /// <response code="401">Unauthorized - Teacher role required</response>
    /// <response code="403">Forbidden - You don't have permission to access this problem</response>
    [HttpGet("{problemId:guid}")]
    [RequireRole("teacher,admin")]
    [ProducesResponseType(typeof(ApiResponse<ProblemResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ForbiddenErrorResponse), 403)]
    public async Task<IActionResult> GetProblem(Guid problemId)
    {
        var (exists, ownerId, visibility) = await _problemService.GetProblemBasicInfoAsync(problemId);
        
        if (!exists)
            throw new ApiException("Problem not found");
        
        if (visibility == Visibility.PRIVATE)
        {
            var userId = GetAuthenticatedUserId();
            if (ownerId != userId)
                throw new ApiException("You do not have permission to access this problem");
        }
        
        var problem = await GetProblemOrThrowAsync(problemId);
        var problemRes = _mapper.Map<ProblemResponse>(problem);
        
        // Get sample dataset and map to DTO
        var datasets = await _datasetService.GetDatasetsByProblemIdAsync(problemId, DatasetKind.SAMPLE);
        var sampleDataset = datasets?.FirstOrDefault();
        if (sampleDataset != null)
        {
            problemRes.datasetSample = _mapper.Map<DatasetDto>(sampleDataset);
        }

        return Ok(ApiResponse<ProblemResponse>.SuccessResponse(problemRes, "Problem retrieved successfully"));
    }

    /// <summary>
    /// Retrieves a specific problem by ID (Student) - Only public/published problems
    /// </summary>
    /// <param name="problemId">The unique identifier of the problem</param>
    /// <returns>Problem details without owner information</returns>
    /// <response code="200">Problem retrieved successfully</response>
    /// <response code="404">Problem not found or not accessible</response>
    /// <response code="401">Unauthorized - Student role required</response>
    [HttpGet("student/get/{problemId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProblemResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    public async Task<IActionResult> GetProblemForStudent(Guid problemId)
    {
        
        var problem = await GetProblemOrThrowAsync(problemId);
        
        var visibility = problem.Visibility;
        
        if (visibility != Visibility.PUBLIC)
            throw new ApiException("Problem not accessible");
        
        var response = _mapper.Map<ProblemResponse>(problem);
        
        response.OwnerId = Guid.Empty;
        
        var datasets = await _datasetService.GetDatasetsByProblemIdAsync(problemId, DatasetKind.SAMPLE);
        var sampleDataset = datasets?.FirstOrDefault();
        if (sampleDataset != null)
        {
            response.datasetSample = _mapper.Map<DatasetDto>(sampleDataset);
        }

        return Ok(ApiResponse<ProblemResponse>.SuccessResponse(response, "Problem retrieved successfully"));
    }

    /// <summary>
    /// Retrieves all problems created by the current teacher with pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20, max: 100)</param>
    /// <returns>Paginated list of problems owned by the current teacher</returns>
    /// <response code="200">Problems retrieved successfully</response>
    /// <response code="401">Unauthorized - Teacher role required</response>
    /// <response code="400">Bad request - Invalid pagination parameters</response>
    [RequireRole("teacher,admin")]
    [HttpGet("all-problems")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ProblemResponse>>), 200)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    public async Task<IActionResult> GetProblemsByOwner(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1)
            throw new ApiException("Page must be greater than 0");
        
        if (pageSize < 1 || pageSize > 100)
            throw new ApiException("Page size must be between 1 and 100");
            
        var ownerId = GetAuthenticatedUserId();

        var (problems, total) = await _problemService.GetProblemsByOwnerIdWithPaginationAsync(ownerId, page, pageSize);

        var problemDtos = _mapper.Map<List<ProblemResponse>>(problems);
        
        var pagedResponse = new PagedResponse<ProblemResponse>
        {
            Data = problemDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        };

        return Ok(ApiResponse<PagedResponse<ProblemResponse>>.SuccessResponse(pagedResponse));
    }

    /// <summary>
    /// Deletes a problem by ID
    /// </summary>
    /// <param name="uid">The unique identifier of the problem to delete</param>
    /// <returns>Success confirmation</returns>
    /// <response code="200">Problem deleted successfully</response>
    /// <response code="404">Problem not found</response>
    /// <response code="401">Unauthorized - Teacher role required</response>
    /// <response code="403">Forbidden - You don't have permission to delete this problem</response>
    /// <response code="500">Internal server error</response>
    [RequireRole("teacher,admin")]
    [HttpDelete("del")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ForbiddenErrorResponse), 403)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> DeleteProblem([FromQuery] Guid uid)
    {
        var userId = GetAuthenticatedUserId();
        
        await VerifyOwnershipLightweightAsync(uid, userId);

        var result = await _problemService.DeleteProblemAsync(uid);

        if (!result)
        {
            throw new ApiException("Failed to delete problem");
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Problem deleted successfully"));
    }

    /// <summary>
    /// Updates an existing problem
    /// </summary>
    /// <param name="request">Problem update request containing updated problem details</param>
    /// <returns>Updated problem information</returns>
    /// <response code="200">Problem updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Problem not found</response>
    /// <response code="401">Unauthorized - Teacher role required</response>
    /// <response code="403">Forbidden - You don't have permission to update this problem</response>
    /// <response code="422">Validation errors</response>
    /// <response code="500">Internal server error</response>
    [RequireRole("teacher,admin")]
    [HttpPut("update")]
    [ProducesResponseType(typeof(ApiResponse<ProblemResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ForbiddenErrorResponse), 403)]
    [ProducesResponseType(typeof(ValidationErrorResponse), 422)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> UpdateProblem([FromBody] ProblemRequest request)
    {
        if (request == null)
            throw new ApiException("Request body is null");

        var userId = GetAuthenticatedUserId();

        // Verify ownership before update
        await VerifyOwnershipLightweightAsync(request.ProblemId, userId);

        var problem = _mapper.Map<Problem>(request);
        problem.OwnerId = userId; // Keep original owner (already verified)
        
        // Handle ProblemAssets if provided
        if (request.ProblemAssets != null && request.ProblemAssets.Any())
        {
            problem.ProblemAssets = request.ProblemAssets
                .Select(dto => _mapper.Map<ProblemAsset>(dto))
                .ToList();
            
            // Set ProblemId for all assets
            foreach (var asset in problem.ProblemAssets)
            {
                asset.ProblemId = problem.ProblemId;
            }
        }

        var updatedProblem = await _problemService.UpdateProblemAsync(problem);

        return Ok(ApiResponse<ProblemResponse>.SuccessResponse(_mapper.Map<ProblemResponse>(updatedProblem), "Problem updated successfully"));
    }    
    
    /// <summary>
    /// Retrieves datasets (test cases) for a specific problem
    /// </summary>
    /// <param name="problemId">The unique identifier of the problem</param>
    /// <returns>List of datasets for the problem</returns>
    /// <response code="200">Datasets retrieved successfully</response>
    /// <response code="404">Problem not found</response>
    /// <response code="401">Unauthorized - Teacher role required</response>
    /// <response code="500">Internal server error</response>
    [RequireRole("teacher,admin")]
    [HttpGet("get-datasets")]
    [ProducesResponseType(typeof(ApiResponse<List<DatasetDto>>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GetDatasetsByProblemId([FromQuery] Guid problemId)
    {
        var userId = GetAuthenticatedUserId();
        
        await VerifyOwnershipLightweightAsync(problemId, userId);

        var datasets = await _problemService.GetDatasetsByProblemIdAsync(problemId);
        var datasetDtos = _mapper.Map<List<DatasetDto>>(datasets);
        return Ok(ApiResponse<List<DatasetDto>>.SuccessResponse(datasetDtos));
    }

    /// <summary>
    /// Retrieves public problems available for students
    /// </summary>
    /// <returns>List of public problems</returns>
    /// <response code="200">Public problems retrieved successfully</response>
    /// <response code="401">Unauthorized - Student role required</response>
    /// <response code="500">Internal server error</response>
    [RequireRole("student")]
    [HttpGet("student/get-public-problems")]
    [ProducesResponseType(typeof(ApiResponse<List<ProblemResponse>>), 200)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GetPublicProblems()
    {
        var problems = await _problemService.GetPublicProblemsAsync();

        var problemDtos = _mapper.Map<List<ProblemResponse>>(problems);

        return Ok(ApiResponse<List<ProblemResponse>>.SuccessResponse(problemDtos, "Public problems retrieved successfully"));
    }

    #region ProblemAsset APIs

    /// <summary>
    /// Get all assets for a problem
    /// </summary>
    [RequireRole("teacher,admin")]
    [HttpGet("{problemId:guid}/assets")]
    [ProducesResponseType(typeof(ApiResponse<List<ProblemAssetDto>>), 200)]
    public async Task<IActionResult> GetProblemAssets(Guid problemId)
    {
        var userId = GetAuthenticatedUserId();
        await VerifyOwnershipLightweightAsync(problemId, userId);

        var assets = await _problemService.GetProblemAssetsAsync(problemId);
        var assetDtos = _mapper.Map<List<ProblemAssetDto>>(assets);
        
        return Ok(ApiResponse<List<ProblemAssetDto>>.SuccessResponse(assetDtos));
    }

    /// <summary>
    /// Add a new asset to a problem
    /// </summary>
    [RequireRole("teacher,admin")]
    [HttpPost("{problemId:guid}/assets")]
    [ProducesResponseType(typeof(ApiResponse<ProblemAssetDto>), 200)]
    public async Task<IActionResult> AddProblemAsset(Guid problemId, [FromBody] CreateProblemAssetDto request)
    {
        var userId = GetAuthenticatedUserId();
        await VerifyOwnershipLightweightAsync(problemId, userId);

        var asset = await _problemService.AddProblemAssetAsync(problemId, request);
        var assetDto = _mapper.Map<ProblemAssetDto>(asset);
        
        return Ok(ApiResponse<ProblemAssetDto>.SuccessResponse(assetDto, "Asset added successfully"));
    }

    /// <summary>
    /// Update an existing problem asset
    /// </summary>
    [RequireRole("teacher,admin")]
    [HttpPut("{problemId:guid}/assets/{assetId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProblemAssetDto>), 200)]
    public async Task<IActionResult> UpdateProblemAsset(Guid problemId, Guid assetId, [FromBody] UpdateProblemAssetDto request)
    {
        var userId = GetAuthenticatedUserId();
        await VerifyOwnershipLightweightAsync(problemId, userId);

        var asset = await _problemService.UpdateProblemAssetAsync(problemId, assetId, request);
        var assetDto = _mapper.Map<ProblemAssetDto>(asset);
        
        return Ok(ApiResponse<ProblemAssetDto>.SuccessResponse(assetDto, "Asset updated successfully"));
    }

    /// <summary>
    /// Delete a problem asset
    /// </summary>
    [RequireRole("teacher,admin")]
    [HttpDelete("{problemId:guid}/assets/{assetId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<IActionResult> DeleteProblemAsset(Guid problemId, Guid assetId)
    {
        var userId = GetAuthenticatedUserId();
        await VerifyOwnershipLightweightAsync(problemId, userId);

        var result = await _problemService.DeleteProblemAssetAsync(problemId, assetId);
        
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Asset deleted successfully"));
    }

    #endregion

    #region Tag APIs

    /// <summary>
    /// Add tags to a problem
    /// </summary>
    [RequireRole("teacher,admin")]
    [HttpPost("{problemId:guid}/tags")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<IActionResult> AddTagsToProblem(Guid problemId, [FromBody] List<Guid> tagIds)
    {
        var userId = GetAuthenticatedUserId();
        await VerifyOwnershipLightweightAsync(problemId, userId);

        await _problemService.AddTagsToProblemAsync(problemId, tagIds);
        
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Tags added successfully"));
    }

    /// <summary>
    /// Remove a tag from a problem
    /// </summary>
    [RequireRole("teacher,admin")]
    [HttpDelete("{problemId:guid}/tags/{tagId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<IActionResult> RemoveTagFromProblem(Guid problemId, Guid tagId)
    {
        var userId = GetAuthenticatedUserId();
        await VerifyOwnershipLightweightAsync(problemId, userId);

        await _problemService.RemoveProblemTagAsync(problemId, tagId);
        
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Tag removed successfully"));
    }

    /// <summary>
    /// Search problems by tag name
    /// </summary>
    [HttpGet("by-tag/{tagName}")]
    [ProducesResponseType(typeof(ApiResponse<List<ProblemResponse>>), 200)]
    public async Task<IActionResult> GetProblemsByTag(string tagName)
    {
        var problems = await _problemService.GetProblemsByTagAsync(tagName);
        var problemDtos = _mapper.Map<List<ProblemResponse>>(problems);

        return Ok(ApiResponse<List<ProblemResponse>>.SuccessResponse(problemDtos));
    }

    #endregion

    #region ProblemLanguage APIs
    
    /// <summary>
    /// Lấy danh sách ngôn ngữ lập trình được dùng cho một bài toán
    /// </summary>
    [RequireRole("teacher,admin")]
    [HttpGet("{problemId:guid}/available-languages")]
    [ProducesResponseType(typeof(ApiResponse<List<ProblemLanguageDto>>), 200)]
    public async Task<IActionResult> GetAvailableLanguagesForProblem(Guid problemId)
    {
        var userId = GetAuthenticatedUserId();
        await VerifyOwnershipLightweightAsync(problemId, userId);

        // Get existing problem language overrides
        var existingOverrides = await _problemService.GetProblemLanguagesAsync(problemId);

        var result = new List<ProblemLanguageDto>();
        foreach (var pl in existingOverrides){
            result.Add(_mapper.Map<ProblemLanguageDto>(pl));
        }

        return Ok(ApiResponse<List<ProblemLanguageDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Add or update language configurations for a problem (batch operation)
    /// </summary>
    [RequireRole("teacher,admin")]
    [HttpPost("{problemId:guid}/languages")]
    [ProducesResponseType(typeof(ApiResponse<List<ProblemLanguageDto>>), 200)]
    public async Task<IActionResult> AddOrUpdateProblemLanguages(Guid problemId, [FromBody] List<ProblemLanguageDto> requests)
    {
        var userId = GetAuthenticatedUserId();
        await VerifyOwnershipLightweightAsync(problemId, userId);

        // Ensure all ProblemIds match route parameter
        foreach (var request in requests)
        {
            request.ProblemId = problemId;
        }

        var languages = await _problemService.AddOrUpdateProblemLanguagesAsync(problemId, requests);
        var languageDtos = _mapper.Map<List<ProblemLanguageDto>>(languages);
        
        return Ok(ApiResponse<List<ProblemLanguageDto>>.SuccessResponse(languageDtos, "Language configurations saved successfully"));
    }

    /// <summary>
    /// Delete a language override for a problem
    /// </summary>
    [RequireRole("teacher,admin")]
    [HttpDelete("{problemId:guid}/languages/{languageId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<IActionResult> DeleteProblemLanguage(Guid problemId, Guid languageId)
    {
        var userId = GetAuthenticatedUserId();
        await VerifyOwnershipLightweightAsync(problemId, userId);

        var result = await _problemService.DeleteProblemLanguageAsync(problemId, languageId);
        
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Language configuration deleted successfully"));
    }

    #endregion

    #region Search & Statistics

    /// <summary>
    /// Search problems with filters
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ProblemResponse>>), 200)]
    public async Task<IActionResult> SearchProblems(
        [FromQuery] string? keyword,
        [FromQuery] string? difficulty,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1)
            throw new ApiException("Page must be greater than 0");
        
        if (pageSize < 1 || pageSize > 100)
            throw new ApiException("Page size must be between 1 and 100");

        var (problems, total) = await _problemService.SearchProblemsAsync(keyword, difficulty, page, pageSize);
        var problemDtos = _mapper.Map<List<ProblemResponse>>(problems);
        
        var pagedResponse = new PagedResponse<ProblemResponse>
        {
            Data = problemDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        };

        return Ok(ApiResponse<PagedResponse<ProblemResponse>>.SuccessResponse(pagedResponse));
    }

    #endregion
}