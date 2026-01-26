using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Common;
using UserService.Application.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace UserService.Api.Controllers;

/// <summary>
/// Controller xử lý đăng ký và xác thực khuôn mặt
/// </summary>
[ApiController]
[Route("api/v1/face-auth")]
[Authorize]
public class FaceAuthController : ControllerBase
{
    private readonly IFaceServiceClient _faceServiceClient;
    private readonly IFileServiceClient _fileServiceClient;
    private readonly IStudentService _studentService;
    private readonly ILogger<FaceAuthController> _logger;

    public FaceAuthController(
        IFaceServiceClient faceServiceClient,
        IFileServiceClient fileServiceClient,
        IStudentService studentService,
        ILogger<FaceAuthController> logger)
    {
        _faceServiceClient = faceServiceClient;
        _fileServiceClient = fileServiceClient;
        _studentService = studentService;
        _logger = logger;
    }

    /// <summary>
    /// Đăng ký khuôn mặt cho sinh viên hiện tại
    /// </summary>
    /// <param name="request">Ảnh khuôn mặt dạng base64</param>
    /// <returns>Kết quả đăng ký</returns>
    /// <response code="200">Đăng ký thành công</response>
    /// <response code="400">Dữ liệu không hợp lệ hoặc khuôn mặt đã tồn tại</response>
    /// <response code="401">Chưa đăng nhập</response>
    /// <response code="503">Face service không khả dụng</response>
    [HttpPost("register")]
    [SwaggerOperation(
        Summary = "Đăng ký khuôn mặt",
        Description = "Đăng ký khuôn mặt cho sinh viên. Mỗi sinh viên chỉ được đăng ký 1 lần, mỗi khuôn mặt chỉ thuộc về 1 sinh viên."
    )]
    [SwaggerResponse(200, "Đăng ký thành công", typeof(ApiResponse<FaceRegisterResponse>))]
    [SwaggerResponse(400, "Đăng ký thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> RegisterFace([FromBody] FaceRegisterRequest request)
    {
        try
        {
            // Lấy user_id từ JWT token
            var userId = GetUserIdFromToken();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid token"));
            }

            _logger.LogInformation("User {UserId} is registering face", userId);

            // Kiểm tra user có phải student không
            var student = await _studentService.GetStudentByIdAsync(userId);
            if (student == null)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Only students can register face"));
            }

            // Kiểm tra đã đăng ký chưa
            if (student.IsFaceAuth == true)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Face already registered. Please delete first to re-register."));
            }

            // STEP 1: Gọi Face Service để đăng ký
            var faceResult = await _faceServiceClient.RegisterFaceAsync(userId, request.Image);

            if (!faceResult.Success)
            {
                _logger.LogWarning("Face registration failed for user {UserId}: {Message}", userId, faceResult.Message);

                // Kiểm tra nếu khuôn mặt đã thuộc về user khác
                if (faceResult.MatchedUserId != null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        $"This face is already registered for another user",
                        new List<string> { faceResult.MatchedUserId }));
                }

