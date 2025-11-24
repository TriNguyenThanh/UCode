using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models;

namespace UCode.Desktop.Services
{
    /// <summary>
    /// Base service class với centralized error handling và response unwrapping
    /// Tương tự như handleApiError và unwrapApiResponse trong Web client
    /// </summary>
    public abstract class BaseApiService
    {
        protected readonly HttpClient _httpClient;
        protected readonly IDialogCoordinator _dialogCoordinator;
        protected readonly TokenStorageService? _tokenStorage;
        protected readonly AuthService? _authService;
        
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Converters = { new VietnamDateTimeConverter() }
        };

        protected BaseApiService(HttpClient httpClient, IDialogCoordinator dialogCoordinator, TokenStorageService? tokenStorage = null, AuthService? authService = null)
        {
            _httpClient = httpClient;
            _dialogCoordinator = dialogCoordinator;
            _tokenStorage = tokenStorage;
            _authService = authService;
            
            // Set base address to api-gateway port
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri("http://localhost:5000/");
            }
        }
        
        /// <summary>
        /// Ensure token is set in HttpClient headers
        /// Priority: 1. AuthService memory token (for non-remember-me sessions)
        ///           2. TokenStorage file token (for remember-me sessions)
        /// </summary>
        protected void EnsureToken()
        {
            // First try to get token from AuthService memory (works for both remember-me and non-remember-me)
            if (_authService != null && !string.IsNullOrEmpty(_authService.AccessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authService.AccessToken);
                return;
            }
            
            // Fallback to token from file (for remember-me when app restarts)
            if (_tokenStorage != null)
            {
                var tokenData = _tokenStorage.LoadToken();
                if (tokenData != null && !string.IsNullOrEmpty(tokenData.AccessToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenData.AccessToken);
                }
            }
        }

        /// <summary>
        /// Execute GET request với auto unwrap ApiResponse<T>
        /// </summary>
        protected async Task<T?> GetAsync<T>(string endpoint, string errorContext = "tải dữ liệu") where T : class
        {
            EnsureToken(); // Ensure token before request
            return await ExecuteAsync<T>(
                () => _httpClient.GetAsync(endpoint),
                errorContext
            );
        }

        /// <summary>
        /// Execute POST request với auto unwrap ApiResponse<T>
        /// </summary>
        protected async Task<T?> PostAsync<T>(string endpoint, object data, string errorContext = "lưu dữ liệu") where T : class
        {
            EnsureToken(); // Ensure token before request
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await ExecuteAsync<T>(
                () => _httpClient.PostAsync(endpoint, content),
                errorContext
            );
        }

        /// <summary>
        /// Execute PUT request với auto unwrap ApiResponse<T>
        /// </summary>
        protected async Task<T?> PutAsync<T>(string endpoint, object data, string errorContext = "cập nhật dữ liệu") where T : class
        {
            EnsureToken(); // Ensure token before request
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await ExecuteAsync<T>(
                () => _httpClient.PutAsync(endpoint, content),
                errorContext
            );
        }

        /// <summary>
        /// Execute PATCH request với auto unwrap ApiResponse<T>
        /// </summary>
        protected async Task<T?> PatchAsync<T>(string endpoint, object data, string errorContext = "cập nhật dữ liệu") where T : class
        {
            EnsureToken(); // Ensure token before request
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint)
            {
                Content = content
            };

            return await ExecuteAsync<T>(
                () => _httpClient.SendAsync(request),
                errorContext
            );
        }

        /// <summary>
        /// Execute DELETE request
        /// </summary>
        protected async Task<bool> DeleteAsync(string endpoint, string errorContext = "xóa dữ liệu")
        {
            EnsureToken(); // Ensure token before request
            var result = await ExecuteAsync<object>(
                () => _httpClient.DeleteAsync(endpoint),
                errorContext
            );

            return result != null;
        }

        /// <summary>
        /// Execute POST request without return data (chỉ cần success/fail)
        /// </summary>
        protected async Task<bool> PostWithoutResponseAsync(string endpoint, object data, string errorContext = "lưu dữ liệu")
        {
            var result = await PostAsync<object>(endpoint, data, errorContext);
            return result != null;
        }

        /// <summary>
        /// Execute PUT request without return data (chỉ cần success/fail)
        /// </summary>
        protected async Task<bool> PutWithoutResponseAsync(string endpoint, object data, string errorContext = "cập nhật dữ liệu")
        {
            var result = await PutAsync<object>(endpoint, data, errorContext);
            return result != null;
        }

        /// <summary>
        /// Core execution method - giống handleApiError của Web
        /// Unwrap ApiResponse<T> → T (giống unwrapApiResponse của Web)
        /// </summary>
        private async Task<T?> ExecuteAsync<T>(
            Func<Task<HttpResponseMessage>> httpCall,
            string errorContext) where T : class
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[BaseApiService] Calling API for: {errorContext}");
                var response = await httpCall();
                var content = await response.Content.ReadAsStringAsync();
                
                System.Diagnostics.Debug.WriteLine($"[BaseApiService] Response status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[BaseApiService] Response content length: {content?.Length ?? 0}");

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[BaseApiService] ERROR: Status code {response.StatusCode}");
                    await HandleApiErrorAsync(response, content, errorContext);
                    return null;
                }

                // Unwrap ApiResponse<T> → T (giống unwrapApiResponse của Web)
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(content, JsonSettings);

                if (apiResponse == null || !apiResponse.Success)
                {
                    System.Diagnostics.Debug.WriteLine($"[BaseApiService] ERROR: ApiResponse is null or not successful");
                    await ShowErrorDialogAsync(
                        $"Lỗi {errorContext}",
                        apiResponse?.Message ?? "Không thể xử lý phản hồi từ server"
                    );
                    return null;
                }
                
                System.Diagnostics.Debug.WriteLine($"[BaseApiService] SUCCESS: Data retrieved for {errorContext}");
                return apiResponse.Data;
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BaseApiService] HttpRequestException: {ex.Message}");
                await ShowErrorDialogAsync(
                    "Lỗi kết nối",
                    $"Không thể kết nối đến server: {ex.Message}"
                );
                return null;
            }
            catch (TaskCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BaseApiService] TaskCanceledException: {ex.Message}");
                await ShowErrorDialogAsync(
                    "Timeout",
                    $"Yêu cầu hết thời gian chờ: {ex.Message}"
                );
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BaseApiService] Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[BaseApiService] StackTrace: {ex.StackTrace}");
                await ShowErrorDialogAsync(
                    $"Lỗi {errorContext}",
                    ex.Message
                );
                return null;
            }
        }

        /// <summary>
        /// Handle API errors - giống handleApiError của Web
        /// </summary>
        private async Task HandleApiErrorAsync(
            HttpResponseMessage response,
            string content,
            string context)
        {
            var errorMsg = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.",
                HttpStatusCode.Forbidden => "Bạn không có quyền thực hiện thao tác này.",
                HttpStatusCode.NotFound => "Không tìm thấy dữ liệu yêu cầu.",
                HttpStatusCode.BadRequest => ParseBadRequestError(content),
                HttpStatusCode.TooManyRequests => "Quá nhiều yêu cầu. Vui lòng thử lại sau.",
                HttpStatusCode.InternalServerError => "Lỗi server. Vui lòng thử lại sau.",
                HttpStatusCode.ServiceUnavailable => "Dịch vụ tạm thời không khả dụng.",
                _ => $"Lỗi {(int)response.StatusCode}: {response.ReasonPhrase}"
            };

            await ShowErrorDialogAsync($"Lỗi {context}", errorMsg);
        }

        /// <summary>
        /// Parse BadRequest errors từ ApiResponse
        /// </summary>
        private string ParseBadRequestError(string content)
        {
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(content, JsonSettings);
                if (errorResponse?.Errors != null && errorResponse.Errors.Count > 0)
                {
                    return string.Join("\n", errorResponse.Errors);
                }
                return errorResponse?.Message ?? "Dữ liệu không hợp lệ";
            }
            catch
            {
                return "Dữ liệu không hợp lệ";
            }
        }

        /// <summary>
        /// Show error dialog - sử dụng IDialogCoordinator
        /// </summary>
        protected async Task ShowErrorDialogAsync(string title, string message)
        {
            try
            {
                // Run on UI thread using Dispatcher
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await _dialogCoordinator.ShowMessageAsync(
                        this,
                        title,
                        message,
                        MessageDialogStyle.Affirmative
                    );
                });
            }
            catch (Exception ex)
            {
                // Fallback nếu DialogCoordinator không hoạt động
                System.Diagnostics.Debug.WriteLine($"Error showing dialog: {ex.Message}");
                MessageBox.Show(
                    $"{title}\n\n{message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        /// <summary>
        /// Show success dialog
        /// </summary>
        protected async Task ShowSuccessDialogAsync(string title, string message)
        {
            try
            {
                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    title,
                    message,
                    MessageDialogStyle.Affirmative
                );
            }
            catch
            {
                System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Show confirmation dialog
        /// </summary>
        protected async Task<bool> ShowConfirmDialogAsync(string title, string message)
        {
            try
            {
                var result = await _dialogCoordinator.ShowMessageAsync(
                    this,
                    title,
                    message,
                    MessageDialogStyle.AffirmativeAndNegative
                );
                return result == MessageDialogResult.Affirmative;
            }
            catch
            {
                var result = System.Windows.MessageBox.Show(
                    message, 
                    title, 
                    System.Windows.MessageBoxButton.YesNo, 
                    System.Windows.MessageBoxImage.Question
                );
                return result == System.Windows.MessageBoxResult.Yes;
            }
        }

        /// <summary>
        /// Download file từ API
        /// </summary>
        protected async Task<byte[]?> DownloadFileAsync(string endpoint, string errorContext = "tải file")
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    await HandleApiErrorAsync(response, content, errorContext);
                    return null;
                }

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (HttpRequestException ex)
            {
                await ShowErrorDialogAsync(
                    "Lỗi kết nối",
                    $"Không thể kết nối đến server: {ex.Message}"
                );
                return null;
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync(
                    $"Lỗi {errorContext}",
                    ex.Message
                );
                return null;
            }
        }
    }
}
