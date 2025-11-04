using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using UserService.Application.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace UserService.Api.Controllers;

/// <summary>
/// Controller quản lý sinh viên
/// </summary>
[ApiController]
[Route("api/v1/students")]
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly IExcelService _excelService;

    public StudentController(IStudentService studentService, IExcelService excelService)
    {
        _studentService = studentService;
        _excelService = excelService;
    }

    /// <summary>
    /// Tạo sinh viên mới
    /// </summary>
    /// <param name="request">Thông tin sinh viên</param>
    /// <returns>Thông tin sinh viên đã tạo</returns>
    /// <response code="200">Sinh viên được tạo thành công</response>
    /// <response code="400">Dữ liệu không hợp lệ</response>
    [HttpPost("create")]
    [SwaggerOperation(Summary = "Tạo sinh viên", Description = "Tạo một sinh viên mới")]
    [SwaggerResponse(200, "Sinh viên được tạo thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Dữ liệu không hợp lệ", typeof(ApiResponse<object>))]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

        var student = await _studentService.CreateStudentAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(student, "Student created successfully"));
    }

    /// <summary>
    /// Lấy thông tin sinh viên theo ID
    /// </summary>
    /// <param name="id">ID sinh viên</param>
    /// <returns>Thông tin sinh viên</returns>
    /// <response code="200">Trả về thông tin sinh viên</response>
    /// <response code="404">Không tìm thấy sinh viên</response>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Lấy sinh viên theo ID", Description = "Lấy thông tin sinh viên theo ID")]
    [SwaggerResponse(200, "Thông tin sinh viên", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "Không tìm thấy sinh viên", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetStudent(string id)
    {
        var student = await _studentService.GetStudentByIdAsync(id);
        if (student == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Student not found"));

        return Ok(ApiResponse<object>.SuccessResponse(student, "Student retrieved successfully"));
    }

    /// <summary>
    /// Lấy thông tin sinh viên theo mã sinh viên
    /// </summary>
    /// <param name="studentId">Mã sinh viên</param>
    /// <returns>Thông tin sinh viên</returns>
    /// <response code="200">Trả về thông tin sinh viên</response>
    /// <response code="404">Không tìm thấy sinh viên</response>
    [HttpGet("by-student-id/{studentId}")]
    [SwaggerOperation(Summary = "Lấy sinh viên theo mã sinh viên", Description = "Lấy thông tin sinh viên theo mã sinh viên")]
    [SwaggerResponse(200, "Thông tin sinh viên", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "Không tìm thấy sinh viên", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetStudentByStudentId(string studentId)
    {
        var student = await _studentService.GetStudentByStudentIdAsync(studentId);
        if (student == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Student not found"));

        return Ok(ApiResponse<object>.SuccessResponse(student, "Student retrieved successfully"));
    }

    /// <summary>
    /// Lấy danh sách sinh viên
    /// </summary>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Số lượng mỗi trang</param>
    /// <param name="classId">ID lớp học (lọc)</param>
    /// <returns>Danh sách sinh viên</returns>
    /// <response code="200">Trả về danh sách sinh viên</response>
    [HttpGet]
    [SwaggerOperation(Summary = "Lấy danh sách sinh viên", Description = "Lấy danh sách sinh viên có phân trang và lọc theo lớp học")]
    [SwaggerResponse(200, "Danh sách sinh viên", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetStudents(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? classId = null)
    {
        var students = await _studentService.GetStudentsAsync(pageNumber, pageSize, classId);
        return Ok(ApiResponse<object>.SuccessResponse(students, "Students retrieved successfully"));
    }

    /// <summary>
    /// Cập nhật thông tin sinh viên
    /// </summary>
    /// <param name="request">Thông tin cập nhật</param>
    /// <returns>Trạng thái cập nhật</returns>
    /// <response code="200">Cập nhật thành công</response>
    /// <response code="400">Cập nhật thất bại</response>
    [HttpPut("update")]
    [SwaggerOperation(Summary = "Cập nhật sinh viên", Description = "Cập nhật thông tin sinh viên")]
    [SwaggerResponse(200, "Cập nhật thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Cập nhật thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> UpdateStudent([FromBody] UpdateStudentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

        var result = await _studentService.UpdateStudentAsync(request.UserId.ToString(), request);
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to update student"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Student updated successfully"));
    }

    /// <summary>
    /// Xóa sinh viên
    /// </summary>
    /// <param name="id">ID sinh viên</param>
    /// <returns>Trạng thái xóa</returns>
    /// <response code="200">Xóa thành công</response>
    /// <response code="400">Xóa thất bại</response>
    [HttpDelete("delete")]
    [SwaggerOperation(Summary = "Xóa sinh viên", Description = "Xóa một sinh viên")]
    [SwaggerResponse(200, "Xóa thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Xóa thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> DeleteStudent([FromQuery] string id)
    {
        var result = await _studentService.DeleteStudentAsync(id);
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to delete student"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Student deleted successfully"));
    }

    /// <summary>
    /// Import sinh viên từ file Excel
    /// </summary>
    /// <param name="file">File Excel chứa danh sách sinh viên</param>
    /// <returns>Kết quả import</returns>
    /// <response code="200">Import thành công</response>
    /// <response code="400">Import thất bại</response>
    [HttpPost("import-excel")]
    [SwaggerOperation(Summary = "Import sinh viên từ Excel", Description = "Import danh sách sinh viên từ file Excel (.xlsx, .xls)")]
    [SwaggerResponse(200, "Import thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Import thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> ImportStudentsFromExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.ErrorResponse("No file uploaded"));

        if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid file format. Only Excel files are allowed"));

        try
        {
            using var stream = file.OpenReadStream();
            var studentRequests = await _excelService.ImportStudentsFromExcelAsync(stream);

            var createdStudents = new List<object>();
            var errors = new List<string>();

            foreach (var request in studentRequests)
            {
                try
                {
                    var student = await _studentService.CreateStudentAsync(request);
                    createdStudents.Add(student);
                }
                catch (Exception ex)
                {
                    errors.Add($"Failed to create student {request.StudentId}: {ex.Message}");
                }
            }

            return Ok(ApiResponse<object>.SuccessResponse(
                new { 
                    TotalProcessed = studentRequests.Count,
                    SuccessCount = createdStudents.Count,
                    FailedCount = errors.Count,
                    Students = createdStudents,
                    Errors = errors
                },
                $"Imported {createdStudents.Count} out of {studentRequests.Count} students"
            ));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Failed to import Excel file: {ex.Message}"));
        }
    }

    /// <summary>
    /// Export sinh viên ra file Excel
    /// </summary>
    /// <param name="studentIds">Danh sách ID sinh viên cần export</param>
    /// <returns>File Excel</returns>
    /// <response code="200">Export thành công</response>
    /// <response code="400">Export thất bại</response>
    [HttpGet("export-excel")]
    [SwaggerOperation(Summary = "Export sinh viên ra Excel", Description = "Export danh sách sinh viên ra file Excel")]
    [SwaggerResponse(200, "Export thành công")]
    [SwaggerResponse(400, "Export thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> ExportStudentsToExcel([FromQuery] List<string> studentIds)
    {
        try
        {
            var fileBytes = await _excelService.ExportStudentsToExcelAsync(studentIds);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                $"Students_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Failed to export Excel file: {ex.Message}"));
        }
    }
}

// Helper DTO for UpdateStudent
public class UpdateStudentRequest : UpdateUserRequest
{
    // Inherits all properties from UpdateUserRequest
}

