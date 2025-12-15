using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using Swashbuckle.AspNetCore.Annotations;
using UserService.Application.Interfaces.Services;

namespace UserService.Api.Controllers;

/// <summary>
/// Controller quản lý điểm danh
/// </summary>
[ApiController]
[Route("api/v1/attendance")]
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

    /// <summary>
    /// [USER] Get sessions for the current user
    /// </summary>
    /// <param name="classId">ID của lớp học</param>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Số lượng mỗi trang</param>
    /// <returns>Danh sách phiên điểm danh</returns>
    /// <response code="200">Trả về danh sách phiên điểm danh</response>
    /// <response code="404">Không tìm thấy phiên điểm danh</response>
    [HttpGet("sessions")]
    [SwaggerOperation(Summary = "[USER] Get sessions", Description = "Retrieve attendance sessions for the current user with pagination")]
    [SwaggerResponse(200, "Success", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "No sessions found", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetSessions(Guid classId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _attendanceService.GetSessionsAsync(classId, pageNumber, pageSize);
        if (!response.Success)
            return NotFound(ApiResponse<string>.ErrorResponse("No sessions found"));
        return Ok(response);
    }

    /// <summary>
    /// [Teacher] Create a new attendance session
    /// </summary>
    /// <param name="request">Thông tin phiên điểm danh mới</param>
    /// <returns>Phiên điểm danh vừa tạo</returns>
    /// <response code="201">Tạo thành công</response>
    /// <response code="400">Dữ liệu không hợp lệ</response>
    /// <response code="500">Lỗi server</response>
    [HttpPost("create-session")]
    [Authorize(Roles = "Teacher,Admin")]
    [SwaggerOperation(Summary = "[Teacher] Create session", Description = "Create a new attendance session")]
    [SwaggerResponse(201, "Created successfully", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Invalid request", typeof(ApiResponse<object>))]
    [SwaggerResponse(500, "Server error", typeof(ApiResponse<object>))]
    public async Task<IActionResult> CreateSession([FromBody] AttendanceSessionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

            // Ensure the Id is not set for new sessions
            request.Id = null;
            var response = await _attendanceService.CreateSessionAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An unexpected error occurred: " + ex.Message));
        }
    }

    /// <summary>
    /// [Teacher] Update an attendance session
    /// </summary>
    /// <param name="request">Thông tin cập nhật phiên điểm danh</param>
    /// <returns>Phiên điểm danh đã cập nhật</returns>
    /// <response code="200">Cập nhật thành công</response>
    /// <response code="400">Dữ liệu không hợp lệ</response>
    /// <response code="500">Lỗi server</response>
    [HttpPut("session")]
    [Authorize(Roles = "Teacher,Admin")]
    [SwaggerOperation(Summary = "[Teacher] Update session", Description = "Update an existing attendance session")]
    [SwaggerResponse(200, "Updated successfully", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Invalid request", typeof(ApiResponse<object>))]
    [SwaggerResponse(500, "Server error", typeof(ApiResponse<object>))]
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
    /// <param name="id">ID của phiên điểm danh</param>
    /// <returns>Trạng thái xóa</returns>
    /// <response code="200">Xóa thành công</response>
    /// <response code="404">Không tìm thấy phiên điểm danh</response>
    [HttpDelete("sessions/{id}")]
    [Authorize(Roles = "Teacher,Admin")]
    [SwaggerOperation(Summary = "[Teacher] Delete session", Description = "Delete an attendance session")]
    [SwaggerResponse(200, "Deleted successfully", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "Session not found", typeof(ApiResponse<object>))]
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
    /// <param name="id">ID của phiên điểm danh</param>
    /// <returns>Thông tin phiên điểm danh</returns>
    /// <response code="200">Trả về thông tin phiên điểm danh</response>
    /// <response code="404">Không tìm thấy phiên điểm danh</response>
    [HttpGet("session/{id}")]
    [SwaggerOperation(Summary = "[USER] Get session by ID", Description = "Retrieve a specific attendance session by its ID")]
    [SwaggerResponse(200, "Success", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "Session not found", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetSessionById(Guid id)
    {
        var session = await _attendanceService.GetSessionByIdAsync(id);
        if (session.Data == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Session not found"));

        return Ok(session);
    }
    /// <summary>
    /// Lấy thông tin Session theo code
    /// </summary>
    /// <param name="code">Mã code của phiên điểm danh</param>
    [HttpGet("session-by-code/{code}")]
    [SwaggerOperation(Summary = "[Teacher] Get session by code", Description = "Retrieve a specific attendance session by its code")]
    [SwaggerResponse(200, "Success", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetSessionByCode(string code)
    {
        var session = await _attendanceService.GetSessionByCodeAsync(code);
        if (session.Data == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Session not found"));

        return Ok(session);
    }


    /// <summary>
    /// [USER] Check-in for attendance
    /// </summary>
    /// <param name="request">Thông tin check-in</param>
    /// <returns>Kết quả check-in</returns>
    /// <response code="200">Check-in thành công</response>
    /// <response code="400">Dữ liệu không hợp lệ</response>
    [HttpPost("records/check-in")]
    [SwaggerOperation(Summary = "[USER] Check-in", Description = "Create a new attendance record for check-in")]
    [SwaggerResponse(200, "Check-in successful", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Invalid request", typeof(ApiResponse<object>))]
    public async Task<IActionResult> CheckIn([FromBody] AttendanceRecordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));
        request.UserId = GetUserIdFromTokenAsGuid();

        // publish thì bỏ cmt nhé
        // request.IpAddress = HttpContext.Items["ClientIp"] as string ?? HttpContext.Request.Headers["X-Client-IP"].FirstOrDefault();

        var record = await _attendanceService.CheckInAsync(request);
        return Ok(record);
    }

    /// <summary>
    /// [TEACHER] Get attendance records by session
    /// </summary>
    /// <param name="sessionId">ID của phiên điểm danh</param>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Số lượng mỗi trang</param>
    /// <returns>Danh sách bản ghi điểm danh</returns>
    /// <response code="200">Trả về danh sách bản ghi</response>
    [HttpGet("records/by-session/{sessionId}")]
    [Authorize(Roles = "Teacher,Admin")]
    [SwaggerOperation(Summary = "[TEACHER] Get records by session", Description = "Retrieve attendance records for a specific session")]
    [SwaggerResponse(200, "Success", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetRecordsBySession(Guid sessionId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var records = await _attendanceService.GetRecordsBySessionAsync(sessionId, pageNumber, pageSize);
        return Ok(records);
    }


    /// <summary>
    /// [USER] Check-in for attendance
    /// </summary>
    /// <param name="sessionId">ID của phiên điểm danh</param>
    /// <returns>Kết quả check-in</returns>
    /// <response code="200">Check-in thành công</response>
    /// <response code="400">Dữ liệu không hợp lệ</response>
    [HttpGet("session/{sessionId}/status")]
    [SwaggerOperation(Summary = "[USER] Check-in", Description = "Create a new attendance record for check-in")]
    [SwaggerResponse(200, "Attendance status retrieved successfully", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Invalid session ID", typeof(ApiResponse<object>))]
    public async Task<IActionResult> CheckAttendanceStatus(Guid sessionId)
    {
        var userId = GetUserIdFromTokenAsGuid();
        var record = await _attendanceService.CheckAttendanceStatusAsync(sessionId, userId);
        return Ok(record);
    }
}