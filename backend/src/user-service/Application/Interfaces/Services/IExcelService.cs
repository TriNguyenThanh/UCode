using UserService.Application.DTOs.Requests;

namespace UserService.Application.Interfaces.Services;

public interface IExcelService
{
    Task<List<CreateStudentRequest>> ImportStudentsFromExcelAsync(Stream fileStream);
    Task<byte[]> ExportStudentsToExcelAsync(List<string> studentIds);
    
    /// <summary>
    /// Generate Excel template để import sinh viên
    /// </summary>
    /// <param name="classId">Optional: ID lớp để loại trừ sinh viên đã enroll</param>
    /// <returns>Excel file bytes với template hoặc pre-filled data</returns>
    Task<byte[]> GenerateImportTemplateAsync(string? classId = null);
}
