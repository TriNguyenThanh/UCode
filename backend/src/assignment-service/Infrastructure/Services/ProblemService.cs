using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Application.DTOs.Common;
using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;
using AssignmentService.Application.Interfaces.Repositories;
using System.Data.Common;
using AutoMapper;

namespace AssignmentService.Infrastructure.Services;

public class ProblemService : IProblemService
{
    private readonly IProblemRepository _problemRepository;
    private readonly IMapper _mapper;
    private const int MaxGenerateRetries = 5;

    public ProblemService(IProblemRepository problemRepository, IMapper mapper)
    {
        _problemRepository = problemRepository;
        _mapper = mapper;
    }
    private string FormatCode(string seq) => $"P{seq:000}";

    public async Task<Problem> CreateProblemAsync(string code, string title, Difficulty difficulty, Guid ownerId, Visibility visibility = Visibility.PRIVATE)
    {
        //test thì để, không thì comment
        // if (string.IsNullOrWhiteSpace(code))
        // {
        //     code = await GenerateUniqueCodeAsync();
        //     if (await _problemRepository.CodeExistsAsync(code))
        //         throw new ApiException("Generated Code already exists, please try again", 500);
        // }

        var problem = new Problem
        {
            ProblemId = Guid.NewGuid(),
            Code = code,
            Title = title,
            Difficulty = difficulty,
            OwnerId = ownerId,
            Slug = GenerateSlug(title),
            Visibility = visibility,
            Status = ProblemStatus.DRAFT
        };

        return await _problemRepository.AddAsync(problem);

    }

    public async Task<Problem> GetProblemByIdAsync(Guid problemId)
    {
        return await _problemRepository.GetByIdAsync(problemId) 
            ?? throw new ApiException("Problem not found");
    }

    public async Task<List<Problem>> GetProblemsByOwnerIdAsync(Guid ownerId)
    {
        return await _problemRepository.FindAsync(p => p.OwnerId == ownerId);
    }

    public async Task<List<Problem>> GetPublicProblemsAsync()
    {
        return await _problemRepository.FindAsync(p => p.Visibility == Visibility.PUBLIC && p.Status == ProblemStatus.PUBLISHED);
    }

    private async Task<string> GenerateUniqueCodeAsync()
    {
        for (int i = 0; i < MaxGenerateRetries; i++)
        {
            var seq = await _problemRepository.GetNextCodeSequenceAsync();
            var candidate = FormatCode(seq);
            if (!await _problemRepository.CodeExistsAsync(candidate))
                return candidate;
        }
        throw new ApiException("Unable to generate unique problem code", 500);
    }

    private string GenerateSlug(string title)
    {
        // Simple slug generation: lowercase, replace spaces with hyphens, remove invalid chars
        var slug = title.ToLower().Replace(" ", "-");
        slug = slug + "-" + DateTime.UtcNow.Ticks; // Ensure uniqueness with timestamp
        return slug;
    }

    public async Task<bool> DeleteProblemAsync(Guid problemId)
    {
        return await _problemRepository.RemoveAsync(problemId);
    }

    public async Task<Problem> UpdateProblemAsync(Problem problem)
    {   
        problem.Slug = GenerateSlug(problem.Title);
        problem.UpdatedAt = DateTime.UtcNow;
        
        // If ProblemAssets are provided, handle them
        if (problem.ProblemAssets != null && problem.ProblemAssets.Any())
        {
            // Get existing problem with assets
            var existingProblem = await _problemRepository.GetByIdWithDetailsAsync(problem.ProblemId);
            if (existingProblem != null)
            {
                // Clear existing assets (cascade delete will handle this)
                existingProblem.ProblemAssets.Clear();
                
                // Add new assets
                foreach (var asset in problem.ProblemAssets)
                {
                    asset.ProblemId = problem.ProblemId;
                    asset.CreatedAt = DateTime.UtcNow;
                    existingProblem.ProblemAssets.Add(asset);
                }
            }
        }
        
        return await _problemRepository.UpdateAsync(problem);
    }
    
    public async Task<List<Dataset>> GetDatasetsByProblemIdAsync(Guid problemId)
    {
        return await _problemRepository.GetDatasetsByProblemIdAsync(problemId);
    }

    public async Task<Guid?> GetProblemOwnerIdAsync(Guid problemId)
    {
        return await _problemRepository.GetProblemOwnerIdAsync(problemId);
    }

    public async Task<(bool exists, Guid? ownerId)> CheckExistsAndGetOwnerAsync(Guid problemId)
    {
        return await _problemRepository.CheckExistsAndGetOwnerAsync(problemId);
    }

