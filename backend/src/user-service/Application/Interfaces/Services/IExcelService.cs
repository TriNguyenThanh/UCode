using UserService.Application.DTOs.Requests;

namespace UserService.Application.Interfaces.Services;

public interface IExcelService
{
    Task<List<CreateStudentRequest>> ImportStudentsFromExcelAsync(Stream fileStream);
    Task<byte[]> ExportStudentsToExcelAsync(List<string> studentIds);
}

