using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.Repositories;

/// <summary>
/// Interface cho RefreshToken Repository
/// </summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    /// <summary>
    /// Tìm refresh token theo token string
    /// </summary>
    Task<RefreshToken?> GetByTokenAsync(string token);

    /// <summary>
    /// Lấy tất cả refresh tokens của user
    /// </summary>
    Task<List<RefreshToken>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Thu hồi tất cả refresh tokens của user
    /// </summary>
    Task RevokeAllUserTokensAsync(Guid userId, string reason, string? replacedByToken = null);

    /// <summary>
    /// Thu hồi refresh token cụ thể
    /// </summary>
    Task RevokeTokenAsync(string token, string reason, string? replacedByToken = null);

    /// <summary>
    /// Xóa các refresh tokens đã hết hạn
    /// </summary>
    Task CleanupExpiredTokensAsync();

    /// <summary>
    /// Cập nhật thông tin sử dụng token
    /// </summary>
    Task UpdateTokenUsageAsync(string token, string? ipAddress = null, string? userAgent = null);
}
