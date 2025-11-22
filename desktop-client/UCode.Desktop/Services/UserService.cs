using System.Threading.Tasks;
using UCode.Desktop.Models;

namespace UCode.Desktop.Services
{
    /// <summary>
    /// Service for user-related API operations
    /// </summary>
    public class UserService
    {
        private readonly ApiService _apiService;
        private readonly AuthService _authService;

        public UserService(ApiService apiService, AuthService authService)
        {
            _apiService = apiService;
            _authService = authService;
        }

        /// <summary>
        /// Get user information by ID
        /// </summary>
        public async Task<ApiResponse<User>> GetUserByIdAsync(string userId)
        {
            return await _apiService.GetAsync<User>($"/api/v1/users/{userId}");
        }

        /// <summary>
        /// Get user information by email
        /// </summary>
        public async Task<ApiResponse<User>> GetUserByEmailAsync(string email)
        {
            return await _apiService.GetAsync<User>($"/api/v1/users/by-email/{email}");
        }

        /// <summary>
        /// Get current user information
        /// </summary>
        public async Task<ApiResponse<User>> GetCurrentUserAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
            {
                return new ApiResponse<User>
                {
                    Success = false,
                    Message = "User not authenticated",
                    Data = null
                };
            }

            return await GetUserByIdAsync(currentUser.UserId);
        }

        /// <summary>
        /// Update user profile information
        /// </summary>
        public async Task<ApiResponse<object>> UpdateUserAsync(string userId, UpdateUserRequest request)
        {
            return await _apiService.PutAsync<object>($"/api/v1/users/{userId}", request);
        }

        /// <summary>
        /// Change user password
        /// </summary>
        public async Task<ApiResponse<object>> ChangePasswordAsync(ChangePasswordRequest request)
        {
            return await _apiService.PostAsync<object>("/api/v1/users/change-password", request);
        }
    }
}
