using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

/// <summary>
/// Entity cho Refresh Token
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// ID của refresh token
    /// </summary>
    public Guid RefreshTokenId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Token string
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// ID của user sở hữu token
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// User sở hữu token
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Thời gian tạo token
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời gian hết hạn
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Thời gian sử dụng cuối cùng
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Địa chỉ IP tạo token
    /// </summary>
    public string? CreatedByIp { get; set; }

    /// <summary>
    /// Địa chỉ IP sử dụng cuối cùng
    /// </summary>
    public string? LastUsedByIp { get; set; }

    /// <summary>
    /// User Agent tạo token
    /// </summary>
    public string? CreatedByUserAgent { get; set; }

    /// <summary>
    /// User Agent sử dụng cuối cùng
    /// </summary>
    public string? LastUsedByUserAgent { get; set; }

    /// <summary>
    /// Trạng thái token
    /// </summary>
    public RefreshTokenStatus Status { get; set; } = RefreshTokenStatus.Active;

    /// <summary>
    /// Lý do thu hồi token (nếu có)
    /// </summary>
    public string? RevokedReason { get; set; }

    /// <summary>
    /// Thời gian thu hồi token
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Token thay thế (nếu token này bị thu hồi)
    /// </summary>
    public string? ReplacedByToken { get; set; }

    /// <summary>
    /// Kiểm tra token có hết hạn không
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Kiểm tra token có bị thu hồi không
    /// </summary>
    public bool IsRevoked => Status == RefreshTokenStatus.Revoked;

    /// <summary>
    /// Kiểm tra token có còn hoạt động không
    /// </summary>
    public bool IsActive => !IsRevoked && !IsExpired;
}

/// <summary>
/// Enum cho trạng thái Refresh Token
/// </summary>
public enum RefreshTokenStatus
{
    /// <summary>
    /// Đang hoạt động
    /// </summary>
    Active = 1,

    /// <summary>
    /// Đã thu hồi
    /// </summary>
    Revoked = 2,

    /// <summary>
    /// Đã hết hạn
    /// </summary>
    Expired = 3
}
