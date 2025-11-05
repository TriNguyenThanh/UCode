using AutoMapper;
using UserService.Application.DTOs.Responses;
using UserService.Domain.Entities;

namespace UserService.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserResponse>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));
        
        // Student mappings
        CreateMap<Student, StudentResponse>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.StudentCode, opt => opt.MapFrom(src => src.StudentCode))
            .ForMember(dest => dest.Classes, opt => opt.MapFrom(src => 
                src.UserClasses.Select(uc => uc.Class).ToList()));
        
        // Teacher mappings
        CreateMap<Teacher, TeacherResponse>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.TeacherCode, opt => opt.MapFrom(src => src.TeacherCode))
            .ForMember(dest => dest.ClassCount, opt => opt.MapFrom(src => src.Classes.Count));
        
        // Admin mappings
        CreateMap<Admin, AdminResponse>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));
        
        // Class mappings
        CreateMap<Class, ClassResponse>()
            .ForMember(dest => dest.ClassId, opt => opt.MapFrom(src => src.ClassId))
            .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher.FullName))
            .ForMember(dest => dest.StudentCount, opt => opt.MapFrom(src => src.UserClasses.Count));
        
        CreateMap<Class, ClassDetailResponse>()
            .ForMember(dest => dest.ClassId, opt => opt.MapFrom(src => src.ClassId))
            .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher.FullName))
            .ForMember(dest => dest.StudentCount, opt => opt.MapFrom(src => src.UserClasses.Count))
            .ForMember(dest => dest.Students, opt => opt.MapFrom(src => 
                src.UserClasses.Select(uc => uc.Student).ToList()))
            .ForMember(dest => dest.Statistics, opt => opt.MapFrom(src => new ClassStatistics
            {
                TotalStudents = src.UserClasses.Count,
                ActiveStudents = src.UserClasses.Count(uc => uc.Student.Status == Domain.Enums.UserStatus.Active),
                InactiveStudents = src.UserClasses.Count(uc => uc.Student.Status == Domain.Enums.UserStatus.Inactive)
            }));
    }
}

