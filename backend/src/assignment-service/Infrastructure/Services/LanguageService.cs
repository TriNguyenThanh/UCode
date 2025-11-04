using AutoMapper;
using AssignmentService.Application.DTOs.Common;
using AssignmentService.Application.Interfaces.Repositories;
using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Domain.Entities;

namespace AssignmentService.Infrastructure.Services;

/// <summary>
/// Service implementation for Language entity - business logic cho quản lý ngôn ngữ
/// </summary>
public class LanguageService : ILanguageService
{
    private readonly ILanguageRepository _languageRepository;
    private readonly IMapper _mapper;

    public LanguageService(ILanguageRepository languageRepository, IMapper mapper)
    {
        _languageRepository = languageRepository;
        _mapper = mapper;
    }

    public async Task<List<LanguageDto>> GetAllLanguagesAsync(bool includeDisabled = false)
    {
        var languages = await _languageRepository.GetAllLanguagesAsync(includeDisabled);
        return _mapper.Map<List<LanguageDto>>(languages);
    }

    public async Task<LanguageDto?> GetLanguageByIdAsync(Guid languageId)
    {
        var language = await _languageRepository.GetByIdAsync(languageId);
        return language == null ? null : _mapper.Map<LanguageDto>(language);
    }

    public async Task<LanguageDto?> GetLanguageByCodeAsync(string code)
    {
        var language = await _languageRepository.GetLanguageByCodeAsync(code);
        return language == null ? null : _mapper.Map<LanguageDto>(language);
    }

    public async Task<LanguageDto> CreateLanguageAsync(LanguageDto languageDto)
    {
        var codeExists = await _languageRepository.CodeExistsAsync(languageDto.Code);
        if (codeExists)
        {
            throw new ApiException($"Language with code '{languageDto.Code}' already exists", 400);
        }

        var language = _mapper.Map<Language>(languageDto);
        language.LanguageId = Guid.NewGuid();

        var createdLanguage = await _languageRepository.AddAsync(language);
        
        return _mapper.Map<LanguageDto>(createdLanguage);
    }

    public async Task<LanguageDto> UpdateLanguageAsync(Guid languageId, LanguageDto languageDto)
    {
        var existingLanguage = await _languageRepository.GetByIdAsync(languageId);
        if (existingLanguage == null)
        {
            throw new ApiException("Language not found", 404);
        }

        if (languageDto.Code != existingLanguage.Code)
        {
            var codeExists = await _languageRepository.CodeExistsAsync(languageDto.Code, languageId);
            if (codeExists)
            {
                throw new ApiException($"Language with code '{languageDto.Code}' already exists", 400);
            }
        }

        _mapper.Map(languageDto, existingLanguage);
        existingLanguage.LanguageId = languageId; 

        var updatedLanguage = await _languageRepository.UpdateAsync(existingLanguage);
        
        return _mapper.Map<LanguageDto>(updatedLanguage);
    }

    public async Task<bool> DeleteLanguageAsync(Guid languageId)
    {
        return await _languageRepository.SetLanguageEnabledAsync(languageId, false);
    }

    public async Task<bool> EnableLanguageAsync(Guid languageId)
    {
        return await _languageRepository.SetLanguageEnabledAsync(languageId, true);
    }
}
