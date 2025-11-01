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
    private readonly IMapper _mapper;
    
    public ProblemController(IProblemService problemService, IMapper mapper)
    {
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
    [RequireRole("teacher")]
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
            request.Code,
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

        return Ok(ApiResponse<ProblemResponse>.SuccessResponse(_mapper.Map<ProblemResponse>(problem), "Problem retrieved successfully"));
    }

    /// <summary>
    /// Retrieves all problems created by the current teacher
    /// </summary>
    /// <returns>List of problems owned by the current teacher</returns>
    /// <response code="200">Problems retrieved successfully</response>
    /// <response code="404">No problems found for this owner</response>
    /// <response code="401">Unauthorized - Teacher role required</response>
    /// <response code="400">Bad request - Missing user ID header</response>
    [RequireRole("teacher")]
    [HttpGet("all-problems")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProblemResponse>>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    public async Task<IActionResult> GetProblemsByOwner()
    {
        var ownerId = GetAuthenticatedUserId();

        var problems = await _problemService.GetProblemsByOwnerIdAsync(ownerId);
        if (problems == null || !problems.Any())
        {
            problems = new List<Problem>();
        }

        return Ok(ApiResponse<IEnumerable<ProblemResponse>>.SuccessResponse(_mapper.Map<IEnumerable<ProblemResponse>>(problems)));
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
    [RequireRole("teacher")]
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
    [RequireRole("teacher")]
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
    [RequireRole("teacher")]
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
}