using System;
using System.Threading.Tasks;
using UCode.Desktop.Models;

namespace UCode.Desktop.Services
{
    public class LoginRequest
    {
        public string EmailOrUsername { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresAt { get; set; }
        public User User { get; set; }
    }

    public class AuthService
    {
        private readonly ApiService _apiService;
        private User _currentUser;
        private string _accessToken;
        private string _refreshToken;

        public User CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null && !string.IsNullOrEmpty(_accessToken);

        public event EventHandler<User> UserChanged;

        public AuthService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<(bool Success, string Message)> LoginAsync(string emailOrUsername, string password, bool rememberMe = false)
        {
            var request = new LoginRequest
            {
                EmailOrUsername = emailOrUsername,
                Password = password,
                RememberMe = rememberMe
            };

            var response = await _apiService.PostAsync<LoginResponse>("/api/v1/auth/login", request);

            if (response.Success && response.Data != null)
            {
                _accessToken = response.Data.AccessToken;
                _refreshToken = response.Data.RefreshToken;
                _currentUser = response.Data.User;

                _apiService.SetAccessToken(_accessToken);
                UserChanged?.Invoke(this, _currentUser);

                return (true, "Login successful");
            }

            return (false, response.Message ?? "Login failed");
        }

        public void Logout()
        {
            _accessToken = null;
            _refreshToken = null;
            _currentUser = null;

            _apiService.SetAccessToken(null);
            UserChanged?.Invoke(this, null);
        }

        public async Task<User> GetCurrentUserAsync()
        {
            if (_currentUser != null)
            {
                return _currentUser;
            }

            // TODO: Implement get current user from API if token exists
            return null;
        }
    }
}
