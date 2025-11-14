using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using UserService.Application.DTOs.Admin;
using UserService.Application.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace UserService.Api.Controllers;

/// <summary>
/// Controller quản lý admin - Quản lý lớp học và người dùng
/// </summary>
[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IClassService _classService;
    private readonly IUserService _userService;

    public AdminController(IClassService classService, IUserService userService)
    {
        _classService = classService;
        _userService = userService;
    }

    /// <summary>
    /// [ADMIN] Lấy tất cả lớp học trong hệ thống với thông tin chi tiết
    /// </summary>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Số lượng mỗi trang</param>
    /// <param name="teacherId">ID giáo viên (lọc)</param>
    /// <param name="isActive">Trạng thái hoạt động (lọc)</param>
    /// <param name="isArchived">Trạng thái archive (lọc)</param>
    /// <param name="searchTerm">Từ khóa tìm kiếm</param>
    /// <returns>Danh sách tất cả lớp học với thông tin chi tiết</returns>
    /// <response code="200">Trả về danh sách lớp học</response>
    [HttpGet("classes")]
    [SwaggerOperation(
        Summary = "[ADMIN] Lấy tất cả lớp học", 
        Description = "Lấy tất cả lớp học trong hệ thống với thông tin chi tiết, hỗ trợ phân trang và lọc")]
    [SwaggerResponse(200, "Danh sách lớp học", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetAllClasses(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? teacherId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool? isArchived = null,
        [FromQuery] string? searchTerm = null)
    {
        var classes = await _classService.GetAllClassesForAdminAsync(
            pageNumber, 
            pageSize, 
            teacherId, 
            isActive, 
            isArchived, 
            searchTerm);
        
        return Ok(ApiResponse<object>.SuccessResponse(classes, "Classes retrieved successfully"));
    }

    /// <summary>
    /// [ADMIN] Lấy thông tin chi tiết lớp học
    /// </summary>
    /// <param name="id">ID của lớp học</param>
    /// <returns>Thông tin chi tiết lớp học</returns>
    /// <response code="200">Thông tin lớp học</response>
    /// <response code="404">Không tìm thấy lớp học</response>
    [HttpGet("classes/{id}")]
    [SwaggerOperation(
        Summary = "[ADMIN] Lấy chi tiết lớp học", 
        Description = "Lấy thông tin chi tiết lớp học theo ID")]
    [SwaggerResponse(200, "Thông tin lớp học", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "Không tìm thấy lớp học", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetClassDetail(string id)
    {
        var classDetail = await _classService.GetClassDetailForAdminAsync(id);
        if (classDetail == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Class not found"));

        return Ok(ApiResponse<object>.SuccessResponse(classDetail, "Class detail retrieved successfully"));
    }

    /// <summary>
    /// [ADMIN] Archive lớp học
    /// </summary>
    /// <param name="id">ID lớp học</param>
    /// <param name="request">Lý do archive (optional)</param>
    /// <returns>Trạng thái archive</returns>
    /// <response code="200">Archive thành công</response>
    /// <response code="400">Archive thất bại</response>
    [HttpPatch("classes/{id}/archive")]
    [SwaggerOperation(
        Summary = "[ADMIN] Archive lớp học", 
        Description = "Archive một lớp học (không xóa, chỉ đánh dấu là archived)")]
    [SwaggerResponse(200, "Archive thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Archive thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> ArchiveClass(string id, [FromBody] ArchiveClassRequest? request = null)
    {
        try
        {
            var result = await _classService.ArchiveClassAsync(id, request?.Reason);
            if (!result)
                return BadRequest(ApiResponse<object>.ErrorResponse("Failed to archive class"));

            return Ok(ApiResponse<object>.SuccessResponse(null, "Class archived successfully"));
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// [ADMIN] Unarchive lớp học
    /// </summary>
    /// <param name="id">ID lớp học</param>
    /// <returns>Trạng thái unarchive</returns>
    /// <response code="200">Unarchive thành công</response>
    /// <response code="400">Unarchive thất bại</response>
    [HttpPatch("classes/{id}/unarchive")]
    [SwaggerOperation(
        Summary = "[ADMIN] Unarchive lớp học", 
        Description = "Khôi phục lớp học đã bị archive")]
    [SwaggerResponse(200, "Unarchive thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Unarchive thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> UnarchiveClass(string id)
    {
        try
        {
            var result = await _classService.UnarchiveClassAsync(id);
            if (!result)
                return BadRequest(ApiResponse<object>.ErrorResponse("Failed to unarchive class"));

            return Ok(ApiResponse<object>.SuccessResponse(null, "Class unarchived successfully"));
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// [ADMIN] Cập nhật thông tin lớp học
    /// </summary>
    /// <param name="id">ID lớp học</param>
    /// <param name="request">Thông tin cập nhật</param>
    /// <returns>Trạng thái cập nhật</returns>
    /// <response code="200">Cập nhật thành công</response>
    /// <response code="400">Cập nhật thất bại</response>
    [HttpPut("classes/{id}")]
    [SwaggerOperation(
        Summary = "[ADMIN] Cập nhật lớp học", 
        Description = "Admin cập nhật thông tin lớp học (bao gồm cả teacher)")]
    [SwaggerResponse(200, "Cập nhật thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Cập nhật thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> UpdateClass(string id, [FromBody] UpdateClassByAdminRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

        if (id != request.ClassId.ToString())
            return BadRequest(ApiResponse<object>.ErrorResponse("Class ID mismatch"));

        try
        {
            var result = await _classService.UpdateClassByAdminAsync(request);
            if (!result)
                return BadRequest(ApiResponse<object>.ErrorResponse("Failed to update class"));

            return Ok(ApiResponse<object>.SuccessResponse(null, "Class updated successfully"));
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// [ADMIN] Xóa vĩnh viễn lớp học
    /// </summary>
    /// <param name="id">ID lớp học</param>
    /// <returns>Trạng thái xóa</returns>
    /// <response code="200">Xóa thành công</response>
    /// <response code="400">Xóa thất bại</response>
    [HttpDelete("classes/{id}")]
    [SwaggerOperation(
        Summary = "[ADMIN] Xóa lớp học", 
        Description = "Xóa vĩnh viễn lớp học (sẽ xóa cả student enrollments)")]
    [SwaggerResponse(200, "Xóa thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Xóa thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> DeleteClass(string id)
    {
        try
        {
            var result = await _classService.DeleteClassByAdminAsync(id);
            if (!result)
                return BadRequest(ApiResponse<object>.ErrorResponse("Failed to delete class"));

            return Ok(ApiResponse<object>.SuccessResponse(null, "Class deleted successfully"));
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// [ADMIN] Lấy thống kê tổng quan về classes
    /// </summary>
    /// <returns>Thống kê về classes trong hệ thống</returns>
    /// <response code="200">Thống kê classes</response>
    [HttpGet("classes/statistics")]
    [SwaggerOperation(
        Summary = "[ADMIN] Lấy thống kê classes", 
        Description = "Lấy thống kê tổng quan về tất cả classes trong hệ thống")]
    [SwaggerResponse(200, "Thống kê classes", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetClassStatistics()
    {
        var statistics = await _classService.GetClassStatisticsAsync();
        return Ok(ApiResponse<object>.SuccessResponse(statistics, "Statistics retrieved successfully"));
    }

    /// <summary>
    /// [ADMIN] Bulk actions trên nhiều classes
    /// </summary>
    /// <param name="request">Bulk action request</param>
    /// <returns>Kết quả thực hiện</returns>
    /// <response code="200">Thực hiện thành công</response>
    [HttpPost("classes/bulk-action")]
    [SwaggerOperation(
        Summary = "[ADMIN] Bulk actions", 
        Description = "Thực hiện hành động trên nhiều classes cùng lúc (archive, unarchive, delete)")]
    [SwaggerResponse(200, "Bulk action thành công", typeof(ApiResponse<object>))]
    public async Task<IActionResult> BulkAction([FromBody] BulkActionRequest request)
    {
        var result = await _classService.BulkActionAsync(request.Action, request.ClassIds, request.Reason);
        return Ok(ApiResponse<object>.SuccessResponse(result, $"Bulk {request.Action} completed successfully"));
    }

    /// <summary>
    /// [ADMIN] Lấy danh sách students trong một class
    /// </summary>
    /// <param name="classId">ID của class</param>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Số lượng mỗi trang</param>
    /// <param name="searchTerm">Từ khóa tìm kiếm</param>
    /// <returns>Danh sách students</returns>
    /// <response code="200">Danh sách students</response>
    [HttpGet("classes/{classId}/students")]
    [SwaggerOperation(
        Summary = "[ADMIN] Lấy students trong class", 
        Description = "Lấy danh sách students trong một class với phân trang")]
    [SwaggerResponse(200, "Danh sách students", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetClassStudents(
        string classId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null)
    {
        var students = await _classService.GetClassStudentsForAdminAsync(
            Guid.Parse(classId), 
            pageNumber, 
            pageSize, 
            searchTerm);
        return Ok(ApiResponse<object>.SuccessResponse(students, "Students retrieved successfully"));
    }

    // ==================== USER MANAGEMENT ====================

    /// <summary>
    /// [ADMIN] Lấy thống kê người dùng
    /// </summary>
    /// <returns>Thống kê người dùng</returns>
    /// <response code="200">Thống kê người dùng</response>
    [HttpGet("users/statistics")]
    [SwaggerOperation(
        Summary = "[ADMIN] Lấy thống kê người dùng",
        Description = "Lấy thống kê tổng quan về người dùng: tổng số, theo role, theo status")]
    [SwaggerResponse(200, "Thống kê người dùng", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetUserStatistics()
    {
        var statistics = await _userService.GetUserStatisticsAsync();
        return Ok(ApiResponse<object>.SuccessResponse(statistics, "Statistics retrieved successfully"));
    }

    /// <summary>
    /// [ADMIN] Lấy danh sách tất cả người dùng
    /// </summary>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Số lượng mỗi trang</param>
    /// <param name="searchTerm">Từ khóa tìm kiếm (name, email, code)</param>
    /// <param name="role">Lọc theo role (Admin, Teacher, Student)</param>
    /// <param name="isActive">Lọc theo trạng thái active</param>
    /// <returns>Danh sách người dùng</returns>
    /// <response code="200">Danh sách người dùng</response>
    [HttpGet("users")]
    [SwaggerOperation(
        Summary = "[ADMIN] Lấy danh sách người dùng",
        Description = "Lấy danh sách tất cả người dùng với phân trang, tìm kiếm và lọc")]
    [SwaggerResponse(200, "Danh sách người dùng", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null)
    {
        var users = await _userService.GetAllUsersForAdminAsync(
            pageNumber, 
            pageSize, 
            searchTerm, 
            role, 
            isActive);
        return Ok(ApiResponse<object>.SuccessResponse(users, "Users retrieved successfully"));
    }

    /// <summary>
    /// [ADMIN] Lấy chi tiết một người dùng
    /// </summary>
    /// <param name="userId">ID người dùng</param>
    /// <returns>Chi tiết người dùng</returns>
    /// <response code="200">Chi tiết người dùng</response>
    /// <response code="404">Không tìm thấy người dùng</response>
    [HttpGet("users/{userId}")]
    [SwaggerOperation(
        Summary = "[ADMIN] Lấy chi tiết người dùng",
        Description = "Lấy thông tin chi tiết của một người dùng bao gồm thống kê")]
    [SwaggerResponse(200, "Chi tiết người dùng", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "Không tìm thấy người dùng", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetUserDetail(string userId)
    {
        var user = await _userService.GetUserDetailForAdminAsync(userId);
        if (user == null)
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

        return Ok(ApiResponse<object>.SuccessResponse(user, "User detail retrieved successfully"));
    }

    /// <summary>
    /// [ADMIN] Tạo người dùng mới
    /// </summary>
    /// <param name="request">Thông tin người dùng</param>
    /// <returns>Kết quả tạo</returns>
    /// <response code="200">Tạo thành công</response>
    /// <response code="400">Email đã tồn tại hoặc dữ liệu không hợp lệ</response>
    [HttpPost("users")]
    [SwaggerOperation(
        Summary = "[ADMIN] Tạo người dùng mới",
        Description = "Tạo người dùng mới với role Admin/Teacher/Student và thông tin tương ứng")]
    [SwaggerResponse(200, "Tạo thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Email đã tồn tại hoặc dữ liệu không hợp lệ", typeof(ApiResponse<object>))]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserByAdminRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

        try
        {
            var result = await _userService.CreateUserByAdminAsync(request);
            if (!result)
                return BadRequest(ApiResponse<object>.ErrorResponse("Failed to create user"));

            return Ok(ApiResponse<object>.SuccessResponse(null, "User created successfully"));
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// [ADMIN] Cập nhật thông tin người dùng
    /// </summary>
    /// <param name="userId">ID người dùng</param>
    /// <param name="request">Thông tin cập nhật</param>
    /// <returns>Kết quả cập nhật</returns>
    /// <response code="200">Cập nhật thành công</response>
    /// <response code="404">Không tìm thấy người dùng</response>
    [HttpPut("users/{userId}")]
    [SwaggerOperation(
        Summary = "[ADMIN] Cập nhật người dùng",
        Description = "Cập nhật thông tin người dùng: name, email, role, status")]
    [SwaggerResponse(200, "Cập nhật thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "Không tìm thấy người dùng", typeof(ApiResponse<object>))]
    public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserByAdminRequest request)
    {
        var result = await _userService.UpdateUserByAdminAsync(userId, request);
        return Ok(ApiResponse<object>.SuccessResponse(result, "User updated successfully"));
    }

    /// <summary>
    /// [ADMIN] Xóa người dùng
    /// </summary>
    /// <param name="userId">ID người dùng</param>
    /// <returns>Kết quả xóa</returns>
    /// <response code="200">Xóa thành công</response>
    /// <response code="404">Không tìm thấy người dùng</response>
    [HttpDelete("users/{userId}")]
    [SwaggerOperation(
        Summary = "[ADMIN] Xóa người dùng",
        Description = "Xóa vĩnh viễn người dùng khỏi hệ thống")]
    [SwaggerResponse(200, "Xóa thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "Không tìm thấy người dùng", typeof(ApiResponse<object>))]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var result = await _userService.DeleteUserByAdminAsync(userId);
        return Ok(ApiResponse<object>.SuccessResponse(result, "User deleted successfully"));
    }

    /// <summary>
    /// [ADMIN] Thực hiện hành động hàng loạt trên nhiều người dùng
    /// </summary>
    /// <param name="request">Thông tin hành động</param>
    /// <returns>Kết quả thực hiện</returns>
    /// <response code="200">Thực hiện thành công</response>
    [HttpPost("users/bulk-action")]
    [SwaggerOperation(
        Summary = "[ADMIN] Bulk action người dùng",
        Description = "Thực hiện hành động hàng loạt: activate, deactivate, delete, changeRole")]
    [SwaggerResponse(200, "Kết quả thực hiện", typeof(ApiResponse<object>))]
    public async Task<IActionResult> BulkAction([FromBody] BulkUserActionRequest request)
    {
        var result = await _userService.BulkActionAsync(
            request.Action, 
            request.UserIds, 
            request.NewRole);
        return Ok(ApiResponse<object>.SuccessResponse(result, "Bulk action completed"));
    }
}

