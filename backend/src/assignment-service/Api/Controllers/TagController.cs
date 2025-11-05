using Microsoft.AspNetCore.Mvc;
using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Application.DTOs.Requests;
using AssignmentService.Application.DTOs.Responses;
using AssignmentService.Application.DTOs.Common;
using AssignmentService.Api.Middlewares;
using AutoMapper;

namespace AssignmentService.Api.Controllers;

/// <summary>
/// Controller for managing tags
/// </summary>
[ApiController]
[Route("api/v1/tags")]
// [ValidateUserId]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;
    private readonly IMapper _mapper;

    public TagController(ITagService tagService, IMapper mapper)
    {
        _tagService = tagService;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all tags with optional category filter
    /// </summary>
    /// <param name="category">Optional: Filter by category (Algorithm, DataStructure, Difficulty, Topic, Other)</param>
    /// <returns>List of all tags</returns>
    /// <response code="200">Tags retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<TagDto>>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GetAllTags([FromQuery] string? category = null)
    {
        var tags = await _tagService.GetAllTagsAsync(category);
        var tagDtos = _mapper.Map<List<TagDto>>(tags);
        
        return Ok(ApiResponse<List<TagDto>>.SuccessResponse(tagDtos, "Tags retrieved successfully"));
    }

    /// <summary>
    /// Get a specific tag by ID
    /// </summary>
    /// <param name="tagId">The unique identifier of the tag</param>
    /// <returns>Tag details</returns>
    /// <response code="200">Tag retrieved successfully</response>
    /// <response code="404">Tag not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{tagId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TagDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GetTagById(Guid tagId)
    {
        var tag = await _tagService.GetTagByIdAsync(tagId);
        var tagDto = _mapper.Map<TagDto>(tag);
        
        return Ok(ApiResponse<TagDto>.SuccessResponse(tagDto, "Tag retrieved successfully"));
    }

    /// <summary>
    /// Create a new tag (Teacher role required)
    /// </summary>
    /// <param name="request">Tag creation request</param>
    /// <returns>Created tag details</returns>
    /// <response code="200">Tag created successfully</response>
    /// <response code="400">Invalid request or tag already exists</response>
    /// <response code="401">Unauthorized - Teacher role required</response>
    /// <response code="500">Internal server error</response>
    [RequireRole("teacher,admin")]
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<TagDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest request)
    {
        var tag = await _tagService.CreateTagAsync(request.Name, request.Category);
        var tagDto = _mapper.Map<TagDto>(tag);
        
        return Ok(ApiResponse<TagDto>.SuccessResponse(tagDto, "Tag created successfully"));
    }

    /// <summary>
    /// Update an existing tag (Teacher role required)
    /// </summary>
    /// <param name="tagId">The unique identifier of the tag to update</param>
    /// <param name="request">Tag update request</param>
    /// <returns>Updated tag details</returns>
    /// <response code="200">Tag updated successfully</response>
    /// <response code="400">Invalid request or name conflict</response>
    /// <response code="401">Unauthorized - Teacher role required</response>
    /// <response code="404">Tag not found</response>
    /// <response code="500">Internal server error</response>
    [RequireRole("teacher,admin")]
    [HttpPut("update/{tagId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TagDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> UpdateTag(Guid tagId, [FromBody] UpdateTagRequest request)
    {
        var tag = await _tagService.UpdateTagAsync(tagId, request.Name, request.Category);
        var tagDto = _mapper.Map<TagDto>(tag);
        
        return Ok(ApiResponse<TagDto>.SuccessResponse(tagDto, "Tag updated successfully"));
    }

    /// <summary>
    /// Delete a tag (Admin role required)
    /// </summary>
    /// <param name="tagId">The unique identifier of the tag to delete</param>
    /// <returns>Success confirmation</returns>
    /// <response code="200">Tag deleted successfully</response>
    /// <response code="401">Unauthorized - Admin role required</response>
    /// <response code="404">Tag not found</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>
    /// This will also remove the tag from all problems that use it (cascade delete from ProblemTags table).
    /// </remarks>
    [RequireRole("admin")]
    [HttpDelete("{tagId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> DeleteTag(Guid tagId)
    {
        var result = await _tagService.DeleteTagAsync(tagId);
        
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Tag deleted successfully"));
    }

    /// <summary>
    /// Get all problems that use a specific tag
    /// </summary>
    /// <param name="tagId">The unique identifier of the tag</param>
    /// <returns>List of problems using this tag</returns>
    /// <response code="200">Problems retrieved successfully</response>
    /// <response code="404">Tag not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{tagId:guid}/problems")]
    [ProducesResponseType(typeof(ApiResponse<List<ProblemResponse>>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GetProblemsByTagId(Guid tagId)
    {
        var problems = await _tagService.GetProblemsByTagIdAsync(tagId);
        var problemDtos = _mapper.Map<List<ProblemResponse>>(problems);
        
        return Ok(ApiResponse<List<ProblemResponse>>.SuccessResponse(problemDtos, "Problems retrieved successfully"));
    }
}
