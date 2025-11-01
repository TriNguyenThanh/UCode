using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using AssignmentService.Application.DTOs;
using AssignmentService.Application.Interfaces;
using AssignmentService.Application.DTOs.Responses;
using AssignmentService.Application.DTOs.Requests;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using AssignmentService.Domain.Entities;
using AssignmentService.Common.Exceptions;
using Swashbuckle.AspNetCore.Annotations;

namespace AssignmentService.Api.Controllers;

[ApiController]
[Route("api/v1/submission")]
public class SubmissionController : ControllerBase
{
    private readonly ISubmissionAppService _submissionService;
    private readonly IMapper _mapper;

    public SubmissionController(ISubmissionAppService submissionService, IMapper mapper)
    {
        _submissionService = submissionService;
        _mapper = mapper;
    }

    /// <summary>
    /// Get a specific submission by ID
    /// </summary>
    /// <param name="id">The unique identifier of the submission</param>
    /// <returns>Returns the submission details if found, otherwise returns 404</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get submission by ID",
        Description = "Retrieves a specific submission using its unique identifier",
        OperationId = "GetSubmission",
        Tags = new[] { "Submissions" }
    )]
    [SwaggerResponse(200, "Submission found successfully", typeof(ApiResponse<SubmissionResponse>))]
    [SwaggerResponse(404, "Submission not found", typeof(ApiResponse<SubmissionResponse>))]
    [SwaggerResponse(500, "Internal server error", typeof(ApiResponse<string>))]
    public async Task<IActionResult> GetSubmission(string id)
    {
        try
        {
            var submission = await _submissionService.GetSubmission(id);
            if (submission == null)
            {
                return NotFound(ApiResponse<SubmissionResponse>.Failed(404, ExceptionConstraint.NotFound, "submission not found"));
            }
            return Ok(ApiResponse<SubmissionResponse>.Success(_mapper.Map<SubmissionResponse>(submission)));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Failed(500, ExceptionConstraint.InternalServerError, ex.Message));
        }
    }

    /// <summary>
    /// Get all submissions for the current user
    /// </summary>
    /// <param name="pageNumber">Page number for pagination (minimum: 1)</param>
    /// <param name="pageSize">Number of items per page (minimum: 10)</param>
    /// <returns>Returns a paginated list of user submissions</returns>
    [HttpGet("user")]
    [SwaggerOperation(
        Summary = "Get all user submissions",
        Description = "Retrieves all submissions for the authenticated user with pagination support. Requires X-User-Id header.",
        OperationId = "GetAllUserSubmissions",
        Tags = new[] { "Submissions" }
    )]
    [SwaggerResponse(200, "User submissions retrieved successfully", typeof(ApiResponse<List<SubmissionResponse>>))]
    [SwaggerResponse(400, "Invalid pagination parameters", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "No submissions found for user", typeof(ApiResponse<List<SubmissionResponse>>))]
    [SwaggerResponse(500, "Internal server error", typeof(ApiResponse<string>))]
    public async Task<IActionResult> GetAllUserSubmissions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        string user_id = "";
        try
        {
            user_id = Request.Headers["X-User-Id"].ToString();

            if (pageNumber < 1 || pageSize < 10)
            {
                return BadRequest(ApiResponse<Object>.Failed(404, ExceptionConstraint.BadRequest, "invalid pagenumber or pageSize"));
            }
            var submissions = await _submissionService.GetAllUserSubmission(user_id, pageNumber, pageSize);
            if (submissions == null || submissions.Count == 0)
            {
                return NotFound(ApiResponse<List<SubmissionResponse>>.Failed(404, ExceptionConstraint.NotFound, "no submissions found for this user"));
            }
            var submissionResponses = _mapper.Map<List<SubmissionResponse>>(submissions);
            return Ok(ApiResponse<List<SubmissionResponse>>.Success(submissionResponses));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Failed(500, ExceptionConstraint.InternalServerError, ex.Message));
        }
    }

    /// <summary>
    /// Get all submissions for a specific problem by the current user
    /// </summary>
    /// <param name="problemId">The unique identifier of the problem</param>
    /// <param name="pageNumber">Page number for pagination (minimum: 1)</param>
    /// <param name="pageSize">Number of items per page (minimum: 10)</param>
    /// <returns>Returns a paginated list of user submissions for the specified problem</returns>
    [HttpGet("submissions/problem/user")]
    [SwaggerOperation(
        Summary = "Get user submissions by problem ID",
        Description = "Retrieves all submissions for a specific problem by the authenticated user with pagination support. Requires X-User-Id header.",
        OperationId = "GetAllSubmissionsByProblemIdAndUserId",
        Tags = new[] { "Submissions" }
    )]
    [SwaggerResponse(200, "User submissions for problem retrieved successfully", typeof(ApiResponse<List<SubmissionResponse>>))]
    [SwaggerResponse(400, "Invalid pagination parameters", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "No submissions found for user in this problem", typeof(ApiResponse<List<SubmissionResponse>>))]
    [SwaggerResponse(500, "Internal server error", typeof(ApiResponse<string>))]
    public async Task<IActionResult> GetAllSubmissionsByProblemIdAndUserId([FromQuery] string problemId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        string user_id = "";
        try
        {
            user_id = Request.Headers["X-User-Id"].ToString();

            if (pageNumber < 1 || pageSize < 10)
            {
                return BadRequest(ApiResponse<Object>.Failed(404, ExceptionConstraint.BadRequest, "invalid pagenumber or pageSize"));
            }
            var submissions = await _submissionService.GetAllSubmissionByProblemIdAndUserId(problemId, user_id, pageNumber, pageSize);
            if (submissions == null || submissions.Count == 0)
            {
                return NotFound(ApiResponse<List<SubmissionResponse>>.Failed(404, ExceptionConstraint.NotFound, "no submissions found for this user in this problem"));
            }
            var submissionResponses = _mapper.Map<List<SubmissionResponse>>(submissions);
            return Ok(ApiResponse<List<SubmissionResponse>>.Success(submissionResponses));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Failed(500, ExceptionConstraint.InternalServerError, ex.Message));
        }
    }

    /// <summary>
    /// Get the best submissions for a specific problem
    /// </summary>
    /// <param name="assignmentId">The unique identifier of the assignment</param>
    /// <param name="problemId">The unique identifier of the problem</param>
    /// <param name="pageNumber">Page number for pagination (minimum: 1)</param>
    /// <param name="pageSize">Number of items per page (minimum: 10)</param>
    /// <returns>Returns a paginated list of the best submissions for the specified problem</returns>
    [HttpGet("best-submissions")]
    [SwaggerOperation(
        Summary = "Get best submissions by problem ID",
        Description = "Retrieves the best submissions for a specific problem with pagination support. Shows top-performing solutions.",
        OperationId = "GetBestUserSubmissionsByProblemId",
        Tags = new[] { "Submissions" }
    )]
    [SwaggerResponse(200, "Best submissions for problem retrieved successfully", typeof(ApiResponse<List<SubmissionResponse>>))]
    [SwaggerResponse(400, "Invalid pagination parameters", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "No submissions found for this problem", typeof(ApiResponse<List<SubmissionResponse>>))]
    [SwaggerResponse(500, "Internal server error", typeof(ApiResponse<string>))]
    public async Task<IActionResult> GetBestUserSubmissionsByProblemId([FromQuery] string assignmentId, [FromQuery] string problemId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            if (pageNumber < 1 || pageSize < 10)
            {
                return BadRequest(ApiResponse<Object>.Failed(404, ExceptionConstraint.BadRequest, "invalid pagenumber or pageSize"));
            }
            var submissions = await _submissionService.GetBestUserSubmissionByProblemId(assignmentId, problemId, pageNumber, pageSize);
            if (submissions == null || submissions.Count == 0)
            {
                return NotFound(ApiResponse<List<SubmissionResponse>>.Failed(404, ExceptionConstraint.NotFound, "no submissions found for this problem"));
            }
            var submissionResponses = _mapper.Map<List<SubmissionResponse>>(submissions);
            return Ok(ApiResponse<List<SubmissionResponse>>.Success(submissionResponses));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Failed(500, ExceptionConstraint.InternalServerError, ex.Message));
        }
    }

    /// <summary>
    /// Create a new code submission
    /// </summary>
    /// <param name="submissionRequest">The submission data including code, language, and problem ID</param>
    /// <returns>Returns the created submission with its unique identifier</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create new submission",
        Description = "Creates a new code submission for evaluation. Requires X-User-Id header for user identification.",
        OperationId = "CreateSubmission",
        Tags = new[] { "Submissions" }
    )]
    [SwaggerResponse(201, "Submission created successfully", typeof(ApiResponse<CreateSubmissionResponse>))]
    [SwaggerResponse(400, "Invalid submission data or model validation failed", typeof(ApiResponse<string>))]
    [SwaggerResponse(500, "Internal server error or mapping failed", typeof(ApiResponse<string>))]
    public async Task<IActionResult> CreateSubmission([FromBody] SubmissionRequest submissionRequest)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.BadRequest("Invalid submission data or model validation failed"));
            }

            var submission = _mapper.Map<Submission>(submissionRequest);
            if (submission != null)
            {
                string user_id = Request.Headers["X-User-Id"].ToString();

                submission.UserId = user_id;
                submission = await _submissionService.AddSubmission(submission);
                var response = _mapper.Map<CreateSubmissionResponse>(submission);

                return Ok(ApiResponse<CreateSubmissionResponse>.CreateAt(response));
            }
            return StatusCode(500, ApiResponse<Object>.Failed(500, ExceptionConstraint.InternalServerError, "Couldn't mapping this object"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Failed(500, ExceptionConstraint.InternalServerError, ex.Message));
        }
    }

    /// <summary>
    /// Delete a submission by ID
    /// </summary>
    /// <param name="id">The unique identifier of the submission to delete</param>
    /// <returns>Returns success message if deletion was successful</returns>
    [HttpDelete("delete")]
    [SwaggerOperation(
        Summary = "Delete submission",
        Description = "Permanently deletes a submission using its unique identifier",
        OperationId = "DeleteSubmission",
        Tags = new[] { "Submissions" }
    )]
    [SwaggerResponse(200, "Submission deleted successfully", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Invalid submission ID", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "Submission not found", typeof(ApiResponse<object>))]
    [SwaggerResponse(500, "Internal server error", typeof(ApiResponse<string>))]
    public async Task<IActionResult> DeleteSubmission([FromQuery] string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(ApiResponse<object>.BadRequest("Id must not be null or empty"));
            }

            var result = await _submissionService.DeleteSubmission(id);
            if (!result)
            {
                return NotFound(ApiResponse<object>.Failed(404, ExceptionConstraint.NotFound, $"Submission with id {id} not found"));
            }

            return Ok(ApiResponse<object>.Delete($"Delete id {id} successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Failed(500, ExceptionConstraint.InternalServerError, ex.Message));
        }
    }
}