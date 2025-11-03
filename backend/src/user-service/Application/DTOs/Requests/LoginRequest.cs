using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Requests;

/// <summary>
/// DTO cho request đăng nhập
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Email hoặc Username để đăng nhập
    /// </summary>
    [Required(ErrorMessage = "Email or Username is required")]
    public string EmailOrUsername { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Ghi nhớ đăng nhập (tùy chọn)
    /// </summary>
    public bool RememberMe { get; set; } = false;
}

/// <summary>
/// DTO cho request đăng xuất
/// </summary>
public class LogoutRequest
{
    /// <summary>
    /// Refresh token để đăng xuất
    /// </summary>
    public string? RefreshToken { get; set; }
}

/// <summary>
/// DTO cho request làm mới token
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// Refresh token để làm mới access token
    /// </summary>
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}