using AutoMapper;
using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using UserService.Application.DTOs.Responses;
using UserService.Application.Interfaces.Repositories;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Infrastructure.Services;

public class TeacherAppService : ITeacherService
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public TeacherAppService(
        ITeacherRepository teacherRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _teacherRepository = teacherRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<TeacherResponse> CreateTeacherAsync(CreateTeacherRequest request)
    {
        // Validate unique constraints
        if (await _userRepository.UsernameExistsAsync(request.Username))
            throw new ApiException("Username already exists");

        if (await _userRepository.EmailExistsAsync(request.Email))
            throw new ApiException("Email already exists");

        if (await _teacherRepository.EmployeeIdExistsAsync(request.EmployeeId))
            throw new ApiException("EmployeeId already exists");

        var teacher = new Teacher
        {
            UserId = Guid.NewGuid(),
            TeacherCode = request.EmployeeId,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Department = request.Department,
            Title = request.Title ?? string.Empty,
            Phone = request.Phone ?? string.Empty,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _teacherRepository.AddAsync(teacher);
        return _mapper.Map<TeacherResponse>(teacher);
    }

    public async Task<TeacherResponse?> GetTeacherByIdAsync(string teacherId)
    {
        var teacher = await _teacherRepository.GetTeacherWithClassesAsync(Guid.Parse(teacherId));
        return teacher != null ? _mapper.Map<TeacherResponse>(teacher) : null;
    }

    public async Task<TeacherResponse?> GetTeacherByEmployeeIdAsync(string employeeId)
    {
        var teacher = await _teacherRepository.GetByEmployeeIdAsync(employeeId);
        return teacher != null ? _mapper.Map<TeacherResponse>(teacher) : null;
    }

    public async Task<PagedResultDto<TeacherResponse>> GetTeachersAsync(int pageNumber, int pageSize, string? department = null)
    {
        List<Teacher> teachers;
        int totalCount;

        if (!string.IsNullOrEmpty(department))
        {
            teachers = await _teacherRepository.GetTeachersByDepartmentAsync(department);
            teachers = teachers.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            totalCount = teachers.Count;
        }
        else
        {
            teachers = await _teacherRepository.GetPagedAsync(pageNumber, pageSize);
            totalCount = await _teacherRepository.CountAsync();
        }

        var teacherResponses = _mapper.Map<List<TeacherResponse>>(teachers);
        return new PagedResultDto<TeacherResponse>(teacherResponses, totalCount, pageNumber, pageSize);
    }

    public async Task<List<ClassResponse>> GetTeacherClassesAsync(string teacherId)
    {
        var teacher = await _teacherRepository.GetTeacherWithClassesAsync(Guid.Parse(teacherId));
        if (teacher == null)
            throw new ApiException("Teacher not found", 404);

        return _mapper.Map<List<ClassResponse>>(teacher.Classes);
    }

    public async Task<bool> UpdateTeacherAsync(string userId, UpdateUserRequest request)
    {
        var teacher = await _teacherRepository.GetByIdAsync(Guid.Parse(userId));
        if (teacher == null)
            throw new ApiException("Teacher not found", 404);

        if (!string.IsNullOrEmpty(request.Email))
        {
            if (await _userRepository.EmailExistsAsync(request.Email) && teacher.Email != request.Email)
                throw new ApiException("Email already exists");
            
            teacher.Email = request.Email;
        }

        if (!string.IsNullOrEmpty(request.FullName))
            teacher.FullName = request.FullName;

        if (!string.IsNullOrEmpty(request.Department))
            teacher.Department = request.Department;

        if (!string.IsNullOrEmpty(request.Title))
            teacher.Title = request.Title;

        if (!string.IsNullOrEmpty(request.Phone))
            teacher.Phone = request.Phone;

        teacher.UpdatedAt = DateTime.UtcNow;
        return await _teacherRepository.UpdateAsync(teacher);
    }

    public async Task<bool> DeleteTeacherAsync(string userId)
    {
        var teacher = await _teacherRepository.GetByIdAsync(Guid.Parse(userId));
        if (teacher == null)
            throw new ApiException("Teacher not found", 404);

        return await _teacherRepository.DeleteAsync(Guid.Parse(userId));
    }
}

