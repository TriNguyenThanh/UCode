using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Application.Interfaces.Repositories;
using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;
using AssignmentService.Application.DTOs.Common;
using System.Data.Common;

namespace AssignmentService.Infrastructure.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<List<Tag>> GetAllTagsAsync(string? category = null)
    {
        try
        {
            if (string.IsNullOrEmpty(category))
            {
                return await _tagRepository.GetAllAsync();
            }

            if (Enum.TryParse<TagCategory>(category, true, out var tagCategory))
            {
                return await _tagRepository.FindAsync(t => t.Category == tagCategory);
            }

            return await _tagRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving tags: {ex.Message}", 500);
        }
    }

    public async Task<Tag> GetTagByIdAsync(Guid tagId)
    {
        try
        {
            var tag = await _tagRepository.GetByIdAsync(tagId);
            if (tag == null)
                throw new ApiException("Tag not found", 404);
            
            return tag;
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving tag: {ex.Message}", 500);
        }
    }

    public async Task<Tag?> GetTagByNameAsync(string name)
    {
        try
        {
            var tags = await _tagRepository.FindAsync(t => t.Name == name);
            return tags.FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving tag by name: {ex.Message}", 500);
        }
    }

    public async Task<Tag> CreateTagAsync(string name, TagCategory category)
    {
        try
        {
            // Check if tag already exists
            if (await TagExistsAsync(name))
                throw new ApiException("Tag with this name already exists", 400);

            var tag = new Tag
            {
                TagId = Guid.NewGuid(),
                Name = name,
                Category = category
            };

            return await _tagRepository.AddAsync(tag);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (DbException ex)
        {
            throw new ApiException($"Database error while creating tag: {ex.Message}", 500);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error creating tag: {ex.Message}", 500);
        }
    }

    public async Task<Tag> UpdateTagAsync(Guid tagId, string name, TagCategory category)
    {
        try
        {
            var tag = await GetTagByIdAsync(tagId);

            // Check if new name conflicts with existing tag
            if (tag.Name != name && await TagExistsAsync(name))
                throw new ApiException("Tag with this name already exists", 400);

            tag.Name = name;
            tag.Category = category;

            return await _tagRepository.UpdateAsync(tag);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (DbException ex)
        {
            throw new ApiException($"Database error while updating tag: {ex.Message}", 500);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error updating tag: {ex.Message}", 500);
        }
    }

    public async Task<bool> DeleteTagAsync(Guid tagId)
    {
        try
        {
            // Check if tag exists
            await GetTagByIdAsync(tagId);

            return await _tagRepository.RemoveAsync(tagId);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (DbException ex)
        {
            throw new ApiException($"Database error while deleting tag: {ex.Message}", 500);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error deleting tag: {ex.Message}", 500);
        }
    }

    public async Task<List<Problem>> GetProblemsByTagIdAsync(Guid tagId)
    {
        try
        {
            // Verify tag exists
            await GetTagByIdAsync(tagId);

            return await _tagRepository.GetProblemsByTagIdAsync(tagId);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error retrieving problems by tag: {ex.Message}", 500);
        }
    }

    public async Task<bool> TagExistsAsync(string name)
    {
        try
        {
            return await _tagRepository.AnyAsync(t => t.Name == name);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error checking tag existence: {ex.Message}", 500);
        }
    }
}
