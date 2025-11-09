using AutoMapper;
using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using UserService.Application.DTOs.Responses;
using UserService.Application.Interfaces.Repositories;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Services;

public class ClassAppService : IClassService
{
    private readonly IClassRepository _classRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUserClassRepository _userClassRepository;
    private readonly IMapper _mapper;

    public ClassAppService(
        IClassRepository classRepository,
        ITeacherRepository teacherRepository,
        IStudentRepository studentRepository,
        IUserClassRepository userClassRepository,
        IMapper mapper)
    {
        _classRepository = classRepository;
        _teacherRepository = teacherRepository;
        _studentRepository = studentRepository;
        _userClassRepository = userClassRepository;
        _mapper = mapper;
    }

    public async Task<ClassResponse> CreateClassAsync(CreateClassRequest request)
    {
        // Validate teacher exists
        var teacher = await _teacherRepository.GetByIdAsync(Guid.Parse(request.TeacherId));
        if (teacher == null)
            throw new ApiException("Teacher not found", 404);

        // Generate unique class code if not provided
        var classCode = request.ClassCode;
        if (string.IsNullOrEmpty(classCode))
        {
            classCode = await GenerateUniqueClassCodeAsync();
        }
        else if (await _classRepository.ClassCodeExistsAsync(classCode))
        {
            throw new ApiException("Class code already exists");
        }

        var classEntity = new Class
        {
            ClassId = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            TeacherId = Guid.Parse(request.TeacherId),
            ClassCode = classCode,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _classRepository.AddAsync(classEntity);
        
        // Load teacher for response
        classEntity = await _classRepository.GetClassWithTeacherAsync(classEntity.ClassId);
        return _mapper.Map<ClassResponse>(classEntity!);
    }

    public async Task<ClassResponse?> GetClassByIdAsync(string classId)
    {
        var classEntity = await _classRepository.GetClassWithTeacherAsync(Guid.Parse(classId));
        return classEntity != null ? _mapper.Map<ClassResponse>(classEntity) : null;
    }

    public async Task<ClassDetailResponse?> GetClassDetailAsync(string classId)
    {
        var classEntity = await _classRepository.GetClassWithStudentsAsync(Guid.Parse(classId));
        return classEntity != null ? _mapper.Map<ClassDetailResponse>(classEntity) : null;
    }

    public async Task<PagedResultDto<ClassResponse>> GetClassesAsync(int pageNumber, int pageSize, string? teacherId = null, bool? isActive = null)
    {
        var teacherGuid = !string.IsNullOrEmpty(teacherId) ? Guid.Parse(teacherId) : (Guid?)null;
        
        var classes = await _classRepository.GetPagedAsync(pageNumber, pageSize, c =>
            (teacherGuid == null || c.TeacherId == teacherGuid) &&
            (!isActive.HasValue || c.IsActive == isActive.Value)
        );

        var totalCount = await _classRepository.CountAsync(c =>
            (teacherGuid == null || c.TeacherId == teacherGuid) &&
            (!isActive.HasValue || c.IsActive == isActive.Value)
        );

        // Load teachers for each class
        foreach (var cls in classes)
        {
            var fullClass = await _classRepository.GetClassWithTeacherAsync(cls.ClassId);
            if (fullClass != null)
            {
                cls.Teacher = fullClass.Teacher;
                cls.UserClasses = fullClass.UserClasses;
            }
        }

        var classResponses = _mapper.Map<List<ClassResponse>>(classes);
        return new PagedResultDto<ClassResponse>(classResponses, totalCount, pageNumber, pageSize);
    }

    public async Task<List<ClassResponse>> GetClassesByTeacherIdAsync(string teacherId)
    {
        var classes = await _classRepository.GetClassesByTeacherIdAsync(Guid.Parse(teacherId));
        
        // Load teacher info
        foreach (var cls in classes)
        {
            var fullClass = await _classRepository.GetClassWithTeacherAsync(cls.ClassId);
            if (fullClass != null)
                cls.Teacher = fullClass.Teacher;
        }

        return _mapper.Map<List<ClassResponse>>(classes);
    }

    public async Task<List<ClassResponse>> GetClassesByStudentIdAsync(string studentId)
    {
        var classes = await _classRepository.GetClassesByStudentIdAsync(Guid.Parse(studentId));
        return _mapper.Map<List<ClassResponse>>(classes);
    }

    public async Task<bool> UpdateClassAsync(string classId, UpdateClassRequest request)
    {
        var classEntity = await _classRepository.GetByIdAsync(Guid.Parse(classId));
        if (classEntity == null)
            throw new ApiException("Class not found", 404);

        if (!string.IsNullOrEmpty(request.Name))
            classEntity.Name = request.Name;

        if (request.Description != null)
            classEntity.Description = request.Description;

        if (!string.IsNullOrEmpty(request.TeacherId))
        {
            var teacher = await _teacherRepository.GetByIdAsync(Guid.Parse(request.TeacherId));
            if (teacher == null)
                throw new ApiException("Teacher not found", 404);
            
            classEntity.TeacherId = Guid.Parse(request.TeacherId);
        }

        if (request.IsActive.HasValue)
            classEntity.IsActive = request.IsActive.Value;

        return await _classRepository.UpdateAsync(classEntity);
    }

    public async Task<bool> DeleteClassAsync(string classId)
    {
        var classEntity = await _classRepository.GetByIdAsync(Guid.Parse(classId));
        if (classEntity == null)
            throw new ApiException("Class not found", 404);

        return await _classRepository.DeleteAsync(Guid.Parse(classId));
    }

    public async Task<bool> AddStudentToClassAsync(string classId, string studentId)
    {
        var classGuid = Guid.Parse(classId);
        var studentGuid = Guid.Parse(studentId);
        
        // Validate class exists
        var classEntity = await _classRepository.GetByIdAsync(classGuid);
        if (classEntity == null)
            throw new ApiException("Class not found", 404);

        // Validate student exists
        var student = await _studentRepository.GetByIdAsync(studentGuid);
        if (student == null)
            throw new ApiException("Student not found", 404);

        // Check if already enrolled
        if (await _userClassRepository.ExistsAsync(studentGuid, classGuid))
            throw new ApiException("Student is already enrolled in this class");

        var userClass = new UserClass
        {
            StudentId = studentGuid,
            ClassId = classGuid,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _userClassRepository.AddAsync(userClass);
        return true;
    }

    public async Task<bool> AddStudentsToClassAsync(AddStudentsToClassRequest request)
    {
        var classGuid = request.ClassId;
        
        // Validate class exists
        var classEntity = await _classRepository.GetByIdAsync(classGuid);
        if (classEntity == null)
            throw new ApiException("Class not found", 404);

        var userClasses = new List<UserClass>();

        foreach (var studentId in request.StudentIds)
        {
            var studentGuid = Guid.Parse(studentId);
            
            // Validate student exists
            var student = await _studentRepository.GetByIdAsync(studentGuid);
            if (student == null)
                continue; // Skip invalid students

            // Check if already enrolled
            if (await _userClassRepository.ExistsAsync(studentGuid, classGuid))
                continue; // Skip already enrolled students

            userClasses.Add(new UserClass
            {
                StudentId = studentGuid,
                ClassId = classGuid,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        if (userClasses.Any())
            await _userClassRepository.AddRangeAsync(userClasses);

        return true;
    }

    public async Task<bool> RemoveStudentFromClassAsync(string classId, string studentId)
    {
        var classGuid = Guid.Parse(classId);
        var studentGuid = Guid.Parse(studentId);
        
        if (!await _userClassRepository.ExistsAsync(studentGuid, classGuid))
            throw new ApiException("Student is not enrolled in this class", 404);

        return await _userClassRepository.RemoveAsync(studentGuid, classGuid);
    }

    public async Task<List<StudentResponse>> GetStudentListByClassAsync(string classId)
    {
        var students = await _studentRepository.GetStudentsByClassIdAsync(Guid.Parse(classId));
        return _mapper.Map<List<StudentResponse>>(students);
    }

    public async Task<List<string>> CheckDuplicatesAsync(string classId, List<string> identifiers)
    {
        var classGuid = Guid.Parse(classId);
        var duplicates = new List<string>();

        foreach (var identifier in identifiers)
        {
            // Try find student by StudentCode or Email
            var student = await _studentRepository.GetByStudentCodeAsync(identifier);
            if (student == null)
            {
                student = await _studentRepository.GetByEmailAsync(identifier);
            }

            if (student != null)
            {
                // Check if already enrolled
                var isEnrolled = await _userClassRepository.IsStudentEnrolledAsync(student.UserId, classGuid);
                if (isEnrolled)
                {
                    duplicates.Add(identifier);
                }
            }
        }

        return duplicates;
    }

    public async Task<BulkEnrollResult> BulkEnrollStudentsAsync(string classId, List<string> studentIds)
    {
        var result = new BulkEnrollResult();
        var classGuid = Guid.Parse(classId);

        // Verify class exists
        var classEntity = await _classRepository.GetByIdAsync(classGuid);
        if (classEntity == null)
            throw new ApiException("Class not found", 404);

        foreach (var studentId in studentIds)
        {
            try
            {
                var studentGuid = Guid.Parse(studentId);
                var student = await _studentRepository.GetByIdAsync(studentGuid);
                
                if (student == null)
                {
                    result.FailedCount++;
                    result.Errors.Add(new BulkEnrollError
                    {
                        StudentId = studentId,
                        ErrorMessage = "Student not found"
                    });
                    continue;
                }

                // Check if already enrolled
                var isEnrolled = await _userClassRepository.IsStudentEnrolledAsync(studentGuid, classGuid);
                if (isEnrolled)
                {
                    result.FailedCount++;
                    result.Errors.Add(new BulkEnrollError
                    {
                        StudentId = studentId,
                        ErrorMessage = "Student already enrolled"
                    });
                    continue;
                }

                // Enroll student
                var userClass = new UserClass
                {
                    StudentId = studentGuid,
                    ClassId = classGuid,
                    IsActive = true,
                    JoinedAt = DateTime.UtcNow
                };

                await _userClassRepository.AddAsync(userClass);
                result.SuccessCount++;
                result.SuccessIds.Add(studentId);
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.Errors.Add(new BulkEnrollError
                {
                    StudentId = studentId,
                    ErrorMessage = ex.Message
                });
            }
        }

        return result;
    }

    private async Task<string> GenerateUniqueClassCodeAsync()
    {
        string classCode;
        int attempts = 0;
        
        do
        {
            // Generate format: CLS + Year(2 digits) + Random(4 digits)
            var year = DateTime.UtcNow.Year % 100;
            var random = new Random().Next(1000, 9999);
            classCode = $"CLS{year}{random}";
            attempts++;

            if (attempts > 10)
                throw new ApiException("Unable to generate unique class code", 500);

        } while (await _classRepository.ClassCodeExistsAsync(classCode));

        return classCode;
    }
}