    public async Task<(bool exists, Guid? ownerId, Visibility visibility)> GetProblemBasicInfoAsync(Guid problemId)
    {
        return await _problemRepository.GetProblemBasicInfoAsync(problemId);
    }

    // Pagination
    public async Task<(List<Problem> problems, int total)> GetProblemsByOwnerIdWithPaginationAsync(Guid ownerId, int page, int pageSize)
    {
        return await _problemRepository.GetByOwnerIdWithPaginationAsync(ownerId, page, pageSize);
    }

    // ProblemAsset methods
    public async Task<List<ProblemAsset>> GetProblemAssetsAsync(Guid problemId)
    {
        return await _problemRepository.GetProblemAssetsAsync(problemId);
    }

    public async Task<ProblemAsset> AddProblemAssetAsync(Guid problemId, CreateProblemAssetDto request)
    {
        var asset = _mapper.Map<ProblemAsset>(request);
        asset.ProblemId = problemId;
        asset.CreatedAt = DateTime.UtcNow;
        
        return await _problemRepository.AddProblemAssetAsync(asset);
    }

    public async Task<ProblemAsset> UpdateProblemAssetAsync(Guid problemId, Guid assetId, UpdateProblemAssetDto request)
    {
        var existingAsset = await _problemRepository.GetProblemAssetByIdAsync(assetId);
        if (existingAsset == null || existingAsset.ProblemId != problemId)
            throw new ApiException("Asset not found");

        _mapper.Map(request, existingAsset);
        
        await _problemRepository.UpdateProblemAssetAsync(existingAsset);
        
        return existingAsset;
    }

    public async Task<bool> DeleteProblemAssetAsync(Guid problemId, Guid assetId)
    {
        var asset = await _problemRepository.GetProblemAssetByIdAsync(assetId);
        if (asset == null || asset.ProblemId != problemId)
            throw new ApiException("Asset not found");
            
        return await _problemRepository.DeleteProblemAssetAsync(assetId);
    }

    // Tag methods
    public async Task AddTagsToProblemAsync(Guid problemId, List<Guid> tagIds)
    {
        await _problemRepository.AddProblemTagsAsync(problemId, tagIds);
    }

    public async Task RemoveTagFromProblemAsync(Guid problemId, Guid tagId)
    {
        await _problemRepository.RemoveProblemTagAsync(problemId, tagId);
    }

    public async Task<List<Tag>> GetAllTagsAsync()
    {
        return await _problemRepository.GetAllTagsAsync();
    }

    public async Task<List<Problem>> GetProblemsByTagAsync(string tagName)
    {
        return await _problemRepository.GetByTagNameAsync(tagName);
    }

    // LanguageLimit methods
    public async Task<List<LanguageLimit>> GetLanguageLimitsAsync(Guid problemId)
    {
        return await _problemRepository.GetLanguageLimitsAsync(problemId);
    }

    public async Task<LanguageLimit> AddOrUpdateLanguageLimitAsync(Guid problemId, LanguageLimitDto request)
    {
        // Check if language limit already exists
        LanguageLimit? existingLimit = null;
        if (request.LanguageLimitId.HasValue)
        {
            existingLimit = await _problemRepository.GetLanguageLimitByIdAsync(request.LanguageLimitId.Value);
        }
        else
        {
            existingLimit = await _problemRepository.GetLanguageLimitByLangAsync(problemId, request.Lang);
        }

        if (existingLimit != null)
        {
            // Update existing
            _mapper.Map(request, existingLimit);
            await _problemRepository.UpdateLanguageLimitAsync(existingLimit);
            return existingLimit;
        }
        else
        {
            // Add new
            var limit = _mapper.Map<LanguageLimit>(request);
            limit.ProblemId = problemId;
            return await _problemRepository.AddLanguageLimitAsync(limit);
        }
    }

    public async Task<bool> DeleteLanguageLimitAsync(Guid problemId, Guid limitId)
    {
        var limit = await _problemRepository.GetLanguageLimitByIdAsync(limitId);
        if (limit == null || limit.ProblemId != problemId)
            throw new ApiException("Language limit not found");
            
        return await _problemRepository.DeleteLanguageLimitAsync(limitId);
    }

    // Search methods
    public async Task<(List<Problem> problems, int total)> SearchProblemsAsync(string? keyword, string? difficulty, int page, int pageSize)
    {
        return await _problemRepository.SearchProblemsAsync(keyword, difficulty, page, pageSize);
    }
}