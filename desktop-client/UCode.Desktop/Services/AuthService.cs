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
        private readonly TokenStorageService _tokenStorage;
        private User _currentUser;
        private string _accessToken;
        private string _refreshToken;
        private long _expiresAt;

        public User CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null && !string.IsNullOrEmpty(_accessToken);

        public event EventHandler<User> UserChanged;

        public AuthService(ApiService apiService, TokenStorageService tokenStorage)
        {
            _apiService = apiService;
            _tokenStorage = tokenStorage;
        }

        public async Task<bool> TryAutoLoginAsync()
        {
            System.Diagnostics.Debug.WriteLine("=== TryAutoLoginAsync started ===");
            var tokenData = _tokenStorage.LoadToken();
            
            if (tokenData == null)
            {
                System.Diagnostics.Debug.WriteLine("No token data found");
                return false;
            }
            
            System.Diagnostics.Debug.WriteLine("Token data loaded");
            
            if (_tokenStorage.IsTokenValid(tokenData))
            {
                System.Diagnostics.Debug.WriteLine("Token is valid, attempting auto-login");
                _accessToken = tokenData.AccessToken;
                _refreshToken = tokenData.RefreshToken;
                _expiresAt = tokenData.ExpiresAt;
                
                _apiService.SetAccessToken(_accessToken);
                
                // Restore user from saved JSON
                try
                {
                    if (!string.IsNullOrEmpty(tokenData.UserJson))
                    {
                        _currentUser = System.Text.Json.JsonSerializer.Deserialize<User>(tokenData.UserJson);
                        if (_currentUser != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Auto-login successful for user: {_currentUser.Username}");
                            UserChanged?.Invoke(this, _currentUser);
                            return true;
                        }
                    }
                    
                    System.Diagnostics.Debug.WriteLine("Failed to restore user from token");
                    _tokenStorage.ClearToken();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error during auto-login: {ex.Message}");
                    // Token might be invalid, clear it
                    _tokenStorage.ClearToken();
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Token expired, clearing...");
                // Token expired, clear it
                _tokenStorage.ClearToken();
            }
            
            return false;
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
                _expiresAt = response.Data.ExpiresAt;
                _currentUser = response.Data.User;

                _apiService.SetAccessToken(_accessToken);
                
                // Save token if remember me is checked
                if (rememberMe)
                {
                    var userJson = System.Text.Json.JsonSerializer.Serialize(_currentUser);
                    _tokenStorage.SaveToken(
                        _accessToken, 
                        _refreshToken, 
                        _expiresAt,
                        userJson);
                }
                
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
            _expiresAt = 0;

            _tokenStorage.ClearToken();
            _apiService.SetAccessToken(null);
            UserChanged?.Invoke(this, null);
        }

        public async Task<User> GetCurrentUserAsync()
        {
            if (_currentUser != null)
            {
                return _currentUser;
            }

            if (string.IsNullOrEmpty(_accessToken))
            {
                return null;
            }

            try
            {
                var response = await _apiService.GetAsync<User>("/api/v1/auth/me");
                if (response.Success && response.Data != null)
                {
                    _currentUser = response.Data;
                    return _currentUser;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting current user: {ex.Message}");
            }

            return null;
        }
    }
}
