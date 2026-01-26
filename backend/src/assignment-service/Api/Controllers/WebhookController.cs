using Microsoft.AspNetCore.Mvc;
using AssignmentService.Application.DTOs.Common;
using AssignmentService.Application.DTOs.Requests;
using AssignmentService.Application.Interfaces.Services;

namespace AssignmentService.Api.Controllers;

/// <summary>
/// Controller xử lý webhooks từ các service khác
/// </summary>
[ApiController]
[Route("api/v1/webhooks")]
public class WebhookController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        IAssignmentService assignmentService,
        ILogger<WebhookController> logger)
    {
        _assignmentService = assignmentService;
        _logger = logger;
    }

    /// <summary>
    /// [INTERNAL] Sync students to all active assignments of a class
    /// Called by User Service when students are added to a class
    /// </summary>
    /// <param name="classId">The unique identifier of the class</param>
    /// <param name="request">List of student IDs to sync</param>
    /// <returns>Number of AssignmentUsers created</returns>
    /// <response code="200">Students synced successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("sync-students-to-class/{classId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> SyncStudentsToClassAssignments(Guid classId, [FromBody] SyncStudentsRequest request)
    {
        try
        {
            if (request.StudentIds == null || !request.StudentIds.Any())
            {
                _logger.LogWarning("SyncStudentsToClassAssignments called with empty StudentIds for class {ClassId}", classId);
                return BadRequest(ApiResponse<object>.ErrorResponse("StudentIds list cannot be empty"));
            }

            _logger.LogInformation("Syncing {Count} students to class {ClassId} assignments", request.StudentIds.Count, classId);

            var count = await _assignmentService.SyncStudentsToClassAssignmentsAsync(classId, request.StudentIds);

            _logger.LogInformation("Successfully created {Count} assignment users for class {ClassId}", count, classId);

            return Ok(ApiResponse<object>.SuccessResponse(
                new { AssignmentUsersCreated = count },
                $"Synced {request.StudentIds.Count} student(s) to {count} assignment user(s)"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing students to class {ClassId} assignments", classId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse($"Internal server error: {ex.Message}"));
        }
    }

    /// <summary>
    /// [INTERNAL] Delete all assignment users when a user is deleted
    /// Called by User Service when a user account is deleted
    /// </summary>
    /// <param name="userId">The unique identifier of the user to delete</param>
    /// <returns>Success confirmation</returns>
    /// <response code="200">Assignment users deleted successfully</response>
    /// <response code="404">No assignment users found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("sync-delete-user")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> SyncDeleteUserFromClass([FromBody] SyncDeleteUserRequest request)
    {
        try
        {
            _logger.LogInformation("Deleting assignment users for user {UserId} in class {ClassId}", 
                request.UserId, request.ClassId);

            var success = await _assignmentService.DeleteAssignmentUserByUserIdAndClassIdAsync(
                request.UserId, request.ClassId);

            if (!success)
            {
                _logger.LogWarning("No assignment users found for user {UserId} in class {ClassId}", 
                    request.UserId, request.ClassId);
                return NotFound(ApiResponse<object>.ErrorResponse("No assignment users found for the given user ID in this class"));
            }

            _logger.LogInformation("Successfully deleted assignment users for user {UserId} in class {ClassId}", 
                request.UserId, request.ClassId);

            return Ok(ApiResponse<object>.SuccessResponse(
                new { request.UserId, request.ClassId }, 
                "Assignment users deleted for the user in the class"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting assignment users for user {UserId} in class {ClassId}", 
                request.UserId, request.ClassId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse($"Internal server error: {ex.Message}"));
        }
    }
}

public class SyncDeleteUserRequest
{
    public Guid UserId { get; set; }
    public Guid ClassId { get; set; }
}
