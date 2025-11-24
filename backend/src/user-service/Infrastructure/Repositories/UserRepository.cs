using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces.Repositories;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(UserDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _dbSet
            .AnyAsync(u => u.Username.ToLower() == username.ToLower());
    }

    /// <summary>
    /// Lấy danh sách email từ danh sách ID người dùng
    /// </summary>
    public async Task<List<string>> GetEmailsByIdsAsync(List<Guid> ids)
    {
        if (ids == null || !ids.Any())
        {
            return new List<string>();
        }
        
        return await _dbSet
            .Where(u => ids.Contains(u.UserId))
            .Select(u => u.Email)
            .ToListAsync();
    }
}

