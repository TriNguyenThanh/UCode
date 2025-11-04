using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using UserService.Application.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace UserService.Api.Controllers;

/// <summary>
/// Controller quản lý xác thực người dùng
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Đăng nhập với email/username và password
    /// </summary>
    /// <param name="request">Thông tin đăng nhập</param>
    /// <returns>Access token và refresh token</returns>
    /// <response code="200">Đăng nhập thành công</response>
    /// <response code="401">Thông tin đăng nhập không hợp lệ</response>
    [HttpPost("login")]
    [SwaggerOperation(Summary = "Đăng nhập", Description = "Đăng nhập với email/username và password để nhận JWT token")]
    [SwaggerResponse(200, "Đăng nhập thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(401, "Thông tin đăng nhập không hợp lệ", typeof(ApiResponse<object>))]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

        var result = await _authService.LoginAsync(request);
        if (result == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid credentials"));

        return Ok(ApiResponse<object>.SuccessResponse(result, "Login successful"));
    }

    /// <summary>
    /// Đăng xuất và thu hồi refresh token
    /// </summary>
    /// <param name="request">Refresh token để đăng xuất</param>
    /// <returns>Thông báo đăng xuất thành công</returns>
    /// <response code="200">Đăng xuất thành công</response>
    /// <response code="400">Không thể đăng xuất</response>
    [HttpPost("logout")]
    [SwaggerOperation(Summary = "Đăng xuất", Description = "Đăng xuất và thu hồi refresh token")]
    [SwaggerResponse(200, "Đăng xuất thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Không thể đăng xuất", typeof(ApiResponse<object>))]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

        var result = await _authService.LogoutAsync(request.RefreshToken ?? "");
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to logout"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Logout successful"));
    }

    /// <summary>
    /// Làm mới access token bằng refresh token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>Access token mới</returns>
    /// <response code="200">Token được làm mới thành công</response>
    /// <response code="401">Refresh token không hợp lệ hoặc đã hết hạn</response>
    [HttpPost("refresh-token")]
    [SwaggerOperation(Summary = "Làm mới token", Description = "Làm mới access token bằng refresh token")]
    [SwaggerResponse(200, "Token được làm mới thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(401, "Refresh token không hợp lệ hoặc đã hết hạn", typeof(ApiResponse<object>))]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        if (result == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid or expired refresh token"));

        return Ok(ApiResponse<object>.SuccessResponse(result, "Token refreshed successfully"));
    }

    /// <summary>
    /// Yêu cầu reset password
    /// </summary>
    /// <param name="request">Email của người dùng</param>
    /// <returns>Thông báo gửi OTP</returns>
    /// <response code="200">OTP đã được gửi</response>
    /// <response code="400">Email không hợp lệ</response>
    [HttpPost("request-reset-password")]
    [SwaggerOperation(Summary = "Yêu cầu reset password", Description = "Gửi OTP qua email để reset password")]
    [SwaggerResponse(200, "OTP đã được gửi", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Email không hợp lệ", typeof(ApiResponse<object>))]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

        var result = await _authService.RequestPasswordResetAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(result, result.Message));
    }

    /// <summary>
    /// Xác thực OTP
    /// </summary>
    /// <param name="request">Email và OTP</param>
    /// <returns>Trạng thái xác thực</returns>
    /// <response code="200">OTP hợp lệ</response>
    /// <response code="400">OTP không hợp lệ hoặc đã hết hạn</response>
    [HttpPost("verify-otp")]
    [SwaggerOperation(Summary = "Xác thực OTP", Description = "Xác thực mã OTP đã gửi qua email")]
    [SwaggerResponse(200, "OTP hợp lệ", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "OTP không hợp lệ hoặc đã hết hạn", typeof(ApiResponse<object>))]
    public async Task<IActionResult> VerifyOTP([FromBody] VerifyOTPRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

        var isValid = await _authService.VerifyOTPAsync(request.Email, request.OTP);
        
        if (!isValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid or expired OTP"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "OTP verified successfully"));
    }

    /// <summary>
    /// Reset password với OTP
    /// </summary>
    /// <param name="request">Email, OTP và password mới</param>
    /// <returns>Trạng thái reset password</returns>
    /// <response code="200">Password đã được reset thành công</response>
    /// <response code="400">Không thể reset password</response>
    [HttpPost("reset-password")]
    [SwaggerOperation(Summary = "Reset password", Description = "Đặt lại password mới với OTP đã xác thực")]
    [SwaggerResponse(200, "Password đã được reset thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Không thể reset password", typeof(ApiResponse<object>))]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

        var result = await _authService.ResetPasswordAsync(request);
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to reset password"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Password reset successfully"));
    }
}

// Helper DTO for VerifyOTP endpoint
public class VerifyOTPRequest
{
    public string Email { get; set; } = string.Empty;
    public string OTP { get; set; } = string.Empty;
}

