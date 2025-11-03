using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using UserService.Application.DTOs.Responses;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Application.Interfaces.Services;

/// <summary>
/// Interface cho JWT Token Service
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Tạo access token cho user
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Tạo refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validate access token
    /// </summary>
    ClaimsPrincipal? ValidateAccessToken(string token);

    /// <summary>
    /// Tạo response đăng nhập với tokens
    /// </summary>
    LoginResponse CreateLoginResponse(User user, string accessToken, string refreshToken);

    /// <summary>
    /// Tạo response refresh token
    /// </summary>
    RefreshTokenResponse CreateRefreshTokenResponse(string accessToken, string refreshToken);
}

/// <summary>
/// JWT Token Service implementation
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpiryMinutes;
    private readonly int _refreshTokenExpiryDays;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        _secretKey = _configuration["JwtSettings:SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
        _issuer = _configuration["JwtSettings:Issuer"] ?? "UserService";
        _audience = _configuration["JwtSettings:Audience"] ?? "UserServiceClient";
        _accessTokenExpiryMinutes = int.Parse(_configuration["JwtSettings:AccessTokenExpiryMinutes"] ?? "60");
        _refreshTokenExpiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");
    }

    public string GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username),
            // new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            // new("fullName", user.FullName),
            // new("status", user.Status.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Thêm claims đặc biệt cho từng role
        switch (user.Role)
        {
            case UserRole.Student:
                var student = user as Student;
                if (student != null)
                {
                    claims.Add(new Claim("studentCode", student.StudentCode));
                    // claims.Add(new Claim("major", student.Major));
                    // claims.Add(new Claim("classYear", student.ClassYear.ToString()));
                    // claims.Add(new Claim("enrollmentYear", student.EnrollmentYear.ToString()));
                }
                break;
            case UserRole.Teacher:
                var teacher = user as Teacher;
                if (teacher != null)
                {
                    claims.Add(new Claim("teacherCode", teacher.TeacherCode));
                    // claims.Add(new Claim("department", teacher.Department));
                    // claims.Add(new Claim("title", teacher.Title ?? ""));
                    // claims.Add(new Claim("phone", teacher.Phone ?? ""));
                }
                break;
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public LoginResponse CreateLoginResponse(User user, string accessToken, string refreshToken)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_accessTokenExpiryMinutes).ToUnixTimeSeconds();

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = new UserResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt
            }
        };
    }

    public RefreshTokenResponse CreateRefreshTokenResponse(string accessToken, string refreshToken)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_accessTokenExpiryMinutes).ToUnixTimeSeconds();

        return new RefreshTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt
        };
    }
}
