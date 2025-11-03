using AutoMapper;
using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using UserService.Application.DTOs.Responses;
using UserService.Application.Interfaces.Repositories;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Infrastructure.Services;

public class AdminAppService : IAdminService
{
    private readonly IRepository<Admin> _adminRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public AdminAppService(
        IRepository<Admin> adminRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _adminRepository = adminRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<AdminResponse> CreateAdminAsync(CreateAdminRequest request)
    {
        // Validate unique constraints
        if (await _userRepository.UsernameExistsAsync(request.Username))
            throw new ApiException("Username already exists");

        if (await _userRepository.EmailExistsAsync(request.Email))
            throw new ApiException("Email already exists");

        var admin = new Admin
        {
            UserId = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _adminRepository.AddAsync(admin);
        return _mapper.Map<AdminResponse>(admin);
    }

    public async Task<AdminResponse?> GetAdminByIdAsync(string adminId)
    {
        var admin = await _adminRepository.GetByIdAsync(Guid.Parse(adminId));
        return admin != null ? _mapper.Map<AdminResponse>(admin) : null;
    }
}

