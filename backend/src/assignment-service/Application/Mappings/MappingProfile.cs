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
        CreateMap<Problem, ProblemResponse>();
        CreateMap<ProblemRequest, Problem>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
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

        CreateMap<LanguageLimit, LanguageLimitDto>();
        CreateMap<CodeTemplate, CodeTemplateDto>();
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
    }
}
