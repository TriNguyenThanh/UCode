using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Common;
using UserService.Application.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace UserService.Api.Controllers;

/// <summary>
/// Controller xử lý webhooks từ các service khác
/// </summary>
[ApiController]
[Route("api/v1/webhooks")]
public class WebhookController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly IUserService _userService;
    private readonly IClassService _classService;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        IStudentService studentService, 
        IUserService userService,
        IClassService classService,
        ILogger<WebhookController> logger)
    {
        _studentService = studentService;
        _userService = userService;
        _classService = classService;
        _logger = logger;
    }

    /// <summary>
    /// [INTERNAL] Lấy emails theo danh sách User IDs
    /// </summary>
    /// <param name="request">Danh sách User IDs</param>
    /// <returns>Danh sách emails</returns>
    /// <response code="200">Trả về danh sách emails</response>
    /// <response code="400">Yêu cầu không hợp lệ</response>
    [HttpPost("get-emails")]
    [SwaggerOperation(
        Summary = "[INTERNAL] Lấy emails theo User IDs",
        Description = "Internal API để các service khác lấy danh sách email. Yêu cầu X-Internal-Api-Key header."
    )]
    [SwaggerResponse(200, "Danh sách emails", typeof(ApiResponse<UserEmailResponse>))]
    [SwaggerResponse(400, "Yêu cầu không hợp lệ", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetEmailsByIds([FromBody] UserIdRequest request)
    {
        try
        {
            if (request.Ids == null || !request.Ids.Any())
            {
                _logger.LogWarning("GetEmailsByIds called with empty or null Ids list");
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request: Ids list is required"));
            }

            _logger.LogInformation("Getting emails for {Count} user IDs", request.Ids.Count);

            var emails = await _userService.GetEmailsByIdsAsync(request.Ids);
            
            _logger.LogInformation("Retrieved {Count} emails", emails.Count);
            
            return Ok(ApiResponse<UserEmailResponse>.SuccessResponse(
                new UserEmailResponse(emails), 
                "Emails retrieved successfully"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting emails by IDs");
            return StatusCode(500, ApiResponse<object>.ErrorResponse($"Internal server error: {ex.Message}"));
        }
    }

    /// <summary>
    /// [INTERNAL] Lấy danh sách User IDs của lớp học
    /// </summary>
    /// <param name="classId">ID lớp học</param>
    /// <returns>Danh sách User IDs (Student objects)</returns>
    /// <response code="200">Trả về danh sách sinh viên</response>
    /// <response code="400">Lỗi khi lấy dữ liệu</response>
    [HttpGet("class/{classId}/students")]
    [SwaggerOperation(
        Summary = "[INTERNAL] Lấy danh sách sinh viên của lớp",
        Description = "Internal API để Assignment Service lấy danh sách sinh viên. Yêu cầu X-Internal-Api-Key header."
    )]
    [SwaggerResponse(200, "Danh sách sinh viên", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Lỗi khi lấy dữ liệu", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetStudentsByClassId(string classId)
    {
        try
        {
            _logger.LogInformation("Getting students for class: {ClassId}", classId);

            var students = await _classService.GetStudentListByClassAsync(classId);
            
            _logger.LogInformation("Retrieved {Count} students for class {ClassId}", students.Count, classId);
            
            return Ok(ApiResponse<object>.SuccessResponse(students, "Student list retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting students for class {ClassId}", classId);
            return BadRequest(ApiResponse<object>.ErrorResponse($"Failed to get students: {ex.Message}"));
        }
    }

    /// <summary>
    /// [INTERNAL] Webhook để face-service cập nhật trạng thái xác thực khuôn mặt
    /// </summary>
    /// <param name="request">Thông tin cập nhật</param>
    /// <returns>Trạng thái cập nhật</returns>
    /// <response code="200">Cập nhật thành công</response>
    /// <response code="400">Dữ liệu không hợp lệ</response>
    /// <response code="404">Không tìm thấy sinh viên</response>
    [HttpPost("face-auth-status")]
    [SwaggerOperation(
        Summary = "[INTERNAL] Cập nhật trạng thái xác thực khuôn mặt",
        Description = "Webhook để face-service gọi khi sinh viên đăng ký/xóa khuôn mặt. Yêu cầu X-Internal-Api-Key header."
    )]
    [SwaggerResponse(200, "Cập nhật thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Dữ liệu không hợp lệ", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "Không tìm thấy sinh viên", typeof(ApiResponse<object>))]
    public async Task<IActionResult> UpdateFaceAuthStatus([FromBody] UpdateFaceAuthRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

            _logger.LogInformation("Received face auth status update for user_id: {UserId}, status: {Status}", 
                request.UserId, request.IsFaceAuth);

            // Tìm student theo user_id
            var student = await _studentService.GetStudentByIdAsync(request.UserId);
            if (student == null)
            {
                _logger.LogWarning("Student not found with user_id: {UserId}", request.UserId);
                return NotFound(ApiResponse<object>.ErrorResponse($"Student not found with user_id: {request.UserId}"));
            }

            // Cập nhật trạng thái face auth
            var updateRequest = new Application.DTOs.Requests.UpdateUserRequest
            {
                UserId = Guid.Parse(request.UserId),
                IsFaceAuth = request.IsFaceAuth
            };

            var result = await _studentService.UpdateStudentAsync(request.UserId, updateRequest);
            
            if (!result)
            {
                _logger.LogError("Failed to update face auth status for user_id: {UserId}", request.UserId);
                return BadRequest(ApiResponse<object>.ErrorResponse("Failed to update face auth status"));
            }

            _logger.LogInformation("Successfully updated face auth status for user_id: {UserId} to {Status}", 
                request.UserId, request.IsFaceAuth);

            return Ok(ApiResponse<object>.SuccessResponse(
                new { 
                    userId = request.UserId, 
                    isFaceAuth = request.IsFaceAuth,
                    updatedAt = DateTime.UtcNow
                }, 
                "Face auth status updated successfully"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating face auth status for user_id: {UserId}", request.UserId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse($"Internal server error: {ex.Message}"));
        }
    }
}

/// <summary>
/// Request để cập nhật trạng thái xác thực khuôn mặt
/// </summary>
public class UpdateFaceAuthRequest
{
    /// <summary>
    /// User ID (GUID string)
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Trạng thái xác thực khuôn mặt (true = đã đăng ký, false = đã xóa)
    /// </summary>
    public bool IsFaceAuth { get; set; }
}

/// <summary>
/// Request để lấy emails theo User IDs
/// </summary>
public class UserIdRequest
{
    public List<Guid> Ids { get; set; } = new();
}

/// <summary>
/// Response chứa danh sách emails
/// </summary>
public class UserEmailResponse
{
    public List<string> Emails { get; set; }

    public UserEmailResponse(List<string> emails)
    {
        Emails = emails;
    }
}
