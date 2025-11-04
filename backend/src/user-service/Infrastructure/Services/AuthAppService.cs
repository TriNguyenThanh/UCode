using Microsoft.Extensions.Caching.Memory;
using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using UserService.Application.DTOs.Responses;
using UserService.Application.Interfaces.Repositories;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Infrastructure.Services;

public class AuthAppService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IEmailService _emailService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMemoryCache _cache;
    private const int OTP_EXPIRATION_MINUTES = 10;
    private const int OTP_LENGTH = 6;

    public AuthAppService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IEmailService emailService,
        IJwtTokenService jwtTokenService,
        IMemoryCache cache)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _emailService = emailService;
        _jwtTokenService = jwtTokenService;
        _cache = cache;
    }

    // JWT Authentication Methods
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        // Find user by email or username
        var user = await _userRepository.GetByEmailAsync(request.EmailOrUsername) ??
                   await _userRepository.GetByUsernameAsync(request.EmailOrUsername);

        if (user == null)
            return null;

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        // Check if user is active
        if (user.Status != UserStatus.Active)
            return null;

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Calculate expiry
        var refreshTokenExpiryDays = request.RememberMe ? 30 : 7; // Longer expiry if remember me
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(refreshTokenExpiryDays);

        // Save refresh token to database
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.UserId,
            ExpiresAt = refreshTokenExpiry,
            CreatedAt = DateTime.UtcNow,
            Status = RefreshTokenStatus.Active
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity);

        // Update last login time
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Cleanup old refresh tokens (keep only last 5)
        await CleanupOldRefreshTokens(user.UserId);

        return _jwtTokenService.CreateLoginResponse(user, accessToken, refreshToken);
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            return false;

        await _refreshTokenRepository.RevokeTokenAsync(refreshToken, "User logout");
        return true;
    }

    public async Task<RefreshTokenResponse?> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            return null;

        var tokenEntity = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (tokenEntity == null || !tokenEntity.IsActive)
            return null;

        // Get user
        var user = await _userRepository.GetByIdAsync(tokenEntity.UserId);
        if (user == null || user.Status != UserStatus.Active)
            return null;

        // Revoke old token
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        await _refreshTokenRepository.RevokeTokenAsync(refreshToken, "Token refresh", newRefreshToken);

        // Create new refresh token
        var newRefreshTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.UserId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            Status = RefreshTokenStatus.Active
        };

        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);

        // Generate new access token
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);

        return _jwtTokenService.CreateRefreshTokenResponse(newAccessToken, newRefreshToken);
    }

    private async Task CleanupOldRefreshTokens(Guid userId)
    {
        var userTokens = await _refreshTokenRepository.GetByUserIdAsync(userId);
        var activeTokens = userTokens.Where(t => t.IsActive).OrderByDescending(t => t.CreatedAt).ToList();

        // Keep only the 5 most recent tokens
        if (activeTokens.Count > 5)
        {
            var tokensToRevoke = activeTokens.Skip(5);
            foreach (var token in tokensToRevoke)
            {
                await _refreshTokenRepository.RevokeTokenAsync(token.Token, "Too many active tokens");
            }
        }
    }

    
    public async Task<PasswordResetResponse> RequestPasswordResetAsync(RequestPasswordResetRequest request)
    {
        // Check if user exists
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            // Don't reveal if email exists or not for security
            return new PasswordResetResponse
            {
                Success = true,
                Message = "If the email exists, an OTP has been sent to it.",
                ExpiresAt = DateTime.UtcNow.AddMinutes(OTP_EXPIRATION_MINUTES)
            };
        }

        // Generate OTP
        var otp = GenerateOTP();
        var expiresAt = DateTime.UtcNow.AddMinutes(OTP_EXPIRATION_MINUTES);

        // Store OTP in cache
        var cacheKey = $"OTP_{request.Email}";
        _cache.Set(cacheKey, otp, TimeSpan.FromMinutes(OTP_EXPIRATION_MINUTES));

        // Send OTP via email
        await _emailService.SendOTPEmailAsync(request.Email, otp);

        return new PasswordResetResponse
        {
            Success = true,
            Message = "OTP has been sent to your email.",
            ExpiresAt = expiresAt
        };
    }

    public async Task<bool> VerifyOTPAsync(string email, string otp)
    {
        var cacheKey = $"OTP_{email}";
        
        if (!_cache.TryGetValue(cacheKey, out string? cachedOtp))
            return false;

        return cachedOtp == otp;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        // Verify OTP
        if (!await VerifyOTPAsync(request.Email, request.OTP))
            throw new ApiException("Invalid or expired OTP");

        // Get user
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            throw new ApiException("User not found", 404);

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userRepository.UpdateAsync(user);

        if (result)
        {
            // Remove OTP from cache after successful password reset
            var cacheKey = $"OTP_{request.Email}";
            _cache.Remove(cacheKey);
        }

        return result;
    }

    private string GenerateOTP()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}

