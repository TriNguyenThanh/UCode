using UserService.Application.DTOs.Requests;
using UserService.Application.DTOs.Responses;

namespace UserService.Application.Interfaces.Services;

public interface IAdminService
{
    Task<AdminResponse> CreateAdminAsync(CreateAdminRequest request);
    Task<AdminResponse?> GetAdminByIdAsync(string adminId);
}

