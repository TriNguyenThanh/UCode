using AutoMapper;
using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using UserService.Application.DTOs.Responses;
using UserService.Application.Interfaces.Repositories;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Infrastructure.Services;

public class UserAppService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserAppService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserResponse?> GetUserByIdAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        return user != null ? _mapper.Map<UserResponse>(user) : null;
    }


    public async Task<UserResponse?> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        return user != null ? _mapper.Map<UserResponse>(user) : null;
    }

    public async Task<UserResponse?> GetUserByUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        return user != null ? _mapper.Map<UserResponse>(user) : null;
    }


    public async Task<PagedResultDto<UserResponse>> GetUsersAsync(int pageNumber, int pageSize, UserRole? role = null, UserStatus? status = null)
    {
        var users = await _userRepository.GetPagedAsync(pageNumber, pageSize, u =>
            (!role.HasValue || u.Role == role.Value) &&
            (!status.HasValue || u.Status == status.Value)
        );

        var totalCount = await _userRepository.CountAsync(u =>
            (!role.HasValue || u.Role == role.Value) &&
            (!status.HasValue || u.Status == status.Value)
        );

        var userResponses = _mapper.Map<List<UserResponse>>(users);
        return new PagedResultDto<UserResponse>(userResponses, totalCount, pageNumber, pageSize);
    }

    public async Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null)
            throw new ApiException("User not found", 404);

        // Update common fields
        if (!string.IsNullOrEmpty(request.Email))
        {
            if (await _userRepository.EmailExistsAsync(request.Email) && user.Email != request.Email)
                throw new ApiException("Email already exists");
            
            user.Email = request.Email;
        }

        if (!string.IsNullOrEmpty(request.FullName))
            user.FullName = request.FullName;

        // Update specific fields based on role
        if (user is Student student)
        {
            if (!string.IsNullOrEmpty(request.Major))
                student.Major = request.Major;
            
            if (request.ClassYear.HasValue)
                student.ClassYear = request.ClassYear.Value;
        }
        else if (user is Teacher teacher)
        {
            if (!string.IsNullOrEmpty(request.Department))
                teacher.Department = request.Department;
            
            if (!string.IsNullOrEmpty(request.Title))
                teacher.Title = request.Title;
            
            if (!string.IsNullOrEmpty(request.Phone))
                teacher.Phone = request.Phone;
        }

        user.UpdatedAt = DateTime.UtcNow;
        return await _userRepository.UpdateAsync(user);
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(Guid.Parse(request.UserId));
        if (user == null)
            throw new ApiException("User not found", 404);

        // Verify old password
        if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
            throw new ApiException("Current password is incorrect");

        // Hash and update new password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        return await _userRepository.UpdateAsync(user);
    }

    public async Task<bool> UpdateUserStatusAsync(string userId, UserStatus status)
    {
        var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null)
            throw new ApiException("User not found", 404);

        user.Status = status;
        user.UpdatedAt = DateTime.UtcNow;

        return await _userRepository.UpdateAsync(user);
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null)
            throw new ApiException("User not found", 404);

        return await _userRepository.DeleteAsync(Guid.Parse(userId));
    }
}