                return BadRequest(ApiResponse<object>.ErrorResponse(faceResult.Message));
            }

            _logger.LogInformation("✅ Step 1/3: Face registered in face-service for user {UserId}", userId);

            var fileName = faceResult.ImageFilename ?? $"{userId}_face.jpg";

            // STEP 2: Upload ảnh lên File Service
            var uploadResult = await _fileServiceClient.UploadFaceImageAsync(userId, request.Image, fileName);

            if (uploadResult == null || !uploadResult.Success)
            {
                _logger.LogError("❌ Step 2/3 FAILED: Image upload failed for user {UserId}: {Error}", 
                    userId, uploadResult?.ErrorMessage ?? "Unknown error");

                // ROLLBACK: Xóa face đã đăng ký
                _logger.LogWarning("🔄 Rolling back: Deleting registered face for user {UserId}", userId);
                await _faceServiceClient.DeleteFaceAsync(userId);

                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Failed to upload face image. Registration rolled back.",
                    new List<string> { uploadResult?.ErrorMessage ?? "Upload failed" }));
            }

            _logger.LogInformation("✅ Step 2/3: Image uploaded to file-service: {Url}", uploadResult.FileUrl);

            // STEP 3: Cập nhật IsFaceAuth = true trong database
            var updateRequest = new Application.DTOs.Requests.UpdateUserRequest
            {
                UserId = Guid.Parse(userId),
                IsFaceAuth = true
            };

            var updateResult = await _studentService.UpdateStudentAsync(userId, updateRequest);

            if (!updateResult)
            {
                _logger.LogError("⚠️ Step 3/3: Failed to update IsFaceAuth for user {UserId}, but face and image are registered", userId);
            }
            else
            {
                _logger.LogInformation("✅ Step 3/3: IsFaceAuth updated for user {UserId}", userId);
            }

            _logger.LogInformation("🎉 Face registration completed successfully for user {UserId}", userId);

            return Ok(ApiResponse<FaceRegisterResponse>.SuccessResponse(
                new FaceRegisterResponse
                {
                    UserId = userId,
                    Confidence = faceResult.Confidence ?? 0,
                    ImageUrl = uploadResult.FileUrl,
                    RegisteredAt = DateTime.UtcNow
                },
                "Face registered successfully"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering face");
            return StatusCode(500, ApiResponse<object>.ErrorResponse($"Internal server error: {ex.Message}"));
        }
    }

    /// <summary>
    /// Xác thực khuôn mặt (1:1 verification)
    /// </summary>
    /// <param name="request">Ảnh khuôn mặt cần xác thực</param>
    /// <returns>Kết quả xác thực</returns>
    [HttpPost("verify")]
    [SwaggerOperation(
        Summary = "Xác thực khuôn mặt",
        Description = "Xác thực khuôn mặt 1:1 với khuôn mặt đã đăng ký của sinh viên"
    )]
    [SwaggerResponse(200, "Xác thực hoàn tất", typeof(ApiResponse<FaceVerifyResponse>))]
    [SwaggerResponse(400, "Dữ liệu không hợp lệ", typeof(ApiResponse<object>))]
    public async Task<IActionResult> VerifyFace([FromBody] FaceVerifyRequest request)
    {
        try
        {
            var userId = GetUserIdFromToken();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid token"));
            }

            _logger.LogInformation("User {UserId} is verifying face", userId);

            // Kiểm tra đã đăng ký face chưa
            var student = await _studentService.GetStudentByIdAsync(userId);
            if (student?.IsFaceAuth != true)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Face not registered. Please register first."));
            }

            // Gọi Face Service để xác thực
            var result = await _faceServiceClient.VerifyFaceAsync(userId, request.Image, request.Threshold ?? 0.6f);

            if (!result.Success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(result.Message));
            }

            return Ok(ApiResponse<FaceVerifyResponse>.SuccessResponse(
                new FaceVerifyResponse
                {
                    UserId = userId,
                    IsMatch = result.IsMatch,
                    Similarity = result.Similarity,
                    Threshold = result.Threshold,
                    VerifiedAt = DateTime.UtcNow
                },
                result.IsMatch ? "Face verified successfully" : "Face verification failed - not a match"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying face");
            return StatusCode(500, ApiResponse<object>.ErrorResponse($"Internal server error: {ex.Message}"));
        }
    }

    /// <summary>
    /// Xóa khuôn mặt đã đăng ký
    /// </summary>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete("delete")]
    // [Authorize(Roles = "Admin")]
    [SwaggerOperation(
        Summary = "Xóa khuôn mặt",
        Description = "Xóa khuôn mặt đã đăng ký. Sau khi xóa có thể đăng ký lại."
    )]
    [SwaggerResponse(200, "Xóa thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Xóa thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> DeleteFace()
    {
        try
        {
            var userId = GetUserIdFromToken();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid token"));
            }

            _logger.LogInformation("User {UserId} is deleting face", userId);

            // Kiểm tra đã đăng ký chưa
            var student = await _studentService.GetStudentByIdAsync(userId);
            if (student?.IsFaceAuth != true)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("No face registered"));
            }

            // Gọi Face Service để xóa
            var deleted = await _faceServiceClient.DeleteFaceAsync(userId);

            if (!deleted)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Failed to delete face from face service"));
            }

            // Cập nhật IsFaceAuth = false trong database
            var updateRequest = new Application.DTOs.Requests.UpdateUserRequest
            {
                UserId = Guid.Parse(userId),
                IsFaceAuth = false
            };

            await _studentService.UpdateStudentAsync(userId, updateRequest);

            _logger.LogInformation("Face deleted successfully for user {UserId}", userId);

            return Ok(ApiResponse<object>.SuccessResponse(
                new { userId, deletedAt = DateTime.UtcNow },
                "Face deleted successfully"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting face");
            return StatusCode(500, ApiResponse<object>.ErrorResponse($"Internal server error: {ex.Message}"));
        }
    }

    /// <summary>
    /// Kiểm tra trạng thái Face Service
    /// </summary>
    [HttpGet("health")]
    [SwaggerOperation(
        Summary = "Health check Face Service",
        Description = "Kiểm tra trạng thái của Face Recognition Service"
    )]
    public async Task<IActionResult> HealthCheck()
    {
        var health = await _faceServiceClient.HealthCheckAsync();

        if (health == null)
        {
            return StatusCode(503, ApiResponse<object>.ErrorResponse("Face service unavailable"));
        }

        return Ok(ApiResponse<FaceServiceHealthResult>.SuccessResponse(health, "Face service is healthy"));
    }

    /// <summary>
    /// Lấy User ID từ JWT token
    /// </summary>
    private string? GetUserIdFromToken()
    {
        // Lấy từ header X-User-Id (được API Gateway inject từ claims)
        var userId = Request.Headers["X-User-Id"].FirstOrDefault();

        if (string.IsNullOrEmpty(userId))
        {
            // Fallback: lấy từ claims
            userId = User.FindFirst("sub")?.Value 
                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        return userId;
    }
}

