using AutoMapper;
using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using UserService.Application.DTOs.Responses;
using UserService.Application.Interfaces.Repositories;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Infrastructure.Services;

public class StudentAppService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public StudentAppService(
        IStudentRepository studentRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _studentRepository = studentRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<StudentResponse> CreateStudentAsync(CreateStudentRequest request)
    {
        // Validate unique constraints
        if (await _userRepository.UsernameExistsAsync(request.Username))
            throw new ApiException("Username already exists");

        if (await _userRepository.EmailExistsAsync(request.Email))
            throw new ApiException("Email already exists");

        if (await _studentRepository.StudentIdExistsAsync(request.StudentId))
            throw new ApiException("StudentId already exists");

        var student = new Student
        {
            UserId = Guid.NewGuid(),
            StudentCode = request.StudentId,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Major = request.Major,
            EnrollmentYear = request.EnrollmentYear,
            ClassYear = request.ClassYear,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _studentRepository.AddAsync(student);
        return _mapper.Map<StudentResponse>(student);
    }

    public async Task<StudentResponse?> GetStudentByIdAsync(string studentId)
    {
        var student = await _studentRepository.GetByIdAsync(Guid.Parse(studentId));
        return student != null ? _mapper.Map<StudentResponse>(student) : null;
    }

    public async Task<StudentResponse?> GetStudentByStudentIdAsync(string studentId)
    {
        var student = await _studentRepository.GetByStudentIdAsync(studentId);
        return student != null ? _mapper.Map<StudentResponse>(student) : null;
    }

    public async Task<PagedResultDto<StudentResponse>> GetStudentsAsync(int pageNumber, int pageSize, string? classId = null)
    {
        List<Student> students;
        int totalCount;

        if (!string.IsNullOrEmpty(classId))
        {
            students = await _studentRepository.GetStudentsByClassIdAsync(Guid.Parse(classId));
            students = students.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            totalCount = students.Count;
        }
        else
        {
            students = await _studentRepository.GetPagedAsync(pageNumber, pageSize);
            totalCount = await _studentRepository.CountAsync();
        }

        var studentResponses = _mapper.Map<List<StudentResponse>>(students);
        return new PagedResultDto<StudentResponse>(studentResponses, totalCount, pageNumber, pageSize);
    }

    public async Task<List<StudentResponse>> GetStudentsByClassIdAsync(string classId)
    {
        var students = await _studentRepository.GetStudentsByClassIdAsync(Guid.Parse(classId));
        return _mapper.Map<List<StudentResponse>>(students);
    }

    public async Task<bool> UpdateStudentAsync(string userId, UpdateUserRequest request)
    {
        var student = await _studentRepository.GetByIdAsync(Guid.Parse(userId));
        if (student == null)
            throw new ApiException("Student not found", 404);

        if (!string.IsNullOrEmpty(request.Email))
        {
            if (await _userRepository.EmailExistsAsync(request.Email) && student.Email != request.Email)
                throw new ApiException("Email already exists");
            
            student.Email = request.Email;
        }

        if (!string.IsNullOrEmpty(request.FullName))
            student.FullName = request.FullName;

        if (!string.IsNullOrEmpty(request.Major))
            student.Major = request.Major;

        if (request.ClassYear.HasValue)
            student.ClassYear = request.ClassYear.Value;

        student.UpdatedAt = DateTime.UtcNow;
        return await _studentRepository.UpdateAsync(student);
    }

    public async Task<bool> DeleteStudentAsync(string userId)
    {
        var student = await _studentRepository.GetByIdAsync(Guid.Parse(userId));
        if (student == null)
            throw new ApiException("Student not found", 404);

        return await _studentRepository.DeleteAsync(Guid.Parse(userId));
    }
}

