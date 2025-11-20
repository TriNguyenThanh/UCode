using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using Swashbuckle.AspNetCore.Annotations;

namespace UserService.Api.Controllers;

/// <summary>
/// Controller quản lý điểm danh
/// </summary>
[ApiController]
[Route("api/v1/attendance")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    /// <summary>
    /// Helper method to extract UserId from JWT token
    /// </summary>
    private string GetUserIdFromToken()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("X-User-Id")?.Value
        
            ?? throw new UnauthorizedAccessException("User ID not found in token");
    }

    private Guid GetUserIdFromTokenAsGuid()
    {
        var userId = GetUserIdFromToken();
        if (!Guid.TryParse(userId, out var userIdGuid))
            throw new UnauthorizedAccessException("Invalid User ID format in token");

        return userIdGuid;
    }

    #region AttendanceSession Actions

    /// <summary>
    /// [USER] Get sessions for the current user
    /// </summary>
    [HttpGet("sessions")]
    [Authorize]
    [SwaggerOperation(Summary = "[USER] Get sessions", Description = "Retrieve attendance sessions for the current user with pagination")]
    public async Task<IActionResult> GetSessions(Guid classId,[FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _attendanceService.GetSessionsAsync(classId, pageNumber, pageSize);
        if (!response.Success)
            return NotFound(ApiResponse<string>.ErrorResponse("No sessions found"));
        return Ok(response);
    }

    /// <summary>
    /// [Teacher] Create a new attendance session
    /// </summary>
    [HttpPost("create-session")]
    [Authorize]
    [SwaggerOperation(Summary = "[Teacher] Create session", Description = "Create a new attendance session")]
    public async Task<IActionResult> CreateSession([FromBody] AttendanceSessionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

            // Ensure the Id is not set for new sessions
            request.Id = null;
            var response = await _attendanceService.CreateSessionAsync(request);
            return CreatedAtAction(nameof(GetSessionById), new { id = response.Data?.Id }, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred: " + ex.Message));
        }
    }

    /// <summary>
    /// [Teacher] Update an attendance session
    /// </summary>
    [HttpPut("session")]
    [Authorize]
    [SwaggerOperation(Summary = "[Teacher] Update session", Description = "Update an existing attendance session")]
    public async Task<IActionResult> UpdateSession([FromBody] AttendanceSessionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

            if (request.Id == null)
                return BadRequest(ApiResponse<object>.ErrorResponse("Session ID is required"));

            var session = await _attendanceService.UpdateSessionAsync(request);
            return Ok(session);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred: " + ex.Message));
        }
    }

    /// <summary>
    /// [Teacher] Delete an attendance session
    /// </summary>
    [HttpDelete("sessions/{id}")]
    [Authorize]
    [SwaggerOperation(Summary = "[Teacher] Delete session", Description = "Delete an attendance session")]
    public async Task<IActionResult> DeleteSession(Guid id)
    {
        var result = await _attendanceService.DeleteSessionAsync(id);
        if (!result.Data)
            return NotFound(ApiResponse<string>.ErrorResponse("Session not found"));

        return Ok(ApiResponse<string>.SuccessResponse(string.Empty, "Session deleted successfully"));
    }

    /// <summary>
    /// [USER] Get session by ID
    /// </summary>
    [HttpGet("session/{id}")]
    [Authorize]
    [SwaggerOperation(Summary = "[USER] Get session by ID", Description = "Retrieve a specific attendance session by its ID")]
    public async Task<IActionResult> GetSessionById(Guid id)
    {
        var session = await _attendanceService.GetSessionByIdAsync(id);
        if (session.Data == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Session not found"));

        return Ok(session);
    }

    #endregion

    #region AttendanceRecord Actions

    /// <summary>
    /// [USER] Check-in for attendance
    /// </summary>
    [HttpPost("records/check-in")]
    [Authorize]
    [SwaggerOperation(Summary = "[USER] Check-in", Description = "Create a new attendance record for check-in")]
    public async Task<IActionResult> CheckIn([FromBody] AttendanceRecordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));
        request.UserId = GetUserIdFromTokenAsGuid();
        var record = await _attendanceService.CheckInAsync(request);
        return Ok(record);
    }

    /// <summary>
    /// [TEACHER] Get attendance records by session
    /// </summary>
    [HttpGet("records/by-session/{sessionId}")]
    [Authorize]
    [SwaggerOperation(Summary = "[TEACHER] Get records by session", Description = "Retrieve attendance records for a specific session")]
    public async Task<IActionResult> GetRecordsBySession(Guid sessionId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var records = await _attendanceService.GetRecordsBySessionAsync(sessionId, pageNumber, pageSize);
        return Ok(records);
    }

    #endregion
}