using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using UserService.Application.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using UCode.UserService.Application.DTOs.Requests;

namespace UserService.Api.Controllers;

/// <summary>
/// Controller quản lý sinh viên
/// </summary>
[ApiController]
[Route("api/v1/students")]
[Authorize]
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
    /// Helper method để lấy UserId từ JWT token
    /// </summary>
    private string GetUserIdFromToken()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");
    }

    #region Student Self-Service APIs

    /// <summary>
    /// [STUDENT] Lấy thông tin cá nhân
    /// </summary>
    /// <returns>Thông tin sinh viên</returns>
    /// <response code="200">Trả về thông tin sinh viên</response>
    /// <response code="404">Không tìm thấy sinh viên</response>
    [HttpGet("me")]
    [Authorize(Roles = "Student")]
    [SwaggerOperation(Summary = "[STUDENT] Lấy thông tin cá nhân", Description = "Sinh viên lấy thông tin của chính mình từ JWT token")]
    [SwaggerResponse(200, "Thông tin sinh viên", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "Không tìm thấy sinh viên", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetUserIdFromToken();
        var student = await _studentService.GetStudentByIdAsync(userId);
        
        if (student == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Student not found"));

        return Ok(ApiResponse<object>.SuccessResponse(student, "Student retrieved successfully"));
    }

    /// <summary>
    /// [STUDENT] Cập nhật thông tin cá nhân
    /// </summary>
    /// <param name="request">Thông tin cập nhật (không cần userId, lấy từ token)</param>
    /// <returns>Trạng thái cập nhật</returns>
    /// <response code="200">Cập nhật thành công</response>
    /// <response code="400">Cập nhật thất bại</response>
    [HttpPut("me")]
    [Authorize(Roles = "Student")]
    [SwaggerOperation(Summary = "[STUDENT] Cập nhật thông tin cá nhân", Description = "Sinh viên cập nhật thông tin của chính mình (userId lấy từ JWT token)")]
    [SwaggerResponse(200, "Cập nhật thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Cập nhật thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateMyProfileRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

        var userId = GetUserIdFromToken();
        
        // Map từ UpdateMyProfileRequest sang UpdateUserRequest
        var updateRequest = new UpdateUserRequest
        {
            Email = request.Email,
            FullName = request.FullName,
            Phone = request.Phone
        };
        
        var result = await _studentService.UpdateStudentAsync(userId, updateRequest);
        
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to update student"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Student updated successfully"));
    }

    #endregion

    #region Admin & Teacher - Full Management APIs

    /// <summary>
    /// [ADMIN/TEACHER] Tạo sinh viên mới
    /// </summary>
    /// <param name="request">Thông tin sinh viên</param>
    /// <returns>Thông tin sinh viên đã tạo</returns>
    /// <response code="200">Sinh viên được tạo thành công</response>
    /// <response code="400">Dữ liệu không hợp lệ</response>
    [HttpPost("create")]
    [Authorize(Roles = "Admin,Teacher")]
    [SwaggerOperation(Summary = "[ADMIN/TEACHER] Tạo sinh viên", Description = "Admin hoặc Teacher tạo sinh viên mới")]
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
    /// [ADMIN/TEACHER] Lấy sinh viên theo ID
    /// </summary>
    /// <param name="id">ID sinh viên</param>
    /// <returns>Thông tin sinh viên</returns>
    /// <response code="200">Trả về thông tin sinh viên</response>
    /// <response code="404">Không tìm thấy sinh viên</response>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    [SwaggerOperation(Summary = "[ADMIN/TEACHER] Lấy sinh viên theo ID", Description = "Lấy thông tin sinh viên theo ID")]
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
    /// [ADMIN/TEACHER] Lấy sinh viên theo mã sinh viên
    /// </summary>
    /// <param name="studentCode">Mã sinh viên</param>
    /// <returns>Thông tin sinh viên</returns>
    /// <response code="200">Trả về thông tin sinh viên</response>
    /// <response code="404">Không tìm thấy sinh viên</response>
    [HttpGet("by-student-code/{studentCode}")]
    [Authorize(Roles = "Admin,Teacher")]
    [SwaggerOperation(Summary = "[ADMIN/TEACHER] Lấy sinh viên theo mã sinh viên", Description = "Lấy thông tin sinh viên theo mã sinh viên (StudentCode)")]
    [SwaggerResponse(200, "Thông tin sinh viên", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "Không tìm thấy sinh viên", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetStudentByStudentCode(string studentCode)
    {
        var student = await _studentService.GetStudentByStudentCodeAsync(studentCode);
        if (student == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Student not found"));

        return Ok(ApiResponse<object>.SuccessResponse(student, "Student retrieved successfully"));
    }

    /// <summary>
    /// [ADMIN/TEACHER] Lấy danh sách sinh viên
    /// </summary>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Số lượng mỗi trang</param>
    /// <param name="classId">ID lớp học (lọc)</param>
    /// <param name="search">Tìm kiếm theo tên, email, MSSV</param>
    /// <param name="year">Lọc theo năm học</param>
    /// <param name="major">Lọc theo khoa</param>
    /// <param name="status">Lọc theo trạng thái</param>
    /// <param name="excludeClassId">Loại trừ sinh viên đã có trong lớp này</param>
    /// <returns>Danh sách sinh viên</returns>
    /// <response code="200">Trả về danh sách sinh viên</response>
    [HttpGet]
    [Authorize(Roles = "Admin,Teacher")]
    [SwaggerOperation(Summary = "[ADMIN/TEACHER] Lấy danh sách sinh viên", Description = "Lấy danh sách sinh viên có phân trang và filter")]
    [SwaggerResponse(200, "Danh sách sinh viên", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetStudents(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? classId = null,
        [FromQuery] string? search = null,
        [FromQuery] int? year = null,
        [FromQuery] string? major = null,
        [FromQuery] string? status = null,
        [FromQuery] string? excludeClassId = null)
    {
        var students = await _studentService.GetStudentsAsync(
            pageNumber, pageSize, classId, search, year, major, status, excludeClassId);
        return Ok(ApiResponse<object>.SuccessResponse(students, "Students retrieved successfully"));
    }

    /// <summary>
    /// [ADMIN/TEACHER] Cập nhật thông tin sinh viên
    /// </summary>
    /// <param name="id">ID sinh viên</param>
    /// <param name="request">Thông tin cập nhật</param>
    /// <returns>Trạng thái cập nhật</returns>
    /// <response code="200">Cập nhật thành công</response>
    /// <response code="400">Cập nhật thất bại</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    [SwaggerOperation(Summary = "[ADMIN/TEACHER] Cập nhật sinh viên", Description = "Cập nhật thông tin sinh viên")]
    [SwaggerResponse(200, "Cập nhật thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Cập nhật thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> UpdateStudent(string id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

        var result = await _studentService.UpdateStudentAsync(id, request);
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to update student"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Student updated successfully"));
    }

    /// <summary>
    /// [ADMIN/TEACHER] Xóa sinh viên
    /// </summary>
    /// <param name="id">ID sinh viên</param>
    /// <returns>Trạng thái xóa</returns>
    /// <response code="200">Xóa thành công</response>
    /// <response code="400">Xóa thất bại</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    [SwaggerOperation(Summary = "[ADMIN/TEACHER] Xóa sinh viên", Description = "Xóa một sinh viên")]
    [SwaggerResponse(200, "Xóa thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Xóa thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> DeleteStudent(string id)
    {
        var result = await _studentService.DeleteStudentAsync(id);
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to delete student"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Student deleted successfully"));
    }

    /// <summary>
    /// [ADMIN/TEACHER] Import sinh viên từ file Excel
    /// </summary>
    /// <param name="file">File Excel chứa danh sách sinh viên</param>
    /// <returns>Kết quả import</returns>
    /// <response code="200">Import thành công</response>
    /// <response code="400">Import thất bại</response>
    [HttpPost("import-excel")]
    [Authorize(Roles = "Admin,Teacher")]
    [SwaggerOperation(Summary = "[ADMIN/TEACHER] Import sinh viên từ Excel", Description = "Import danh sách sinh viên từ file Excel")]
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
                    errors.Add($"Failed to create student {request.StudentCode}: {ex.Message}");
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
    /// [ADMIN/TEACHER] Export sinh viên ra file Excel
    /// </summary>
    /// <param name="studentIds">Danh sách ID sinh viên cần export</param>
    /// <returns>File Excel</returns>
    /// <response code="200">Export thành công</response>
    /// <response code="400">Export thất bại</response>
    [HttpGet("export-excel")]
    [Authorize(Roles = "Admin,Teacher")]
    [SwaggerOperation(Summary = "[ADMIN/TEACHER] Export sinh viên ra Excel", Description = "Export danh sách sinh viên ra file Excel")]
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

    /// <summary>
    /// [TEACHER] Download Excel template để import sinh viên
    /// </summary>
    /// <param name="classId">ID lớp học (để loại trừ sinh viên đã enroll)</param>
    /// <returns>File Excel template có thể bao gồm danh sách sinh viên available</returns>
    /// <response code="200">Download thành công</response>
    /// <response code="400">Download thất bại</response>
    [HttpGet("template")]
    [Authorize(Roles = "Teacher")]
    [SwaggerOperation(Summary = "[TEACHER] Download Excel template", Description = "Download file Excel template để import sinh viên, có thể bao gồm danh sách sinh viên available")]
    [SwaggerResponse(200, "Download thành công")]
    [SwaggerResponse(400, "Download thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> DownloadImportTemplate([FromQuery] string? classId = null)
    {
        try
        {
            var fileBytes = await _excelService.GenerateImportTemplateAsync(classId);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                $"Student_Import_Template_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Failed to generate template: {ex.Message}"));
        }
    }

    /// <summary>
    /// [TEACHER] Validate danh sách sinh viên trước khi import (batch validation)
    /// </summary>
    /// <param name="request">Danh sách MSSV hoặc Email cần validate</param>
    /// <returns>Kết quả validation từng student</returns>
    /// <response code="200">Validation thành công</response>
    /// <response code="400">Validation thất bại</response>
    [HttpPost("validate-batch")]
    [Authorize(Roles = "Teacher")]
    [SwaggerOperation(Summary = "[TEACHER] Validate batch students", Description = "Validate danh sách sinh viên trước khi import để hiện preview")]
    [SwaggerResponse(200, "Validation thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Validation thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> ValidateBatch([FromBody] ValidateBatchRequest request)
    {
        try
        {
            var validationResults = await _studentService.ValidateBatchAsync(request.Identifiers, request.ClassId);
            
            return Ok(ApiResponse<object>.SuccessResponse(
                new {
                    TotalCount = validationResults.Count,
                    ValidCount = validationResults.Count(r => r.IsValid),
                    InvalidCount = validationResults.Count(r => !r.IsValid),
                    Results = validationResults
                },
                "Batch validation completed"
            ));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Failed to validate batch: {ex.Message}"));
        }
    }

    #endregion
}
