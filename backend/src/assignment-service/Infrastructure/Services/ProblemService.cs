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
    private readonly ILanguageService _languageService;
    private readonly IMapper _mapper;
    private const int MaxGenerateRetries = 5;

    public ProblemService(IProblemRepository problemRepository, ILanguageService languageService, IMapper mapper)
    {
        _problemRepository = problemRepository;
        _languageService = languageService;
        _mapper = mapper;
    }
    private string FormatCode(string seq) => $"P{seq:000}";

    public async Task<Problem> CreateProblemAsync(string code, string title, Difficulty difficulty, Guid ownerId, Visibility visibility = Visibility.PRIVATE)
    {
        try
        {
            //test thì để, không thì comment
            if (string.IsNullOrWhiteSpace(code))
            {
                code = await GenerateUniqueCodeAsync();
                if (await _problemRepository.CodeExistsAsync(code))
                    throw new ApiException("Generated Code already exists, please try again", 500);
            }

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
        catch (DbException ex)
        {
            throw new ApiException($"Database error while creating problem: {ex.Message}", 500);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException($"Unexpected error while creating problem: {ex.Message}", 500);
        }
    }

    public async Task<Problem> GetProblemByIdAsync(Guid problemId)
    {
        try
        {
            return await _problemRepository.GetByIdAsync(problemId)
                ?? throw new ApiException("Problem not found", 404);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving problem: {ex.Message}", 500);
        }
    }

    public async Task<List<Problem>> GetProblemsByOwnerIdAsync(Guid ownerId)
    {
        try
        {
            return await _problemRepository.FindAsync(p => p.OwnerId == ownerId);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving problems by owner: {ex.Message}", 500);
        }
    }

    public async Task<List<Problem>> GetPublicProblemsAsync()
    {
        try
        {
            return await _problemRepository.FindAsync(p => p.Visibility == Visibility.PUBLIC && p.Status == ProblemStatus.PUBLISHED);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving public problems: {ex.Message}", 500);
        }
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
        var slug = title.ToLower().Replace(" ", "-").Replace("--", "-").Replace("!", "").Replace("@", "").Replace("#", "")
                                   .Replace("$", "").Replace("%", "").Replace("^", "").Replace("&", "")
                                   .Replace("*", "").Replace("(", "").Replace(")", "").Replace("+", "")
                                   .Replace("=", "").Replace(",", "").Replace(".", "").Replace("/", "")
                                   .Replace("?", "").Replace("<", "").Replace(">", "").Replace("'", "")
                                   .Replace("\"", "").Replace(":", "").Replace(";", "").Replace("[", "")
                                   .Replace("]", "").Replace("{", "").Replace("}", "").Replace("|", "")
                                   .Replace("\\", "");

        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        slug = $"{slug}-{timestamp}";
        return slug;
    }

    public async Task<bool> DeleteProblemAsync(Guid problemId)
    {
        try
        {
            return await _problemRepository.RemoveAsync(problemId);
        }
        catch (DbException ex)
        {
            throw new ApiException($"Database error while deleting problem: {ex.Message}", 500);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error deleting problem: {ex.Message}", 500);
        }
    }

    public async Task<Problem> UpdateProblemAsync(Problem problem)
    {
        try
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
        catch (DbException ex)
        {
            throw new ApiException($"Database error while updating problem: {ex.Message}", 500);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error updating problem: {ex.Message}", 500);
        }
    }

    public async Task<List<Dataset>> GetDatasetsByProblemIdAsync(Guid problemId)
    {
        try
        {
            return await _problemRepository.GetDatasetsByProblemIdAsync(problemId);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving datasets: {ex.Message}", 500);
        }
    }

    public async Task<Guid?> GetProblemOwnerIdAsync(Guid problemId)
    {
        try
        {
            return await _problemRepository.GetProblemOwnerIdAsync(problemId);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving problem owner: {ex.Message}", 500);
        }
    }

    public async Task<(bool exists, Guid? ownerId)> CheckExistsAndGetOwnerAsync(Guid problemId)
    {
        try
        {
            return await _problemRepository.CheckExistsAndGetOwnerAsync(problemId);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error checking problem existence: {ex.Message}", 500);
        }
    }

    public async Task<(bool exists, Guid? ownerId, Visibility visibility)> GetProblemBasicInfoAsync(Guid problemId)
    {
        try
        {
            return await _problemRepository.GetProblemBasicInfoAsync(problemId);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving problem basic info: {ex.Message}", 500);
        }
    }

    // Pagination
    public async Task<(List<Problem> problems, int total)> GetProblemsByOwnerIdWithPaginationAsync(Guid ownerId, int page, int pageSize)
    {
        try
        {
            return await _problemRepository.GetByOwnerIdWithPaginationAsync(ownerId, page, pageSize);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving paginated problems: {ex.Message}", 500);
        }
    }

    // ProblemAsset methods
    public async Task<List<ProblemAsset>> GetProblemAssetsAsync(Guid problemId)
    {
        try
        {
            return await _problemRepository.GetProblemAssetsAsync(problemId);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving problem assets: {ex.Message}", 500);
        }
    }

    public async Task<ProblemAsset> AddProblemAssetAsync(Guid problemId, CreateProblemAssetDto request)
    {
        try
        {
            var asset = _mapper.Map<ProblemAsset>(request);
            asset.ProblemId = problemId;
            asset.CreatedAt = DateTime.UtcNow;

            return await _problemRepository.AddProblemAssetAsync(asset);
        }
        catch (DbException ex)
        {
            throw new ApiException($"Database error while adding asset: {ex.Message}", 500);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error adding problem asset: {ex.Message}", 500);
        }
    }

    public async Task<ProblemAsset> UpdateProblemAssetAsync(Guid problemId, Guid assetId, UpdateProblemAssetDto request)
    {
        try
        {
            var existingAsset = await _problemRepository.GetProblemAssetByIdAsync(assetId);
            if (existingAsset == null || existingAsset.ProblemId != problemId)
                throw new ApiException("Asset not found", 404);

            _mapper.Map(request, existingAsset);

            await _problemRepository.UpdateProblemAssetAsync(existingAsset);

            return existingAsset;
        }
        catch (ApiException)
        {
            throw;
        }
        catch (DbException ex)
        {
            throw new ApiException($"Database error while updating asset: {ex.Message}", 500);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error updating problem asset: {ex.Message}", 500);
        }
    }

    public async Task<bool> DeleteProblemAssetAsync(Guid problemId, Guid assetId)
    {
        try
        {
            var asset = await _problemRepository.GetProblemAssetByIdAsync(assetId);
            if (asset == null || asset.ProblemId != problemId)
                throw new ApiException("Asset not found", 404);

            return await _problemRepository.DeleteProblemAssetAsync(assetId);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (DbException ex)
        {
            throw new ApiException($"Database error while deleting asset: {ex.Message}", 500);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error deleting problem asset: {ex.Message}", 500);
        }
    }

    // Tag methods
    public async Task AddTagsToProblemAsync(Guid problemId, List<Guid> tagIds)
    {
        try
        {
            await _problemRepository.AddProblemTagsAsync(problemId, tagIds);
        }
        catch (DbException ex)
        {
            throw new ApiException($"Database error while adding tags: {ex.Message}", 500);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error adding tags to problem: {ex.Message}", 500);
        }
    }

    public async Task RemoveProblemTagAsync(Guid problemId, Guid tagId)
    {
        try
        {
            await _problemRepository.RemoveProblemTagAsync(problemId, tagId);
        }
        catch (DbException ex)
        {
            throw new ApiException($"Database error while removing tag: {ex.Message}", 500);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error removing tag from problem: {ex.Message}", 500);
        }
    }

    public async Task<List<Tag>> GetAllTagsAsync()
    {
        try
        {
            return await _problemRepository.GetAllTagsAsync();
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving tags: {ex.Message}", 500);
        }
    }

    public async Task<List<Problem>> GetProblemsByTagAsync(string tagName)
    {
        try
        {
            return await _problemRepository.GetByTagNameAsync(tagName);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving problems by tag: {ex.Message}", 500);
        }
    }

    // ProblemLanguage methods (using new Language + ProblemLanguage schema)
    public async Task<List<ProblemLanguage>> GetProblemLanguagesAsync(Guid problemId)
    {
        try
        {
            // Returns ProblemLanguage with Language navigation property loaded
            return await _problemRepository.GetProblemLanguagesAsync(problemId);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving language configurations: {ex.Message}", 500);
        }
    }

    public async Task<List<ProblemLanguage>> AddOrUpdateProblemLanguagesAsync(Guid problemId, List<ProblemLanguageDto> requests)
    {
        try
        {   //Cái này có tracking nhé
            var problem = await _problemRepository.GetByIdWithLanguagesAsync(problemId);
            if (problem == null)
            {
                throw new ApiException("Problem not found", 404);
            }

            // validate code language ID
            foreach (var request in requests)
            {
                var languageDto = await _languageService.GetLanguageByIdAsync(request.LanguageId);
                if (languageDto == null)
                {
                    throw new ApiException($"Language with ID '{request.LanguageId}' not found", 404);
                }
            }

            problem.ProblemLanguages.Clear();
            var _problemLanguages = new List<ProblemLanguage>();

            foreach (var request in requests)
            {
                var languageDto = await _languageService.GetLanguageByIdAsync(request.LanguageId);

                var problemLanguage = new ProblemLanguage
                {
                    ProblemId = problemId,
                    LanguageId = request.LanguageId,

                    TimeFactorOverride = request.TimeFactor.HasValue &&
                        request.TimeFactor.Value != languageDto!.DefaultTimeFactor
                        ? request.TimeFactor : null,

                    MemoryKbOverride = request.MemoryKb.HasValue &&
                        request.MemoryKb.Value != languageDto!.DefaultMemoryKb
                        ? request.MemoryKb : null,

                    HeadOverride = !string.IsNullOrEmpty(request.Head) &&
                        request.Head != languageDto!.DefaultHead
                        ? request.Head : null,

                    BodyOverride = !string.IsNullOrEmpty(request.Body) &&
                        request.Body != languageDto!.DefaultBody
                        ? request.Body : null,

                    TailOverride = !string.IsNullOrEmpty(request.Tail) &&
                        request.Tail != languageDto!.DefaultTail
                        ? request.Tail : null,

                    IsAllowed = request.IsAllowed
                };

                _problemLanguages.Add(problemLanguage);
            }

            problem.ProblemLanguages = _problemLanguages;

            // Save changes - EF will automatically handle all add/update/delete operations
            await _problemRepository.SaveChangesAsync();

            var updatedProblem = await _problemRepository.GetByIdWithLanguagesAsync(problemId);
            return updatedProblem?.ProblemLanguages.ToList() ?? new List<ProblemLanguage>();
        }
        catch (DbException ex)
        {
            throw new ApiException($"Database error while saving language configurations: {ex.Message}", 500);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error adding/updating language configurations: {ex.Message}", 500);
        }
    }

    public async Task<bool> DeleteProblemLanguageAsync(Guid problemId, Guid languageId)
    {
        try
        {
            var problemLanguage = await _problemRepository.GetProblemLanguageAsync(problemId, languageId);
            if (problemLanguage == null)
                throw new ApiException("Problem language configuration not found", 404);

            return await _problemRepository.DeleteProblemLanguageAsync(problemId, languageId);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (DbException ex)
        {
            throw new ApiException($"Database error while deleting language configuration: {ex.Message}", 500);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error deleting language configuration: {ex.Message}", 500);
        }
    }

    // Search methods
    public async Task<(List<Problem> problems, int total)> SearchProblemsAsync(string? keyword, string? difficulty, int page, int pageSize)
    {
        try
        {
            return await _problemRepository.SearchProblemsAsync(keyword, difficulty, page, pageSize);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error searching problems: {ex.Message}", 500);
        }
    }
}