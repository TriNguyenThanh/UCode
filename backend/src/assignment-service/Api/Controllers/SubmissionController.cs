using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using AssignmentService.Api.Middlewares;
using AssignmentService.Application.DTOs.Common;
using AssignmentService.Application.DTOs.Requests;
using AssignmentService.Application.DTOs.Responses;
using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Domain.Entities;

namespace AssignmentService.Api.Controllers;

/// <summary>
/// Controller for managing code submissions in the ucode.io.vn platform
/// </summary>
[ApiController]
[Route("api/v1/submissions")]
[ValidateUserId]
public class SubmissionController : ControllerBase
{
    private readonly ISubmissionService _submissionService;
    private readonly IMapper _mapper;

    public SubmissionController(ISubmissionService submissionService, IMapper mapper)
    {
        _submissionService = submissionService;
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

    #endregion

    /// <summary>
    /// Get a specific submission by ID
    /// </summary>
    /// <param name="id">The unique identifier of the submission</param>
    /// <returns>Returns the submission details if found</returns>
    /// <response code="200">Submission retrieved successfully</response>
    /// <response code="404">Submission not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SubmissionResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GetSubmission(Guid id)
    {
        var submission = await _submissionService.GetSubmission(id);
        if (submission == null)
            return NotFound(ApiResponse<SubmissionResponse>.ErrorResponse("Submission not found"));

        var response = _mapper.Map<SubmissionResponse>(submission);
        return Ok(ApiResponse<SubmissionResponse>.SuccessResponse(response));
    }

    /// <summary>
    /// Submit code for a problem
    /// </summary>
    /// <param name="request">The submission data including code, language, and problem ID</param>
    /// <returns>Returns the created submission with its unique identifier</returns>
    /// <response code="200">Submission created successfully</response>
    /// <response code="400">Invalid submission data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="422">Validation errors</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("submit-code")]
    [ProducesResponseType(typeof(ApiResponse<CreateSubmissionResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ValidationErrorResponse), 422)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> CreateSubmission([FromBody] SubmissionRequest request)
    {
        var userId = GetAuthenticatedUserId();

        var submission = _mapper.Map<Submission>(request);
        submission.UserId = userId;

        var created = await _submissionService.SubmitCode(submission);
        var response = _mapper.Map<CreateSubmissionResponse>(created);

        return Ok(ApiResponse<CreateSubmissionResponse>.SuccessResponse(response, "Judging in progress"));
    }

    /// <summary>
    /// Run code for a problem
    /// </summary>
    /// <param name="request">The submission data including code, language, and problem ID</param>
    /// <returns>Returns the created submission with its unique identifier</returns>
    /// <response code="200">Submission created successfully</response>
    /// <response code="400">Invalid submission data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="422">Validation errors</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("run-code")]
    [ProducesResponseType(typeof(ApiResponse<CreateSubmissionResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ValidationErrorResponse), 422)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> RunSubmission([FromBody] SubmissionRequest request)
    {
        var userId = GetAuthenticatedUserId();

        var submission = _mapper.Map<Submission>(request);
        submission.UserId = userId;

        var created = await _submissionService.RunCode(submission);
        var response = _mapper.Map<CreateSubmissionResponse>(created);

        return Ok(ApiResponse<CreateSubmissionResponse>.SuccessResponse(response, "Judging in progress"));
    }

    /// <summary>
    /// Get all submissions for the authenticated user with pagination
    /// </summary>
    /// <param name="pageNumber">The page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10)</param>
    /// <returns>Returns a paginated list of user submissions</returns>
    /// <response code="200">Submissions retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("user")]
    [ProducesResponseType(typeof(ApiResponse<List<SubmissionResponse>>), 200)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GetAllUserSubmissions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetAuthenticatedUserId();
        var submissions = await _submissionService.GetAllUserSubmission(userId, pageNumber, pageSize);
        var response = _mapper.Map<List<SubmissionResponse>>(submissions);
        
        return Ok(ApiResponse<List<SubmissionResponse>>.SuccessResponse(response, $"Retrieved {response.Count} submissions"));
    }

    /// <summary>
    /// Get all submissions for a specific problem by the authenticated user
    /// </summary>
    /// <param name="problemId">The unique identifier of the problem</param>
    /// <param name="pageNumber">The page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10)</param>
    /// <returns>Returns a paginated list of submissions for the problem</returns>
    /// <response code="200">Submissions retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("problem/{problemId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<SubmissionResponse>>), 200)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GetAllSubmissionsByProblem(Guid problemId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetAuthenticatedUserId();
        var submissions = await _submissionService.GetAllSubmissionProblem(problemId, userId, pageNumber, pageSize);
        var response = _mapper.Map<List<SubmissionResponse>>(submissions);
        
        return Ok(ApiResponse<List<SubmissionResponse>>.SuccessResponse(response, $"Retrieved {response.Count} submissions for problem"));
    }

    /// <summary>
    /// Get best submissions (leaderboard) for a specific problem in an assignment
    /// </summary>
    /// <param name="assignmentUserId">The unique identifier of the assignment</param>
    /// <param name="problemId">The unique identifier of the problem</param>
    /// <param name="pageNumber">The page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10)</param>
    /// <returns>Returns a paginated list of best submissions</returns>
    /// <response code="200">Best submissions retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("assignment/{assignmentUserId:guid}/problem/{problemId:guid}/best")]
    [ProducesResponseType(typeof(ApiResponse<List<BestSubmissionResponse>>), 200)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GetBestSubmissions(Guid assignmentUserId, Guid problemId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var bestSubmissions = await _submissionService.GetBestSubmissionByProblemId(assignmentUserId, problemId, pageNumber, pageSize);
        var response = _mapper.Map<List<BestSubmissionResponse>>(bestSubmissions);

        return Ok(ApiResponse<List<BestSubmissionResponse>>.SuccessResponse(response, $"Retrieved {response.Count} best submissions"));
    }
    
    public record BestSubmissionRequest(List<Guid> ProblemIds);
    /// <summary>
    /// Get best submissions (leaderboard) for a specific problem in an assignment
    /// </summary>
    /// <param name="assignmentUserId">The unique identifier of the assignment</param>
    /// <param name="request">List of problem IDs</param>
    /// <returns>Returns a paginated list of best submissions</returns>
    /// <response code="200">Best submissions retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("assignment/{assignmentUserId:guid}/problem/list-my-best")]
    [ProducesResponseType(typeof(ApiResponse<List<BestSubmissionResponse>>), 200)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GetMyBestSubmissions(Guid assignmentUserId, [FromBody] BestSubmissionRequest request)
    {
        var problemIds = request.ProblemIds;
        var userId = GetAuthenticatedUserId();
        var bestSubmissions = await _submissionService.GetMyBestSubmissionByAssignment(assignmentUserId, problemIds, userId);
        var response = _mapper.Map<List<BestSubmissionResponse>>(bestSubmissions);

        return Ok(ApiResponse<List<BestSubmissionResponse>>.SuccessResponse(response, $"Retrieved {response.Count} best submissions"));
    }

    /// <summary>
    /// Get the total number of submissions for the authenticated user
    /// </summary>
    /// <returns>Returns the total count of user submissions</returns>
    /// <response code="200">Count retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("user/count")]
    [ProducesResponseType(typeof(ApiResponse<int>), 200)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GetUserSubmissionCount()
    {
        var userId = GetAuthenticatedUserId();
        var count = await _submissionService.GetNumberOfSubmission(userId);
        
        return Ok(ApiResponse<int>.SuccessResponse(count, "Submission count retrieved successfully"));
    }

    /// <summary>
    /// Get the number of submissions for a specific problem by the authenticated user
    /// </summary>
    /// <param name="assignmentId">The unique identifier of the assignment</param>
    /// <param name="problemId">The unique identifier of the problem</param>
    /// <returns>Returns the count of submissions for the problem</returns>
    /// <response code="200">Count retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("assignment/{assignmentId:guid}/problem/{problemId:guid}/count")]
    [ProducesResponseType(typeof(ApiResponse<int>), 200)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GetSubmissionCountPerProblem(Guid assignmentId, Guid problemId)
    {
        var userId = GetAuthenticatedUserId();
        var count = await _submissionService.GetNumberOfSubmissionPerProblemId(assignmentId, problemId, userId);
        
        return Ok(ApiResponse<int>.SuccessResponse(count, "Problem submission count retrieved successfully"));
    }

    /// <summary>
    /// Get a specific best submission by submission ID
    /// </summary>
    /// <param name="assignmentUserId">The unique identifier of the assignment</param>
    /// <param name="problemId">The unique identifier of the problem</param>
    /// <param name="submissionId">The unique identifier of the submission</param>
    /// <returns>Returns the best submission details if found</returns>
    /// <response code="200">Best submission retrieved successfully</response>
    /// <response code="404">Best submission not found</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("assignment/{assignmentUserId:guid}/problem/{problemId:guid}/best/{submissionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<BestSubmissionResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GetBestSubmission(Guid assignmentUserId, Guid problemId, Guid submissionId)
    {
        var bestSubmission = await _submissionService.GetBestSubmission(assignmentUserId, problemId, submissionId);
        
        if (bestSubmission == null)
            return NotFound(ApiResponse<BestSubmissionResponse>.ErrorResponse("Best submission not found"));

        var response = _mapper.Map<BestSubmissionResponse>(bestSubmission);
        return Ok(ApiResponse<BestSubmissionResponse>.SuccessResponse(response, "Best submission retrieved successfully"));
    }
}