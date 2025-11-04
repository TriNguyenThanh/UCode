using Microsoft.AspNetCore.Mvc;
using AssignmentService.Application.DTOs.Common;
using AssignmentService.Application.DTOs.Responses;
using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Api.Filters;
using AssignmentService.Api.Middlewares;

namespace AssignmentService.Api.Controllers;

[ApiController]
[Route("api/v1/languages")]
public class LanguageController : ControllerBase
{
    private readonly ILanguageService _languageService;

    public LanguageController(ILanguageService languageService)
    {
        _languageService = languageService;
    }

    /// <summary>
    /// Get all available programming languages
    /// </summary>
    /// <param name="includeDisabled">Include disabled languages</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<LanguageDto>>), 200)]
    public async Task<IActionResult> GetAllLanguages([FromQuery] bool includeDisabled = false)
    {
        var languageDtos = await _languageService.GetAllLanguagesAsync(includeDisabled);
        return Ok(ApiResponse<List<LanguageDto>>.SuccessResponse(languageDtos));
    }

    /// <summary>
    /// Get a specific language by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<LanguageDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<IActionResult> GetLanguage(Guid id)
    {
        var languageDto = await _languageService.GetLanguageByIdAsync(id);
        
        if (languageDto == null)
        {
            return NotFound(ApiResponse<LanguageDto>.ErrorResponse("Language not found"));
        }
        
        return Ok(ApiResponse<LanguageDto>.SuccessResponse(languageDto));
    }

    /// <summary>
    /// Get a specific language by code
    /// </summary>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(ApiResponse<LanguageDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<IActionResult> GetLanguageByCode(string code)
    {
        var languageDto = await _languageService.GetLanguageByCodeAsync(code);
        
        if (languageDto == null)
        {
            return NotFound(ApiResponse<LanguageDto>.ErrorResponse($"Language with code '{code}' not found"));
        }
        
        return Ok(ApiResponse<LanguageDto>.SuccessResponse(languageDto));
    }

    /// <summary>
    /// Create a new language (Admin only)
    /// </summary>
    [RequireRole("admin")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<LanguageDto>), 201)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ForbiddenErrorResponse), 403)]
    public async Task<IActionResult> CreateLanguage([FromBody] LanguageDto request)
    {
        try
        {
            var languageDto = await _languageService.CreateLanguageAsync(request);
            
            return CreatedAtAction(
                nameof(GetLanguage), 
                new { id = languageDto.LanguageId }, 
                ApiResponse<LanguageDto>.SuccessResponse(languageDto, "Language created successfully")
            );
        }
        catch (ApiException ex)
        {
            return BadRequest(ApiResponse<LanguageDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Update an existing language (Admin only)
    /// </summary>
    [RequireRole("admin")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<LanguageDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ForbiddenErrorResponse), 403)]
    public async Task<IActionResult> UpdateLanguage(Guid id, [FromBody] LanguageDto request)
    {
        try
        {
            var languageDto = await _languageService.UpdateLanguageAsync(id, request);
            return Ok(ApiResponse<LanguageDto>.SuccessResponse(languageDto, "Language updated successfully"));
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return NotFound(ApiResponse<LanguageDto>.ErrorResponse(ex.Message));
        }
        catch (ApiException ex)
        {
            return BadRequest(ApiResponse<LanguageDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Delete a language (Admin only) - Soft delete by disabling
    /// </summary>
    [RequireRole("admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ForbiddenErrorResponse), 403)]
    public async Task<IActionResult> DeleteLanguage(Guid id)
    {
        var success = await _languageService.DeleteLanguageAsync(id);
        
        if (!success)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse("Language not found"));
        }
        
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Language disabled successfully"));
    }

    /// <summary>
    /// Enable a disabled language (Admin only)
    /// </summary>
    [RequireRole("admin")]
    [HttpPost("{id:guid}/enable")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(UnauthorizedErrorResponse), 401)]
    [ProducesResponseType(typeof(ForbiddenErrorResponse), 403)]
    public async Task<IActionResult> EnableLanguage(Guid id)
    {
        var success = await _languageService.EnableLanguageAsync(id);
        
        if (!success)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse("Language not found"));
        }
        
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Language enabled successfully"));
    }
}
