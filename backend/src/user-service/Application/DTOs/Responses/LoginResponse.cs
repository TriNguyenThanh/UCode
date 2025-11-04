namespace UserService.Application.DTOs.Responses;

/// <summary>
/// DTO cho response đăng nhập thành công
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Access token để xác thực API calls
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token để làm mới access token
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian hết hạn của access token (Unix timestamp)
    /// </summary>
    public long ExpiresAt { get; set; }

    /// <summary>
    /// Thông tin user đã đăng nhập
    /// </summary>
    public UserResponse User { get; set; } = new();
}

/// <summary>
/// DTO cho response làm mới token
/// </summary>
public class RefreshTokenResponse
{
    /// <summary>
    /// Access token mới
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token mới
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian hết hạn của access token mới (Unix timestamp)
    /// </summary>
    public long ExpiresAt { get; set; }
}
