using AutoMapper;
using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using UserService.Application.DTOs.Responses;
using UserService.Application.DTOs.Admin;
using UserService.Application.Interfaces.Repositories;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Infrastructure.Services;

public class UserAppService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IClassRepository _classRepository;
    private readonly IUserClassRepository _userClassRepository;
    private readonly IMapper _mapper;

    public UserAppService(
        IUserRepository userRepository,
        ITeacherRepository teacherRepository,
        IStudentRepository studentRepository,
        IClassRepository classRepository,
        IUserClassRepository userClassRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _teacherRepository = teacherRepository;
        _studentRepository = studentRepository;
        _classRepository = classRepository;
        _userClassRepository = userClassRepository;
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

    public async Task<bool> UpdateUserRoleAsync(string userId, UserRole role)
    {
        var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null)
            throw new ApiException("User not found", 404);

        user.Role = role;
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

    // ==================== ADMIN USER MANAGEMENT ====================

    public async Task<UserStatisticsResponse> GetUserStatisticsAsync()
    {
        var allUsers = await _userRepository.GetAllAsync();
        
        return new UserStatisticsResponse
        {
            TotalUsers = allUsers.Count,
            Teachers = allUsers.Count(u => u.Role == UserRole.Teacher),
            Students = allUsers.Count(u => u.Role == UserRole.Student),
            Admins = allUsers.Count(u => u.Role == UserRole.Admin),
            ActiveUsers = allUsers.Count(u => u.Status == UserStatus.Active),
            InactiveUsers = allUsers.Count(u => u.Status == UserStatus.Inactive)
        };
    }

    public async Task<PagedResultDto<AdminUserResponse>> GetAllUsersForAdminAsync(
        int pageNumber, int pageSize, string? searchTerm = null, string? role = null, bool? isActive = null)
    {
        var allUsers = await _userRepository.GetAllAsync();
        var allTeachers = await _teacherRepository.GetAllAsync();
        var allStudents = await _studentRepository.GetAllAsync();
        var allClasses = await _classRepository.GetAllAsync();
        var allUserClasses = await _userClassRepository.GetAllAsync();

        // Build query
        var query = allUsers.AsQueryable();

        // Apply search
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u =>
                u.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (u.Role == UserRole.Student && allStudents.Any(s => s.UserId == u.UserId && s.StudentCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))) ||
                (u.Role == UserRole.Teacher && allTeachers.Any(t => t.UserId == u.UserId && t.TeacherCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
            );
        }

        // Apply role filter
        if (!string.IsNullOrEmpty(role))
        {
            if (Enum.TryParse<UserRole>(role, true, out var userRole))
            {
                query = query.Where(u => u.Role == userRole);
            }
        }

        // Apply active filter
        if (isActive.HasValue)
        {
            var status = isActive.Value ? UserStatus.Active : UserStatus.Inactive;
            query = query.Where(u => u.Status == status);
        }

        var totalCount = query.Count();
        var users = query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var response = users.Select(u =>
        {
            var student = allStudents.FirstOrDefault(s => s.UserId == u.UserId);
            var teacher = allTeachers.FirstOrDefault(t => t.UserId == u.UserId);
            
            var adminUser = new AdminUserResponse
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role.ToString(),
                IsActive = u.Status == UserStatus.Active,
                CreatedAt = u.CreatedAt,
                StudentCode = student?.StudentCode,
                TeacherCode = teacher?.TeacherCode,
                ClassCount = 0,
                EnrolledClassCount = 0
            };

            if (u.Role == UserRole.Teacher && teacher != null)
            {
                adminUser.ClassCount = allClasses.Count(c => c.TeacherId == teacher.UserId);
            }
            else if (u.Role == UserRole.Student && student != null)
            {
                adminUser.EnrolledClassCount = allUserClasses.Count(uc => uc.StudentId == student.UserId);
            }

            return adminUser;
        }).ToList();

        return new PagedResultDto<AdminUserResponse>(response, totalCount, pageNumber, pageSize);
    }

    public async Task<AdminUserDetailResponse?> GetUserDetailForAdminAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null) return null;

        // Student and Teacher don't have UserId, they use Id which is same as User.UserId
        var allStudents = await _studentRepository.GetAllAsync();
        var allTeachers = await _teacherRepository.GetAllAsync();
        var student = allStudents.FirstOrDefault(s => s.UserId == user.UserId);
        var teacher = allTeachers.FirstOrDefault(t => t.UserId == user.UserId);
        var allClasses = await _classRepository.GetAllAsync();
        var allUserClasses = await _userClassRepository.GetAllAsync();

        var detail = new AdminUserDetailResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsActive = user.Status == UserStatus.Active,
            CreatedAt = user.CreatedAt,
            StudentCode = student?.StudentCode,
            TeacherCode = null,
            Phone = null, // Add if available in User entity
            LastLoginAt = null, // Add if tracked
            EmailVerified = true, // Add if tracked
            ClassCount = 0,
            EnrolledClassCount = 0,
            TotalAssignments = 0,
            TotalSubmissions = 0,
            AverageScore = 0
        };

        if (user.Role == UserRole.Teacher && teacher != null)
        {
            detail.ClassCount = allClasses.Count(c => c.TeacherId == teacher.UserId);
            // TODO: Get total assignments from assignment service
        }
        else if (user.Role == UserRole.Student && student != null)
        {
            detail.EnrolledClassCount = allUserClasses.Count(uc => uc.StudentId == student.UserId);
            // TODO: Get submissions and scores from assignment service
        }

        return detail;
    }

    public async Task<bool> CreateUserByAdminAsync(CreateUserByAdminRequest request)
    {
        // Check if email already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new ApiException("Email already exists", 400);

        // Parse role
        if (!Enum.TryParse<UserRole>(request.Role, true, out var userRole))
            throw new ApiException("Invalid role", 400);

        // Create user based on role
        Domain.Entities.User user;
        
        switch (userRole)
        {
            case UserRole.Admin:
                user = new Domain.Entities.Admin
                {
                    UserId = Guid.NewGuid(),
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    FullName = request.FullName,
                    Username = request.Email.Split('@')[0], // Use email prefix as username
                    Status = request.IsActive ? UserStatus.Active : UserStatus.Inactive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                break;
                
            case UserRole.Teacher:
                if (string.IsNullOrEmpty(request.TeacherCode))
                    throw new ApiException("Teacher code is required for Teacher role", 400);
                    
                user = new Domain.Entities.Teacher
                {
                    UserId = Guid.NewGuid(),
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    FullName = request.FullName,
                    Username = request.Email.Split('@')[0],
                    Status = request.IsActive ? UserStatus.Active : UserStatus.Inactive,
                    TeacherCode = request.TeacherCode,
                    Phone = request.Phone ?? string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                break;
                
            case UserRole.Student:
                if (string.IsNullOrEmpty(request.StudentCode))
                    throw new ApiException("Student code is required for Student role", 400);
                    
                user = new Domain.Entities.Student
                {
                    UserId = Guid.NewGuid(),
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    FullName = request.FullName,
                    Username = request.Email.Split('@')[0],
                    Status = request.IsActive ? UserStatus.Active : UserStatus.Inactive,
                    StudentCode = request.StudentCode,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                break;
                
            default:
                throw new ApiException("Invalid role", 400);
        }

        await _userRepository.AddAsync(user);
        return true;
    }

    public async Task<bool> UpdateUserByAdminAsync(string userId, UpdateUserByAdminRequest request)
    {
        var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null)
            throw new ApiException("User not found", 404);

        if (!string.IsNullOrEmpty(request.FullName))
            user.FullName = request.FullName;

        if (!string.IsNullOrEmpty(request.Email))
            user.Email = request.Email;

        if (!string.IsNullOrEmpty(request.Role))
        {
            if (Enum.TryParse<UserRole>(request.Role, true, out var newRole))
            {
                user.Role = newRole;
            }
        }

        if (request.IsActive.HasValue)
        {
            user.Status = request.IsActive.Value ? UserStatus.Active : UserStatus.Inactive;
        }

        user.UpdatedAt = DateTime.UtcNow;
        return await _userRepository.UpdateAsync(user);
    }

    public async Task<bool> DeleteUserByAdminAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null)
            throw new ApiException("User not found", 404);

        // TODO: Consider soft delete or cleanup related data
        return await _userRepository.DeleteAsync(Guid.Parse(userId));
    }

    public async Task<object> BulkActionAsync(string action, List<string> userIds, string? newRole = null)
    {
        var results = new List<object>();
        int successCount = 0;
        int failureCount = 0;

        foreach (var userId in userIds)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
                if (user == null)
                {
                    results.Add(new { userId, success = false, error = "User not found" });
                    failureCount++;
                    continue;
                }

                bool success = false;
                switch (action.ToLower())
                {
                    case "activate":
                        user.Status = UserStatus.Active;
                        user.UpdatedAt = DateTime.UtcNow;
                        success = await _userRepository.UpdateAsync(user);
                        break;

                    case "deactivate":
                        user.Status = UserStatus.Inactive;
                        user.UpdatedAt = DateTime.UtcNow;
                        success = await _userRepository.UpdateAsync(user);
                        break;

                    case "delete":
                        success = await _userRepository.DeleteAsync(Guid.Parse(userId));
                        break;

                    case "changerole":
                        if (!string.IsNullOrEmpty(newRole) && Enum.TryParse<UserRole>(newRole, true, out var role))
                        {
                            user.Role = role;
                            user.UpdatedAt = DateTime.UtcNow;
                            success = await _userRepository.UpdateAsync(user);
                        }
                        break;

                    default:
                        results.Add(new { userId, success = false, error = "Invalid action" });
                        failureCount++;
                        continue;
                }

                if (success)
                {
                    results.Add(new { userId, success = true });
                    successCount++;
                }
                else
                {
                    results.Add(new { userId, success = false, error = "Operation failed" });
                    failureCount++;
                }
            }
            catch (Exception ex)
            {
                results.Add(new { userId, success = false, error = ex.Message });
                failureCount++;
            }
        }

        return new
        {
            action,
            totalRequested = userIds.Count,
            successCount,
            failureCount,
            results
        };
    }
}


