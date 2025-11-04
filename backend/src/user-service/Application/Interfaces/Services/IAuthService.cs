using UserService.Application.DTOs.Requests;
using UserService.Application.DTOs.Responses;

namespace UserService.Application.Interfaces.Services;

public interface IAuthService
{
    // JWT Authentication
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<bool> LogoutAsync(string refreshToken);
    Task<RefreshTokenResponse?> RefreshTokenAsync(string refreshToken);
    
    // Password Reset (existing)
    Task<PasswordResetResponse> RequestPasswordResetAsync(RequestPasswordResetRequest request);
    Task<bool> VerifyOTPAsync(string email, string otp);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
}

