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
    private readonly IAssignmentServiceClient _assignmentServiceClient;

    public ClassAppService(
        IClassRepository classRepository,
        ITeacherRepository teacherRepository,
        IStudentRepository studentRepository,
        IUserClassRepository userClassRepository,
        IMapper mapper,
        IAssignmentServiceClient assignmentServiceClient)
    {
        _classRepository = classRepository;
        _teacherRepository = teacherRepository;
        _studentRepository = studentRepository;
        _userClassRepository = userClassRepository;
        _mapper = mapper;
        _assignmentServiceClient = assignmentServiceClient;
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
        
        // Return class even if archived (for teacher to see with warning)
        return classEntity != null ? _mapper.Map<ClassResponse>(classEntity) : null;
    }

    public async Task<ClassDetailResponse?> GetClassDetailAsync(string classId)
    {
        var classEntity = await _classRepository.GetClassWithStudentsAsync(Guid.Parse(classId));
        
        // Return class even if archived (for teacher to see with warning)
        return classEntity != null ? _mapper.Map<ClassDetailResponse>(classEntity) : null;
    }

    public async Task<PagedResultDto<ClassResponse>> GetClassesAsync(int pageNumber, int pageSize, string? teacherId = null, bool? isActive = null)
    {
        var teacherGuid = !string.IsNullOrEmpty(teacherId) ? Guid.Parse(teacherId) : (Guid?)null;
        
        // Filter out archived classes for normal users (Teacher/Student views)
        var classes = await _classRepository.GetPagedAsync(pageNumber, pageSize, c =>
            (teacherGuid == null || c.TeacherId == teacherGuid) &&
            (!isActive.HasValue || c.IsActive == isActive.Value) &&
            !c.IsArchived // Only show non-archived classes
        );

        var totalCount = await _classRepository.CountAsync(c =>
            (teacherGuid == null || c.TeacherId == teacherGuid) &&
            (!isActive.HasValue || c.IsActive == isActive.Value) &&
            !c.IsArchived // Only count non-archived classes
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
        
        // Filter out archived classes - Teacher không nên thấy classes đã archive
        classes = classes.Where(c => !c.IsArchived).ToList();
        
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
        
        // Filter out archived classes - Student không nên thấy classes đã archive
        var activeClasses = classes.Where(c => !c.IsArchived).ToList();
        
        return _mapper.Map<List<ClassResponse>>(activeClasses);
    }

    public async Task<bool> UpdateClassAsync(string classId, UpdateClassRequest request)
    {
        var classEntity = await _classRepository.GetByIdAsync(Guid.Parse(classId));
        if (classEntity == null)
            throw new ApiException("Class not found", 404);

        // Prevent updating archived classes
        if (classEntity.IsArchived)
            throw new ApiException("Cannot update archived class. Please unarchive it first.", 400);

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

        classEntity.UpdatedAt = DateTime.UtcNow;

        return await _classRepository.UpdateAsync(classEntity);
    }

    public async Task<bool> DeleteClassAsync(string classId)
    {
        var classEntity = await _classRepository.GetByIdAsync(Guid.Parse(classId));
        if (classEntity == null)
            throw new ApiException("Class not found", 404);

        // Prevent deleting archived classes - should unarchive first or use admin delete
        if (classEntity.IsArchived)
            throw new ApiException("Cannot delete archived class. Please contact admin.", 400);

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

        // Prevent enrolling to archived classes
        if (classEntity.IsArchived)
            throw new ApiException("Cannot enroll students to archived class", 400);

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
        
        // Sync student to all active assignments of this class (fire-and-forget)
        _ = Task.Run(async () => 
        {
            try 
            {
                await _assignmentServiceClient.SyncStudentsToClassAssignmentsAsync(classGuid, new List<Guid> { studentGuid });
            }
            catch
            {
                // Ignore errors - this is a best-effort sync
            }
        });
        
        return true;
    }

    public async Task<bool> AddStudentsToClassAsync(AddStudentsToClassRequest request)
    {
        var classGuid = request.ClassId;
        
        // Validate class exists
        var classEntity = await _classRepository.GetByIdAsync(classGuid);
        if (classEntity == null)
            throw new ApiException("Class not found", 404);

        // Prevent enrolling to archived classes
        if (classEntity.IsArchived)
            throw new ApiException("Cannot enroll students to archived class", 400);

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
        {
            await _userClassRepository.AddRangeAsync(userClasses);
            
            // Sync students to all active assignments of this class (fire-and-forget)
            var addedStudentIds = userClasses.Select(uc => uc.StudentId).ToList();
            _ = Task.Run(async () => 
            {
                try 
                {
                    await _assignmentServiceClient.SyncStudentsToClassAssignmentsAsync(classGuid, addedStudentIds);
                }
                catch
                {
                    // Ignore errors - this is a best-effort sync
                }
            });
        }

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

    public async Task<List<StudentListResponse>> GetStudentListByClassAsync(string classId)
    {
        var students = await _studentRepository.GetStudentsByClassIdAsync(Guid.Parse(classId));
        return _mapper.Map<List<StudentListResponse>>(students);
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
        var result = new BulkEnrollResult
        {
            ClassId = classId,
            Results = new List<BulkEnrollStudentResult>()
        };
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
                    result.FailureCount++;
                    result.Results.Add(new BulkEnrollStudentResult
                    {
                        StudentId = studentId,
                        Success = false,
                        ErrorMessage = "Student not found"
                    });
                    continue;
                }

                // Check if already enrolled
                var isEnrolled = await _userClassRepository.IsStudentEnrolledAsync(studentGuid, classGuid);
                if (isEnrolled)
                {
                    result.FailureCount++;
                    result.Results.Add(new BulkEnrollStudentResult
                    {
                        StudentId = studentId,
                        Success = false,
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
                result.Results.Add(new BulkEnrollStudentResult
                {
                    StudentId = studentId,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                result.FailureCount++;
                result.Results.Add(new BulkEnrollStudentResult
                {
                    StudentId = studentId,
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        // Sync all successfully enrolled students to class assignments (fire-and-forget)
        var enrolledStudentIds = result.Results
            .Where(r => r.Success)
            .Select(r => Guid.Parse(r.StudentId))
            .ToList();

        if (enrolledStudentIds.Any())
        {
            _ = Task.Run(async () => 
            {
                try 
                {
                    await _assignmentServiceClient.SyncStudentsToClassAssignmentsAsync(classGuid, enrolledStudentIds);
                }
                catch
                {
                    // Ignore errors - this is a best-effort sync
                }
            });
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

    // ===== ADMIN METHODS =====

    public async Task<PagedResultDto<AdminClassResponse>> GetAllClassesForAdminAsync(
        int pageNumber, 
        int pageSize, 
        string? teacherId = null, 
        bool? isActive = null,
        bool? isArchived = null,
        string? searchTerm = null)
    {
        var teacherGuid = !string.IsNullOrEmpty(teacherId) ? Guid.Parse(teacherId) : (Guid?)null;
        
        var classes = await _classRepository.GetPagedAsync(pageNumber, pageSize, c =>
            (teacherGuid == null || c.TeacherId == teacherGuid) &&
            (!isActive.HasValue || c.IsActive == isActive.Value) &&
            (!isArchived.HasValue || c.IsArchived == isArchived.Value) &&
            (string.IsNullOrEmpty(searchTerm) || 
             c.Name.Contains(searchTerm) || 
             c.ClassCode.Contains(searchTerm) ||
             c.Description.Contains(searchTerm))
        );

        var totalCount = await _classRepository.CountAsync(c =>
            (teacherGuid == null || c.TeacherId == teacherGuid) &&
            (!isActive.HasValue || c.IsActive == isActive.Value) &&
            (!isArchived.HasValue || c.IsArchived == isArchived.Value) &&
            (string.IsNullOrEmpty(searchTerm) || 
             c.Name.Contains(searchTerm) || 
             c.ClassCode.Contains(searchTerm) ||
             c.Description.Contains(searchTerm))
        );

        var adminClassResponses = new List<AdminClassResponse>();

        foreach (var cls in classes)
        {
            var fullClass = await _classRepository.GetClassWithTeacherAsync(cls.ClassId);
            if (fullClass != null)
            {
                var response = new AdminClassResponse
                {
                    ClassId = fullClass.ClassId,
                    Name = fullClass.Name,
                    Description = fullClass.Description,
                    ClassCode = fullClass.ClassCode,
                    TeacherId = fullClass.TeacherId,
                    TeacherName = fullClass.Teacher?.FullName ?? "Unknown",
                    TeacherEmail = fullClass.Teacher?.Email ?? "Unknown",
                    StudentCount = fullClass.UserClasses?.Count ?? 0,
                    ActiveStudentCount = fullClass.UserClasses?.Count(uc => uc.IsActive) ?? 0,
                    AssignmentCount = 0, // TODO: Integrate with assignment service
                    SubmissionCount = 0, // TODO: Integrate with assignment service
                    IsActive = fullClass.IsActive,
                    IsArchived = fullClass.IsArchived,
                    CreatedAt = fullClass.CreatedAt,
                    UpdatedAt = fullClass.UpdatedAt,
                    ArchivedAt = fullClass.ArchivedAt
                };
                adminClassResponses.Add(response);
            }
        }

        return new PagedResultDto<AdminClassResponse>(adminClassResponses, totalCount, pageNumber, pageSize);
    }

    public async Task<AdminClassResponse?> GetClassDetailForAdminAsync(string classId)
    {
        var classEntity = await _classRepository.GetClassWithTeacherAsync(Guid.Parse(classId));
        if (classEntity == null)
            return null;

        return new AdminClassResponse
        {
            ClassId = classEntity.ClassId,
            Name = classEntity.Name,
            Description = classEntity.Description,
            ClassCode = classEntity.ClassCode,
            TeacherId = classEntity.TeacherId,
            TeacherName = classEntity.Teacher?.FullName ?? "Unknown",
            TeacherEmail = classEntity.Teacher?.Email ?? "Unknown",
            StudentCount = classEntity.UserClasses?.Count ?? 0,
            ActiveStudentCount = classEntity.UserClasses?.Count(uc => uc.IsActive) ?? 0,
            AssignmentCount = 0, // TODO: Integrate with assignment service
            SubmissionCount = 0, // TODO: Integrate with assignment service
            IsActive = classEntity.IsActive,
            IsArchived = classEntity.IsArchived,
            CreatedAt = classEntity.CreatedAt,
            UpdatedAt = classEntity.UpdatedAt,
            ArchivedAt = classEntity.ArchivedAt
        };
    }

    public async Task<bool> ArchiveClassAsync(string classId, string? reason = null)
    {
        var classEntity = await _classRepository.GetByIdAsync(Guid.Parse(classId));
        if (classEntity == null)
            throw new ApiException("Class not found", 404);

        if (classEntity.IsArchived)
            throw new ApiException("Class is already archived");

        classEntity.IsArchived = true;
        classEntity.ArchivedAt = DateTime.UtcNow;
        classEntity.ArchiveReason = reason;
        classEntity.UpdatedAt = DateTime.UtcNow;

        return await _classRepository.UpdateAsync(classEntity);
    }

    public async Task<bool> UnarchiveClassAsync(string classId)
    {
        var classEntity = await _classRepository.GetByIdAsync(Guid.Parse(classId));
        if (classEntity == null)
            throw new ApiException("Class not found", 404);

        if (!classEntity.IsArchived)
            throw new ApiException("Class is not archived");

        classEntity.IsArchived = false;
        classEntity.ArchivedAt = null;
        classEntity.ArchiveReason = null;
        classEntity.UpdatedAt = DateTime.UtcNow;

        return await _classRepository.UpdateAsync(classEntity);
    }

    public async Task<bool> UpdateClassByAdminAsync(UpdateClassByAdminRequest request)
    {
        var classEntity = await _classRepository.GetByIdAsync(request.ClassId);
        if (classEntity == null)
            throw new ApiException("Class not found", 404);

        // Validate teacher exists
        var teacher = await _teacherRepository.GetByIdAsync(request.TeacherId);
        if (teacher == null)
            throw new ApiException("Teacher not found", 404);

        classEntity.Name = request.Name;
        classEntity.Description = request.Description;
        classEntity.TeacherId = request.TeacherId;
        classEntity.IsActive = request.IsActive;
        classEntity.UpdatedAt = DateTime.UtcNow;

        return await _classRepository.UpdateAsync(classEntity);
    }

    public async Task<bool> DeleteClassByAdminAsync(string classId)
    {
        var classEntity = await _classRepository.GetByIdAsync(Guid.Parse(classId));
        if (classEntity == null)
            throw new ApiException("Class not found", 404);

        // Remove all student enrollments first
        await _userClassRepository.RemoveAllByClassIdAsync(Guid.Parse(classId));

        // Delete the class
        return await _classRepository.DeleteAsync(Guid.Parse(classId));
    }

    public async Task<ClassStatisticsResponse> GetClassStatisticsAsync()
    {
        var allClasses = await _classRepository.GetAllAsync();
        var activeClasses = allClasses.Where(c => c.IsActive && !c.IsArchived).ToList();
        var archivedClasses = allClasses.Where(c => c.IsArchived).ToList();

        // Get all enrollments
        var allEnrollments = await _userClassRepository.GetAllAsync();
        
        // Calculate statistics
        var totalStudentsEnrolled = allEnrollments.Select(e => e.StudentId).Distinct().Count();
        var teacherIds = allClasses.Select(c => c.TeacherId).Distinct().ToList();
        
        var averageStudentsPerClass = allClasses.Any() 
            ? (double)allEnrollments.Count() / allClasses.Count() 
            : 0;

        // Find most popular class
        var classStudentCounts = allClasses
            .Select(c => new
            {
                Class = c,
                StudentCount = allEnrollments.Count(e => e.ClassId == c.ClassId)
            })
            .OrderByDescending(x => x.StudentCount)
            .FirstOrDefault();

        ClassWithMostStudents? mostPopularClass = null;
        if (classStudentCounts != null)
        {
            mostPopularClass = new ClassWithMostStudents
            {
                ClassId = classStudentCounts.Class.ClassId,
                ClassName = classStudentCounts.Class.Name,
                StudentCount = classStudentCounts.StudentCount
            };
        }

        // Get top teachers by class count
        var teacherClassCounts = allClasses
            .GroupBy(c => c.TeacherId)
            .Select(g => new
            {
                TeacherId = g.Key,
                ClassCount = g.Count()
            })
            .OrderByDescending(x => x.ClassCount)
            .Take(5)
            .ToList();

        var topTeachers = new List<TeacherClassCount>();
        foreach (var tcc in teacherClassCounts)
        {
            var teacher = await _teacherRepository.GetByIdAsync(tcc.TeacherId);
            if (teacher != null)
            {
                topTeachers.Add(new TeacherClassCount
                {
                    TeacherId = tcc.TeacherId,
                    TeacherName = teacher.FullName ?? "Unknown",
                    ClassCount = tcc.ClassCount
                });
            }
        }

        return new ClassStatisticsResponse
        {
            TotalClasses = allClasses.Count,
            ActiveClasses = activeClasses.Count,
            ArchivedClasses = archivedClasses.Count,
            TotalStudentsEnrolled = totalStudentsEnrolled,
            TotalTeachers = teacherIds.Count,
            AverageStudentsPerClass = Math.Round(averageStudentsPerClass, 2),
            MostPopularClass = mostPopularClass,
            TopTeachers = topTeachers
        };
    }

    public async Task<object> BulkActionAsync(string action, List<string> classIds, string? reason)
    {
        var results = new List<object>();
        var successCount = 0;
        var failureCount = 0;

        foreach (var classIdStr in classIds)
        {
            try
            {
                var classId = Guid.Parse(classIdStr);
                
                switch (action.ToLower())
                {
                    case "archive":
                        await ArchiveClassAsync(classIdStr, reason);
                        successCount++;
                        results.Add(new { classId = classIdStr, success = true });
                        break;
                        
                    case "unarchive":
                        await UnarchiveClassAsync(classIdStr);
                        successCount++;
                        results.Add(new { classId = classIdStr, success = true });
                        break;
                        
                    case "delete":
                        await DeleteClassByAdminAsync(classIdStr);
                        successCount++;
                        results.Add(new { classId = classIdStr, success = true });
                        break;
                        
                    default:
                        throw new ApiException($"Invalid action: {action}", 400);
                }
            }
            catch (Exception ex)
            {
                failureCount++;
                results.Add(new { classId = classIdStr, success = false, error = ex.Message });
            }
        }

        return new
        {
            action,
            totalRequested = classIds.Count,
            successCount,
            failureCount,
            results
        };
    }

    public async Task<object> GetClassStudentsForAdminAsync(Guid classId, int pageNumber, int pageSize, string? searchTerm)
    {
        var classEntity = await _classRepository.GetByIdAsync(classId);
        if (classEntity == null)
            throw new ApiException("Class not found", 404);

        var userClassesQuery = await _userClassRepository.GetByClassIdAsync(classId);
        var studentIds = userClassesQuery.Select(uc => uc.StudentId).ToList();

        if (!studentIds.Any())
        {
            return new
            {
                items = new List<object>(),
                totalCount = 0,
                pageNumber,
                pageSize,
                totalPages = 0
            };
        }

        var studentsQuery = (await _studentRepository.GetAllAsync())
            .Where(s => studentIds.Contains(s.UserId))
            .AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            studentsQuery = studentsQuery.Where(s =>
                s.FullName.Contains(searchTerm) ||
                s.StudentCode.Contains(searchTerm) ||
                s.Email.Contains(searchTerm));
        }

        var totalCount = studentsQuery.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var studentsList = studentsQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var students = studentsList
            .Select(s =>
            {
                var userClass = userClassesQuery.FirstOrDefault(uc => uc.StudentId == s.UserId);
                return new
                {
                    userId = s.UserId,
                    fullName = s.FullName,
                    email = s.Email,
                    studentCode = s.StudentCode,
                    isActive = userClass?.IsActive ?? false,
                    joinedAt = userClass?.JoinedAt
                };
            })
            .ToList();

        return new
        {
            items = students,
            totalCount,
            pageNumber,
            pageSize,
            totalPages
        };
    }
}
