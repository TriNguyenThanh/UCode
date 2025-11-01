using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;
using AssignmentService.Application.Interfaces.Repositories;
using System.Data.Common;
using AssignmentService.Application.DTOs.Common;

namespace AssignmentService.Infrastructure.Services;

public class ProblemService : IProblemService
{
    private readonly IProblemRepository _problemRepository;
    private const int MaxGenerateRetries = 5;

    public ProblemService(IProblemRepository problemRepository)
    {
        _problemRepository = problemRepository;
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
}