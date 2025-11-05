using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces.Repositories;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho RefreshToken
/// </summary>
public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(UserDbContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<List<RefreshToken>> GetByUserIdAsync(Guid userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync();
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, string reason, string? replacedByToken = null)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.IsActive)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.Status = RefreshTokenStatus.Revoked;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = reason;
            token.ReplacedByToken = replacedByToken;
        }

        await _context.SaveChangesAsync();
    }

    public async Task RevokeTokenAsync(string token, string reason, string? replacedByToken = null)
    {
        var refreshToken = await GetByTokenAsync(token);
        if (refreshToken != null && refreshToken.IsActive)
        {
            refreshToken.Status = RefreshTokenStatus.Revoked;
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedReason = reason;
            refreshToken.ReplacedByToken = replacedByToken;
            await _context.SaveChangesAsync();
        }
    }

    public async Task CleanupExpiredTokensAsync()
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.IsExpired || rt.Status == RefreshTokenStatus.Expired)
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateTokenUsageAsync(string token, string? ipAddress = null, string? userAgent = null)
    {
        var refreshToken = await GetByTokenAsync(token);
        if (refreshToken != null)
        {
            refreshToken.LastUsedAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(ipAddress))
                refreshToken.LastUsedByIp = ipAddress;
            if (!string.IsNullOrEmpty(userAgent))
                refreshToken.LastUsedByUserAgent = userAgent;
            
            await _context.SaveChangesAsync();
        }
    }
}
