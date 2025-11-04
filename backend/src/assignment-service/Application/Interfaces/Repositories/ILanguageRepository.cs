using AssignmentService.Domain.Entities;

namespace AssignmentService.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface cho Language entity
/// </summary>
public interface ILanguageRepository : IRepository<Language>
{
    /// <summary>
    /// Lấy tất cả ngôn ngữ, có thể lọc theo trạng thái enabled
    /// </summary>
    Task<List<Language>> GetAllLanguagesAsync(bool includeDisabled = false);
    
    /// <summary>
    /// Lấy ngôn ngữ theo code (ví dụ: "cpp", "java")
    /// </summary>
    Task<Language?> GetLanguageByCodeAsync(string code);
    
    /// <summary>
    /// Kiểm tra xem code đã tồn tại chưa (dùng khi create/update)
    /// </summary>
    Task<bool> CodeExistsAsync(string code, Guid? excludeLanguageId = null);
    
    /// <summary>
    /// Enable/disable một ngôn ngữ
    /// </summary>
    Task<bool> SetLanguageEnabledAsync(Guid languageId, bool isEnabled);
}
