using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using UserService.Application.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace UserService.Api.Controllers;

/// <summary>
/// Controller quản lý lớp học
/// </summary>
[ApiController]
[Route("api/v1/classes")]
public class ClassController : ControllerBase
{
    private readonly IClassService _classService;

    public ClassController(IClassService classService)
    {
        _classService = classService;
    }

    /// <summary>
    /// Tạo lớp học mới
    /// </summary>
    /// <param name="request">Thông tin lớp học</param>
    /// <returns>Thông tin lớp học đã tạo</returns>
    /// <response code="200">Lớp học được tạo thành công</response>
    /// <response code="400">Dữ liệu không hợp lệ</response>
    [HttpPost("create")]
    [SwaggerOperation(Summary = "Tạo lớp học", Description = "Tạo một lớp học mới")]
    [SwaggerResponse(200, "Lớp học được tạo thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Dữ liệu không hợp lệ", typeof(ApiResponse<object>))]
    public async Task<IActionResult> CreateClass([FromBody] CreateClassRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

        var classEntity = await _classService.CreateClassAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(classEntity, "Class created successfully"));
    }

    /// <summary>
    /// Lấy thông tin lớp học theo ID
    /// </summary>
    /// <param name="id">ID của lớp học</param>
    /// <returns>Thông tin lớp học</returns>
    /// <response code="200">Trả về thông tin lớp học</response>
    /// <response code="404">Không tìm thấy lớp học</response>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Lấy lớp học theo ID", Description = "Lấy thông tin lớp học theo ID")]
    [SwaggerResponse(200, "Thông tin lớp học", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "Không tìm thấy lớp học", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetClass(string id)
    {
        var classEntity = await _classService.GetClassByIdAsync(id);
        if (classEntity == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Class not found"));

        return Ok(ApiResponse<object>.SuccessResponse(classEntity, "Class retrieved successfully"));
    }

    /// <summary>
    /// Lấy thông tin chi tiết lớp học theo ID
    /// </summary>
    /// <param name="id">ID của lớp học</param>
    /// <returns>Thông tin chi tiết lớp học bao gồm danh sách sinh viên</returns>
    /// <response code="200">Thông tin chi tiết lớp học</response>
    /// <response code="404">Không tìm thấy lớp học</response>
    [HttpGet("{id}/detail")]
    [SwaggerOperation(Summary = "Lấy thông tin chi tiết lớp học", Description = "Lấy thông tin chi tiết lớp học bao gồm danh sách sinh viên")]
    [SwaggerResponse(200, "Thông tin chi tiết lớp học", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "Không tìm thấy lớp học", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetClassDetail(string id)
    {
        var classDetail = await _classService.GetClassDetailAsync(id);
        if (classDetail == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Class not found"));

        return Ok(ApiResponse<object>.SuccessResponse(classDetail, "Class detail retrieved successfully"));
    }

    /// <summary>
    /// Lấy danh sách lớp học
    /// </summary>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Số lượng mỗi trang</param>
    /// <param name="teacherId">ID giáo viên (lọc)</param>
    /// <param name="isActive">Trạng thái hoạt động (lọc)</param>
    /// <returns>Danh sách lớp học</returns>
    /// <response code="200">Trả về danh sách lớp học</response>
    [HttpGet]
    [SwaggerOperation(Summary = "Lấy danh sách lớp học", Description = "Lấy danh sách lớp học có phân trang và lọc theo giáo viên, trạng thái")]
    [SwaggerResponse(200, "Danh sách lớp học", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetClasses(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? teacherId = null,
        [FromQuery] bool? isActive = null)
    {
        var classes = await _classService.GetClassesAsync(pageNumber, pageSize, teacherId, isActive);
        return Ok(ApiResponse<object>.SuccessResponse(classes, "Classes retrieved successfully"));
    }

    /// <summary>
    /// Cập nhật thông tin lớp học
    /// </summary>
    /// <param name="request">Thông tin cập nhật</param>
    /// <returns>Trạng thái cập nhật</returns>
    /// <response code="200">Cập nhật thành công</response>
    /// <response code="400">Cập nhật thất bại</response>
    [HttpPut("update")]
    [SwaggerOperation(Summary = "Cập nhật lớp học", Description = "Cập nhật thông tin lớp học")]
    [SwaggerResponse(200, "Cập nhật thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Cập nhật thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> UpdateClass([FromBody] UpdateClassRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

        var result = await _classService.UpdateClassAsync(request.ClassId.ToString(), request);
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to update class"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Class updated successfully"));
    }

    /// <summary>
    /// Xóa lớp học
    /// </summary>
    /// <param name="id">ID lớp học</param>
    /// <returns>Trạng thái xóa</returns>
    /// <response code="200">Xóa thành công</response>
    /// <response code="400">Xóa thất bại</response>
    [HttpDelete("delete")]
    [SwaggerOperation(Summary = "Xóa lớp học", Description = "Xóa một lớp học")]
    [SwaggerResponse(200, "Xóa thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Xóa thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> DeleteClass([FromQuery] string id)
    {
        var result = await _classService.DeleteClassAsync(id);
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to delete class"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Class deleted successfully"));
    }

    /// <summary>
    /// [STUDENT] Lấy danh sách lớp học đã đăng ký
    /// </summary>
    /// <returns>Danh sách lớp học đã đăng ký</returns>
    /// <response code="200">Danh sách lớp học</response>
    [HttpGet("enrolled")]
    [Authorize(Roles = "Student")]
    [SwaggerOperation(Summary = "[STUDENT] Lấy lớp học đã đăng ký", Description = "Student lấy danh sách các lớp đã enroll từ JWT token")]
    [SwaggerResponse(200, "Danh sách lớp học đã đăng ký", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetEnrolledClasses()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");

        var classes = await _classService.GetClassesByStudentIdAsync(userId);
        return Ok(ApiResponse<object>.SuccessResponse(classes, "Enrolled classes retrieved successfully"));
    }

    /// <summary>
    /// Thêm sinh viên vào lớp học
    /// </summary>
    /// <param name="request">ID lớp học và sinh viên</param>
    /// <returns>Trạng thái thêm</returns>
    /// <response code="200">Thêm thành công</response>
    /// <response code="400">Thêm thất bại</response>
    [HttpPost("add-student")]
    [SwaggerOperation(Summary = "Thêm sinh viên vào lớp", Description = "Thêm một sinh viên vào lớp học")]
    [SwaggerResponse(200, "Thêm thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Thêm thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> AddStudentToClass([FromBody] AddStudentToClassRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

        var result = await _classService.AddStudentToClassAsync(request.ClassId, request.StudentId);
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to add student to class"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Student added to class successfully"));
    }

    /// <summary>
    /// Thêm nhiều sinh viên vào lớp học
    /// </summary>
    /// <param name="request">ID lớp học và danh sách sinh viên</param>
    /// <returns>Trạng thái thêm</returns>
    /// <response code="200">Thêm thành công</response>
    /// <response code="400">Thêm thất bại</response>
    [HttpPost("add-students")]
    [SwaggerOperation(Summary = "Thêm nhiều sinh viên vào lớp", Description = "Thêm nhiều sinh viên vào lớp học")]
    [SwaggerResponse(200, "Thêm thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Thêm thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> AddStudentsToClass([FromBody] AddStudentsToClassRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

        var result = await _classService.AddStudentsToClassAsync(request);
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to add students to class"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Students added to class successfully"));
    }

    /// <summary>
    /// Xóa sinh viên khỏi lớp học
    /// </summary>
    /// <param name="classId">ID lớp học</param>
    /// <param name="studentId">ID sinh viên</param>
    /// <returns>Trạng thái xóa</returns>
    /// <response code="200">Xóa thành công</response>
    /// <response code="400">Xóa thất bại</response>
    [HttpDelete("remove-student")]
    [SwaggerOperation(Summary = "Xóa sinh viên khỏi lớp", Description = "Xóa sinh viên khỏi lớp học")]
    [SwaggerResponse(200, "Xóa thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Xóa thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> RemoveStudentFromClass([FromQuery] string classId, [FromQuery] string studentId)
    {
        var result = await _classService.RemoveStudentFromClassAsync(classId, studentId);
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to remove student from class"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Student removed from class successfully"));
    }

    /// <summary>
    /// Lấy danh sách sinh viên trong lớp học
    /// </summary>
    /// <param name="classId">ID lớp học</param>
    /// <returns>Danh sách sinh viên</returns>
    /// <response code="200">Trả về danh sách sinh viên</response>
    [HttpGet("{classId}/students")]
    [SwaggerOperation(Summary = "Lấy danh sách sinh viên trong lớp", Description = "Lấy danh sách sinh viên trong một lớp học")]
    [SwaggerResponse(200, "Danh sách sinh viên", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetStudentListByClass(string classId)
    {
        var students = await _classService.GetStudentListByClassAsync(classId);
        return Ok(ApiResponse<object>.SuccessResponse(students, "Student list retrieved successfully"));
    }
}

// Helper DTOs
public class AddStudentToClassRequest
{
    public string ClassId { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
}

