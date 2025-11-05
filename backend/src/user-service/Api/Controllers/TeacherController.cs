using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using UserService.Application.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace UserService.Api.Controllers;

/// <summary>
/// Controller quản lý giáo viên
/// </summary>
[ApiController]
[Route("api/v1/teachers")]
[Authorize]
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
    /// Helper method để lấy UserId từ JWT token
    /// </summary>
    private string GetUserIdFromToken()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");
    }

    #region Teacher Self-Service APIs
    /// <summary>
    /// [TEACHER] Lấy thông tin cá nhân
    /// </summary>
    /// <returns>Thông tin giáo viên</returns>
    /// <response code="200">Trả về thông tin giáo viên</response>
    /// <response code="404">Không tìm thấy giáo viên</response>
    [HttpGet("me")]
    [Authorize(Roles = "Teacher")]
    [SwaggerOperation(Summary = "[TEACHER] Lấy thông tin cá nhân", Description = "Teacher lấy thông tin của chính mình từ JWT token")]
    [SwaggerResponse(200, "Thông tin giáo viên", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "Không tìm thấy giáo viên", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetUserIdFromToken();
        var teacher = await _teacherService.GetTeacherByIdAsync(userId);
        
        if (teacher == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Teacher not found"));

        return Ok(ApiResponse<object>.SuccessResponse(teacher, "Teacher retrieved successfully"));
    }

    /// <summary>
    /// [TEACHER] Cập nhật thông tin cá nhân
    /// </summary>
    /// <param name="request">Thông tin cập nhật (không cần userId, lấy từ token)</param>
    /// <returns>Trạng thái cập nhật</returns>
    /// <response code="200">Cập nhật thành công</response>
    /// <response code="400">Cập nhật thất bại</response>
    [HttpPut("me")]
    [Authorize(Roles = "Teacher")]
    [SwaggerOperation(Summary = "[TEACHER] Cập nhật thông tin cá nhân", Description = "Teacher cập nhật thông tin của chính mình (userId lấy từ JWT token)")]
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
        
        var result = await _teacherService.UpdateTeacherAsync(userId, updateRequest);
        
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to update teacher"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Teacher updated successfully"));
    }

    /// <summary>
    /// [TEACHER] Lấy danh sách lớp học của mình
    /// </summary>
    /// <returns>Danh sách lớp học</returns>
    /// <response code="200">Trả về danh sách lớp học</response>
    [HttpGet("me/classes")]
    [Authorize(Roles = "Teacher")]
    [SwaggerOperation(Summary = "[TEACHER] Lấy danh sách lớp học của mình", Description = "Teacher lấy danh sách lớp học mình giảng dạy")]
    [SwaggerResponse(200, "Danh sách lớp học", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetMyClasses()
    {
        var userId = GetUserIdFromToken();
        var classes = await _teacherService.GetTeacherClassesAsync(userId);
        return Ok(ApiResponse<object>.SuccessResponse(classes, "Teacher classes retrieved successfully"));
    }

    #endregion

    #region Admin & Teacher - Full Management APIs
    /// <summary>
    /// [ADMIN/TEACHER] Tạo giáo viên mới
    /// </summary>
    /// <param name="request">Thông tin giáo viên</param>
    /// <returns>Thông tin giáo viên đã tạo</returns>
    /// <response code="200">Giáo viên được tạo thành công</response>
    /// <response code="400">Dữ liệu không hợp lệ</response>
    [HttpPost("create")]
    [Authorize(Roles = "Admin,Teacher")]
    [SwaggerOperation(Summary = "[ADMIN/TEACHER] Tạo giáo viên", Description = "Admin hoặc Teacher tạo giáo viên mới")]
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
    /// [ADMIN ONLY] Tạo admin mới
    /// </summary>
    /// <param name="request">Thông tin admin</param>
    /// <returns>Thông tin admin đã tạo</returns>
    /// <response code="200">Admin được tạo thành công</response>
    /// <response code="400">Dữ liệu không hợp lệ</response>
    [HttpPost("create-admin")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "[ADMIN] Tạo admin", Description = "Chỉ Admin mới có thể tạo admin mới")]
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
    /// [ADMIN/TEACHER] Lấy giáo viên theo ID
    /// </summary>
    /// <param name="id">ID giáo viên</param>
    /// <returns>Thông tin giáo viên</returns>
    /// <response code="200">Trả về thông tin giáo viên</response>
    /// <response code="404">Không tìm thấy giáo viên</response>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    [SwaggerOperation(Summary = "[ADMIN/TEACHER] Lấy giáo viên theo ID", Description = "Lấy thông tin giáo viên theo ID")]
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
    /// [ADMIN/TEACHER] Lấy giáo viên theo mã giảng viên
    /// </summary>
    /// <param name="teacherCode">Mã giảng viên</param>
    /// <returns>Thông tin giáo viên</returns>
    /// <response code="200">Trả về thông tin giáo viên</response>
    /// <response code="404">Không tìm thấy giáo viên</response>
    [HttpGet("by-teacher-code/{teacherCode}")]
    [Authorize(Roles = "Admin,Teacher")]
    [SwaggerOperation(Summary = "[ADMIN/TEACHER] Lấy giáo viên theo mã giảng viên", Description = "Lấy thông tin giáo viên theo mã giảng viên (TeacherCode)")]
    [SwaggerResponse(200, "Thông tin giáo viên", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "Không tìm thấy giáo viên", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetTeacherByTeacherCode(string teacherCode)
    {
        var teacher = await _teacherService.GetTeacherByTeacherCodeAsync(teacherCode);
        if (teacher == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Teacher not found"));

        return Ok(ApiResponse<object>.SuccessResponse(teacher, "Teacher retrieved successfully"));
    }

    /// <summary>
    /// [ADMIN/TEACHER] Lấy danh sách giáo viên
    /// </summary>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Số lượng mỗi trang</param>
    /// <param name="department">Khoa (lọc)</param>
    /// <returns>Danh sách giáo viên</returns>
    /// <response code="200">Trả về danh sách giáo viên</response>
    [HttpGet]
    [Authorize(Roles = "Admin,Teacher")]
    [SwaggerOperation(Summary = "[ADMIN/TEACHER] Lấy danh sách giáo viên", Description = "Lấy danh sách giáo viên có phân trang")]
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
    /// [ADMIN/TEACHER] Lấy danh sách lớp học của giáo viên
    /// </summary>
    /// <param name="teacherId">ID giáo viên</param>
    /// <returns>Danh sách lớp học</returns>
    /// <response code="200">Trả về danh sách lớp học</response>
    [HttpGet("{teacherId}/classes")]
    [Authorize(Roles = "Admin,Teacher")]
    [SwaggerOperation(Summary = "[ADMIN/TEACHER] Lấy danh sách lớp học của giáo viên", Description = "Lấy danh sách lớp học do giáo viên giảng dạy")]
    [SwaggerResponse(200, "Danh sách lớp học", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetTeacherClasses(string teacherId)
    {
        var classes = await _teacherService.GetTeacherClassesAsync(teacherId);
        return Ok(ApiResponse<object>.SuccessResponse(classes, "Teacher classes retrieved successfully"));
    }

    /// <summary>
    /// [ADMIN/TEACHER] Cập nhật thông tin giáo viên
    /// </summary>
    /// <param name="id">ID giáo viên</param>
    /// <param name="request">Thông tin cập nhật</param>
    /// <returns>Trạng thái cập nhật</returns>
    /// <response code="200">Cập nhật thành công</response>
    /// <response code="400">Cập nhật thất bại</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    [SwaggerOperation(Summary = "[ADMIN/TEACHER] Cập nhật giáo viên", Description = "Cập nhật thông tin giáo viên")]
    [SwaggerResponse(200, "Cập nhật thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Cập nhật thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> UpdateTeacher(string id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

        var result = await _teacherService.UpdateTeacherAsync(id, request);
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to update teacher"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Teacher updated successfully"));
    }

    /// <summary>
    /// [ADMIN/TEACHER] Xóa giáo viên
    /// </summary>
    /// <param name="id">ID giáo viên</param>
    /// <returns>Trạng thái xóa</returns>
    /// <response code="200">Xóa thành công</response>
    /// <response code="400">Xóa thất bại</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    [SwaggerOperation(Summary = "[ADMIN/TEACHER] Xóa giáo viên", Description = "Xóa một giáo viên")]
    [SwaggerResponse(200, "Xóa thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Xóa thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> DeleteTeacher(string id)
    {
        var result = await _teacherService.DeleteTeacherAsync(id);
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to delete teacher"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Teacher deleted successfully"));
    }

    #endregion
}
