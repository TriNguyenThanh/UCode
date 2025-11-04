using AssignmentService.Application.DTOs.Common;
using AssignmentService.Domain.Entities;

namespace AssignmentService.Application.Interfaces.Services;

/// <summary>
/// Service interface cho Language entity - quản lý ngôn ngữ lập trình
/// </summary>
public interface ILanguageService
{
    /// <summary>
    /// Lấy tất cả ngôn ngữ (mặc định chỉ enabled, truyền true để bao gồm disabled)
    /// </summary>
    Task<List<LanguageDto>> GetAllLanguagesAsync(bool includeDisabled = false);
    
    /// <summary>
    /// Lấy ngôn ngữ theo ID
    /// </summary>
    Task<LanguageDto?> GetLanguageByIdAsync(Guid languageId);
    
    /// <summary>
    /// Lấy ngôn ngữ theo code (cpp, java, python, ...)
    /// </summary>
    Task<LanguageDto?> GetLanguageByCodeAsync(string code);
    
    /// <summary>
    /// Tạo ngôn ngữ mới (Admin only)
    /// </summary>
    Task<LanguageDto> CreateLanguageAsync(LanguageDto languageDto);
    
    /// <summary>
    /// Cập nhật ngôn ngữ (Admin only)
    /// </summary>
    Task<LanguageDto> UpdateLanguageAsync(Guid languageId, LanguageDto languageDto);
    
    /// <summary>
    /// Xóa ngôn ngữ (soft delete - disable)
    /// </summary>
    Task<bool> DeleteLanguageAsync(Guid languageId);
    
    /// <summary>
    /// Enable một ngôn ngữ đã bị disable
    /// </summary>
    Task<bool> EnableLanguageAsync(Guid languageId);
}
