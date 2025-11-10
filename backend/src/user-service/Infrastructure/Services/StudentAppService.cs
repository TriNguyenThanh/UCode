using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

        if (await _studentRepository.StudentCodeExistsAsync(request.StudentCode))
            throw new ApiException("StudentCode already exists");

        var student = new Student
        {
            UserId = Guid.NewGuid(),
            StudentCode = request.StudentCode,
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

    public async Task<StudentResponse?> GetStudentByStudentCodeAsync(string studentCode)
    {
        var student = await _studentRepository.GetByStudentCodeAsync(studentCode);
        return student != null ? _mapper.Map<StudentResponse>(student) : null;
    }

    public async Task<PagedResultDto<StudentListResponse>> GetStudentsAsync(
        int pageNumber, 
        int pageSize, 
        string? classId = null,
        string? search = null, 
        int? year = null, 
        string? major = null, 
        string? status = null, 
        string? excludeClassId = null)
    {
        List<Student> students;
        int totalCount;

        if (!string.IsNullOrEmpty(classId))
        {
            // Get students in a specific class
            students = await _studentRepository.GetStudentsByClassIdAsync(Guid.Parse(classId));
            students = students.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            totalCount = students.Count;
        }
        else
        {
            // Get all students with filters
            var query = _studentRepository.AsQueryable();
            
            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => 
                    s.FullName.Contains(search) || 
                    s.StudentCode.Contains(search) || 
                    s.Email.Contains(search));
            }
            
            if (year.HasValue)
            {
                query = query.Where(s => s.EnrollmentYear == year.Value);
            }
            
            if (!string.IsNullOrEmpty(major))
            {
                query = query.Where(s => s.Major == major);
            }
            
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<UserStatus>(status, out var userStatus))
            {
                query = query.Where(s => s.Status == userStatus);
            }
            
            // Exclude students already in a class
            if (!string.IsNullOrEmpty(excludeClassId))
            {
                var excludeClassGuid = Guid.Parse(excludeClassId);
                query = query.Where(s => !s.UserClasses.Any(uc => 
                    uc.ClassId == excludeClassGuid && uc.IsActive));
            }
            
            totalCount = await query.CountAsync();
            students = await query
                .OrderBy(s => s.StudentCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        var studentResponses = _mapper.Map<List<StudentListResponse>>(students);
        return new PagedResultDto<StudentListResponse>(studentResponses, totalCount, pageNumber, pageSize);
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

    public async Task<List<StudentValidationResult>> ValidateBatchAsync(List<string> identifiers, string? classId = null)
    {
        var results = new List<StudentValidationResult>();
        Guid? classGuid = string.IsNullOrEmpty(classId) ? null : Guid.Parse(classId);

        foreach (var identifier in identifiers)
        {
            var result = new StudentValidationResult { Identifier = identifier };

            // Try find student by StudentCode or Email
            var student = await _studentRepository.GetByStudentCodeAsync(identifier);
            if (student == null)
            {
                student = await _studentRepository.GetByEmailAsync(identifier);
            }

            if (student == null)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Student not found with identifier: {identifier}";
            }
            else
            {
                result.IsValid = true;
                result.StudentId = student.UserId.ToString();
                result.StudentCode = student.StudentCode;
                result.FullName = student.FullName;
                result.Email = student.Email;

                // Check if already enrolled in class
                if (classGuid.HasValue)
                {
                    var isEnrolled = student.UserClasses?.Any(uc => 
                        uc.ClassId == classGuid.Value && uc.IsActive) ?? false;
                    
                    if (isEnrolled)
                    {
                        result.IsDuplicate = true;
                        result.ErrorMessage = "Student already enrolled in this class";
                    }
                }
            }

            results.Add(result);
        }

        return results;
    }
}
