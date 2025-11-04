using AutoMapper;
using AssignmentService.Application.DTOs.Requests;
using AssignmentService.Application.DTOs.Responses;
using AssignmentService.Application.DTOs.Common;
using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;


namespace AssignmentService.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Problem, ProblemResponse>()
            .ForMember(dest => dest.ProblemAssets, opt => opt.MapFrom(src => src.ProblemAssets))
            .ForMember(dest => dest.TagNames, opt => opt.MapFrom(src => 
                src.ProblemTags != null 
                    ? src.ProblemTags.Where(pt => pt.Tag != null).Select(pt => pt.Tag.Name).ToList() 
                    : new List<string>()))
            .ForMember(dest => dest.ProblemLanguages, opt => opt.MapFrom(src => src.ProblemLanguages));

        CreateMap<ProblemRequest, Problem>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ProblemAssets, opt => opt.Ignore()); // Handle separately in service
        // CreateMap<UpdateProblemRequest, Problem>();

        // // Examination mappings
        // CreateMap<Examination, ExaminationResponse>();
        // CreateMap<CreateExaminationRequest, Examination>();
        // CreateMap<UpdateExaminationRequest, Examination>();
        CreateMap<Tag, TagDto>();

        CreateMap<Dataset, DatasetDto>()
            .ForMember(d => d.Kind, o => o.MapFrom(s => s.Kind.ToString()))
            .ReverseMap();
        CreateMap<UpdateDatasetDto, Dataset>()
            .ForMember(d => d.Kind, o => o.MapFrom(s => s.Kind.ToString()));

        CreateMap<TestCase, TestCaseDto>().ReverseMap();

        // Language and ProblemLanguage mappings
        CreateMap<Language, LanguageDto>().ReverseMap();
        
        CreateMap<ProblemLanguage, ProblemLanguageDto>()
            .ForMember(dest => dest.LanguageCode, opt => opt.MapFrom(src => src.Language.Code))
            .ForMember(dest => dest.LanguageDisplayName, opt => opt.MapFrom(src => src.Language.DisplayName))
            .ForMember(dest => dest.TimeFactor, opt => opt.MapFrom(src => 
                src.TimeFactorOverride ?? src.Language.DefaultTimeFactor))
            .ForMember(dest => dest.MemoryKb, opt => opt.MapFrom(src => 
                src.MemoryKbOverride ?? src.Language.DefaultMemoryKb))
            .ForMember(dest => dest.Head, opt => opt.MapFrom(src => 
                src.HeadOverride ?? src.Language.DefaultHead))
            .ForMember(dest => dest.Body, opt => opt.MapFrom(src => 
                src.BodyOverride ?? src.Language.DefaultBody))
            .ForMember(dest => dest.Tail, opt => opt.MapFrom(src => 
                src.TailOverride ?? src.Language.DefaultTail));
        
        CreateMap<ProblemLanguageDto, ProblemLanguage>()
            .ForMember(dest => dest.TimeFactorOverride, opt => opt.MapFrom(src => src.TimeFactor))
            .ForMember(dest => dest.MemoryKbOverride, opt => opt.MapFrom(src => src.MemoryKb))
            .ForMember(dest => dest.HeadOverride, opt => opt.MapFrom(src => src.Head))
            .ForMember(dest => dest.BodyOverride, opt => opt.MapFrom(src => src.Body))
            .ForMember(dest => dest.TailOverride, opt => opt.MapFrom(src => src.Tail))
            .ForMember(dest => dest.Language, opt => opt.Ignore())
            .ForMember(dest => dest.Problem, opt => opt.Ignore());
        
        CreateMap<ProblemAsset, ProblemAssetDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));

        // Assignment mappings
        CreateMap<Assignment, AssignmentDto>().ReverseMap();
        CreateMap<Assignment, AssignmentResponse>()
            .ForMember(dest => dest.AssignmentType, opt => opt.MapFrom(src => src.AssignmentType.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Problems, opt => opt.MapFrom(src => src.AssignmentProblems));
        
        CreateMap<AssignmentRequest, Assignment>()
            .ForMember(dest => dest.AssignmentProblems, opt => opt.MapFrom(src => src.Problems))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status ?? AssignmentStatus.DRAFT));

        // AssignmentUser mappings
        CreateMap<AssignmentUser, AssignmentUserDto>().ReverseMap();

        // AssignmentProblem mappings
        CreateMap<AssignmentProblem, AssignmentProblemDto>()
            .ForMember(dest => dest.ProblemTitle, opt => opt.MapFrom(src => src.Problem.Title))
            .ReverseMap() 
            .ForMember(dest => dest.Problem, opt => opt.Ignore()) 
            .ForMember(dest => dest.Assignment, opt => opt.Ignore());
        CreateMap<AssignmentProblem, AssignmentProblemDetailDto>()
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Problem.Code))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Problem.Title))
            .ForMember(dest => dest.Difficulty, opt => opt.MapFrom(src => src.Problem.Difficulty.ToString()));
        
        CreateMap<BestSubmission, BestSubmissionDto>().ReverseMap();

        CreateMap<AssignmentProblemDto, AssignmentProblem>()
            .ForMember(dest => dest.Assignment, opt => opt.Ignore())
            .ForMember(dest => dest.Problem, opt => opt.Ignore());


        // Submission mappings
        CreateMap<Submission, SubmissionResponse>();
        CreateMap<SubmissionRequest, Submission>();
        CreateMap<Submission, CreateSubmissionResponse>();

        // BestSubmission mappings
        CreateMap<BestSubmission, BestSubmissionResponse>();
        // ProblemAsset Mappings
        CreateMap<ProblemAsset, ProblemAssetDto>();

        CreateMap<CreateProblemAssetDto, ProblemAsset>()
            .ForMember(dest => dest.ProblemAssetId, opt => opt.Ignore())
            .ForMember(dest => dest.ProblemId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Problem, opt => opt.Ignore());

        CreateMap<UpdateProblemAssetDto, ProblemAsset>()
            .ForMember(dest => dest.ProblemId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Problem, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
