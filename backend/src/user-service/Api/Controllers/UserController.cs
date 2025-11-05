using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace UserService.Api.Controllers;

/// <summary>
/// Controller quản lý thông tin User
/// </summary>
[ApiController]
[Route("api/v1/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Lấy thông tin user theo ID
    /// </summary>
    /// <param name="id">ID của user</param>
    /// <returns>Thông tin user</returns>
    /// <response code="200">Trả về thông tin user thành công</response>
    /// <response code="404">Không tìm thấy user</response>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get user by ID", Description = "Lấy thông tin user theo ID")]
    [SwaggerResponse(200, "Success", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "User not found", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetUser(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

        return Ok(ApiResponse<object>.SuccessResponse(user, "User retrieved successfully"));
    }

    /// <summary>
    /// Lấy thông tin user theo email
    /// </summary>
    /// <param name="email">Email của user</param>
    /// <returns>Thông tin user</returns>
    /// <response code="200">Trả về thông tin user</response>
    /// <response code="404">Không tìm thấy user</response>
    [HttpGet("by-email/{email}")]
    [SwaggerOperation(Summary = "Get user by email", Description = "Lấy thông tin user theo email")]
    [SwaggerResponse(200, "Thông tin user", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "User not found", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var user = await _userService.GetUserByEmailAsync(email);
        if (user == null)
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

        return Ok(ApiResponse<object>.SuccessResponse(user, "User retrieved successfully"));
    }

    /// <summary>
    /// Lấy thông tin user theo username
    /// </summary>
    /// <param name="username">Username của user</param>
    /// <returns>Thông tin user</returns>
    /// <response code="200">Trả về thông tin user</response>
    /// <response code="404">Không tìm thấy user</response>
    [HttpGet("by-username/{username}")]
    [SwaggerOperation(Summary = "Get user by username", Description = "Lấy thông tin user theo username")]
    [SwaggerResponse(200, "Thông tin user", typeof(ApiResponse<object>))]
    [SwaggerResponse(404, "User not found", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetUserByUsername(string username)
    {
        var user = await _userService.GetUserByUsernameAsync(username);
        if (user == null)
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

        return Ok(ApiResponse<object>.SuccessResponse(user, "User retrieved successfully"));
    }

    /// <summary>
    /// Lấy danh sách user
    /// </summary>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Số lượng mỗi trang</param>
    /// <param name="role">Vai trò (lọc)</param>
    /// <param name="status">Trạng thái (lọc)</param>
    /// <returns>Danh sách user</returns>
    /// <response code="200">Trả về danh sách user</response>
    [HttpGet]
    [SwaggerOperation(Summary = "Get users", Description = "Lấy danh sách user có phân trang và lọc theo vai trò, trạng thái")]
    [SwaggerResponse(200, "Danh sách user", typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] UserRole? role = null,
        [FromQuery] UserStatus? status = null)
    {
        var users = await _userService.GetUsersAsync(pageNumber, pageSize, role, status);
        return Ok(ApiResponse<object>.SuccessResponse(users, "Users retrieved successfully"));
    }

    /// <summary>
    /// Cập nhật thông tin user
    /// </summary>
    /// <param name="id">ID của user cần cập nhật</param>
    /// <param name="request">Thông tin cập nhật</param>
    /// <returns>Trạng thái cập nhật</returns>
    /// <response code="200">Cập nhật thành công</response>
    /// <response code="400">Cập nhật thất bại</response>
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Update user", Description = "Cập nhật thông tin user")]
    [SwaggerResponse(200, "Cập nhật thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Cập nhật thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserByAdminRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

        // Tạo UpdateUserRequest từ UpdateUserByAdminRequest
        var updateRequest = new UpdateUserRequest
        {
            UserId = Guid.Parse(id),
            Email = request.Email,
            FullName = request.FullName,
            Phone = request.Phone,
            Major = request.Major,
            ClassYear = request.ClassYear,
            Department = request.Department,
            Title = request.Title
        };

        var result = await _userService.UpdateUserAsync(id, updateRequest);
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to update user"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "User updated successfully"));
    }

    /// <summary>
    /// Đổi mật khẩu user
    /// </summary>
    /// <param name="request">Mật khẩu hiện tại và mật khẩu mới</param>
    /// <returns>Trạng thái đổi mật khẩu</returns>
    /// <response code="200">Đổi mật khẩu thành công</response>
    /// <response code="400">Đổi mật khẩu thất bại</response>
    [HttpPost("change-password")]
    [SwaggerOperation(Summary = "Change password", Description = "Đổi mật khẩu user")]
    [SwaggerResponse(200, "Đổi mật khẩu thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Đổi mật khẩu thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

        var result = await _userService.ChangePasswordAsync(request);
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to change password"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Password changed successfully"));
    }

    /// <summary>
    /// Cập nhật trạng thái user
    /// </summary>
    /// <param name="request">ID user và trạng thái mới</param>
    /// <returns>Trạng thái cập nhật</returns>
    /// <response code="200">Cập nhật thành công</response>
    /// <response code="400">Cập nhật thất bại</response>
    [HttpPatch("update-status")]
    [SwaggerOperation(Summary = "Update user status", Description = "Cập nhật trạng thái user")]
    [SwaggerResponse(200, "Cập nhật thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Cập nhật thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> UpdateUserStatus([FromBody] UpdateUserStatusRequest request)
    {
        var result = await _userService.UpdateUserStatusAsync(request.UserId, request.Status);
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to update user status"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "User status updated successfully"));
    }

    /// <summary>
    /// Xóa user
    /// </summary>
    /// <param name="id">ID user</param>
    /// <returns>Trạng thái xóa</returns>
    /// <response code="200">Xóa thành công</response>
    /// <response code="400">Xóa thất bại</response>
    [HttpDelete("delete")]
    [SwaggerOperation(Summary = "Delete user", Description = "Xóa một user")]
    [SwaggerResponse(200, "Xóa thành công", typeof(ApiResponse<object>))]
    [SwaggerResponse(400, "Xóa thất bại", typeof(ApiResponse<object>))]
    public async Task<IActionResult> DeleteUser([FromQuery] string id)
    {
        var result = await _userService.DeleteUserAsync(id);
        if (!result)
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to delete user"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "User deleted successfully"));
    }
}

// Helper DTO for UpdateUserStatus
public class UpdateUserStatusRequest
{
    public string UserId { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
}

