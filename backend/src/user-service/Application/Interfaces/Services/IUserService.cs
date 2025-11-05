using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using UserService.Application.DTOs.Responses;
using UserService.Domain.Enums;

namespace UserService.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserResponse?> GetUserByIdAsync(string userId);
    Task<UserResponse?> GetUserByEmailAsync(string email);
    Task<UserResponse?> GetUserByUsernameAsync(string username);
    Task<PagedResultDto<UserResponse>> GetUsersAsync(int pageNumber, int pageSize, UserRole? role = null, UserStatus? status = null);
    Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
    Task<bool> UpdateUserStatusAsync(string userId, UserStatus status);
    Task<bool> DeleteUserAsync(string userId);
}

