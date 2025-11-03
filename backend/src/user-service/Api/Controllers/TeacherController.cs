using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using UserService.Application.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace UserService.Api.Controllers;

/// <summary>
/// Controller quản lý giáo viên
/// </summary>
[ApiController]
[Route("api/v1/teachers")]
public class TeacherController : ControllerBase
{
    private readonly ITeacherService _teacherService;
    private readonly IAdminService _adminService;

    public TeacherController(ITeacherService teacherService, IAdminService adminService)
    {
        _teacherService = teacherService;
        _adminService = adminService;
    }

    /// <summary>
    /// Tạo giáo viên mới
    /// </summary>
    /// <param name="request">Thông tin giáo viên</param>
    /// <returns>Thông tin giáo viên đã tạo</returns>
    /// <response code="200">Giáo viên được tạo thành công</response>
    /// <response code="400">Dữ liệu không hợp lệ</response>
    [HttpPost("create")]
    [SwaggerOperation(Summary = "Tạo giáo viên", Description = "Tạo một giáo viên mới")]
    [SwaggerResponse(200, "Giáo viên được tạo thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Dữ liệu không hợp lệ", typeof(ApiResponse<object>))]
    public async Task<IActionResult> CreateTeacher([FromBody] CreateTeacherRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

        var teacher = await _teacherService.CreateTeacherAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(teacher, "Teacher created successfully"));
    }

    /// <summary>
    /// Tạo admin mới
    /// </summary>
    /// <param name="request">Thông tin admin</param>
    /// <returns>Thông tin admin đã tạo</returns>
    /// <response code="200">Admin được tạo thành công</response>
    /// <response code="400">Dữ liệu không hợp lệ</response>
    [HttpPost("create-admin")]
    [SwaggerOperation(Summary = "Tạo admin", Description = "Tạo một admin mới")]
    [SwaggerResponse(200, "Admin được tạo thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Dữ liệu không hợp lệ", typeof(ApiResponse<object>))]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

        var admin = await _adminService.CreateAdminAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(admin, "Admin created successfully"));
    }

    /// <summary>
    /// Lấy thông tin giáo viên theo ID
    /// </summary>
    /// <param name="id">ID giáo viên</param>
    /// <returns>Thông tin giáo viên</returns>
    /// <response code="200">Trả về thông tin giáo viên</response>
    /// <response code="404">Không tìm thấy giáo viên</response>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Lấy giáo viên theo ID", Description = "Lấy thông tin giáo viên theo ID")]
    [SwaggerResponse(200, "Thông tin giáo viên", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "Không tìm thấy giáo viên", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetTeacher(string id)
    {
        var teacher = await _teacherService.GetTeacherByIdAsync(id);
        if (teacher == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Teacher not found"));

        return Ok(ApiResponse<object>.SuccessResponse(teacher, "Teacher retrieved successfully"));
    }

    /// <summary>
    /// Lấy thông tin giáo viên theo mã nhân viên
    /// </summary>
    /// <param name="employeeId">Mã nhân viên</param>
    /// <returns>Thông tin giáo viên</returns>
    /// <response code="200">Trả về thông tin giáo viên</response>
    /// <response code="404">Không tìm thấy giáo viên</response>
    [HttpGet("by-employee-id/{employeeId}")]
    [SwaggerOperation(Summary = "Lấy giáo viên theo mã nhân viên", Description = "Lấy thông tin giáo viên theo mã nhân viên")]
    [SwaggerResponse(200, "Thông tin giáo viên", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "Không tìm thấy giáo viên", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetTeacherByEmployeeId(string employeeId)
    {
        var teacher = await _teacherService.GetTeacherByEmployeeIdAsync(employeeId);
        if (teacher == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Teacher not found"));

        return Ok(ApiResponse<object>.SuccessResponse(teacher, "Teacher retrieved successfully"));
    }

    /// <summary>
    /// Lấy danh sách giáo viên
    /// </summary>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Số lượng mỗi trang</param>
    /// <param name="department">Khoa (lọc)</param>
    /// <returns>Danh sách giáo viên</returns>
    /// <response code="200">Trả về danh sách giáo viên</response>
    [HttpGet]
    [SwaggerOperation(Summary = "Lấy danh sách giáo viên", Description = "Lấy danh sách giáo viên có phân trang và lọc theo khoa")]
    [SwaggerResponse(200, "Danh sách giáo viên", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetTeachers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? department = null)
    {
        var teachers = await _teacherService.GetTeachersAsync(pageNumber, pageSize, department);
        return Ok(ApiResponse<object>.SuccessResponse(teachers, "Teachers retrieved successfully"));
    }

    /// <summary>
    /// Lấy danh sách lớp học của giáo viên
    /// </summary>
    /// <param name="teacherId">ID giáo viên</param>
    /// <returns>Danh sách lớp học</returns>
    /// <response code="200">Trả về danh sách lớp học</response>
    [HttpGet("{teacherId}/classes")]
    [SwaggerOperation(Summary = "Lấy danh sách lớp học của giáo viên", Description = "Lấy danh sách lớp học do giáo viên giảng dạy")]
    [SwaggerResponse(200, "Danh sách lớp học", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetTeacherClasses(string teacherId)
    {
        var classes = await _teacherService.GetTeacherClassesAsync(teacherId);
        return Ok(ApiResponse<object>.SuccessResponse(classes, "Teacher classes retrieved successfully"));
    }

    /// <summary>
    /// Cập nhật thông tin giáo viên
    /// </summary>
    /// <param name="request">Thông tin cập nhật</param>
    /// <returns>Trạng thái cập nhật</returns>
    /// <response code="200">Cập nhật thành công</response>
    /// <response code="400">Cập nhật thất bại</response>
    [HttpPut("update")]
    [SwaggerOperation(Summary = "Cập nhật giáo viên", Description = "Cập nhật thông tin giáo viên")]
    [SwaggerResponse(200, "Cập nhật thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Cập nhật thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> UpdateTeacher([FromBody] UpdateTeacherRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

        var result = await _teacherService.UpdateTeacherAsync(request.UserId.ToString(), request);
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to update teacher"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Teacher updated successfully"));
    }

    /// <summary>
    /// Xóa giáo viên
    /// </summary>
    /// <param name="id">ID giáo viên</param>
    /// <returns>Trạng thái xóa</returns>
    /// <response code="200">Xóa thành công</response>
    /// <response code="400">Xóa thất bại</response>
    [HttpDelete("delete")]
    [SwaggerOperation(Summary = "Xóa giáo viên", Description = "Xóa một giáo viên")]
    [SwaggerResponse(200, "Xóa thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Xóa thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> DeleteTeacher([FromQuery] string id)
    {
        var result = await _teacherService.DeleteTeacherAsync(id);
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to delete teacher"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Teacher deleted successfully"));
    }
}

// Helper DTO for UpdateTeacher
public class UpdateTeacherRequest : UpdateUserRequest
{
    // Inherits all properties from UpdateUserRequest
}

