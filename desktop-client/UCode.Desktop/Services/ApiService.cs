using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models;

namespace UCode.Desktop.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private string _accessToken;
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Converters = { new UtcToLocalDateTimeConverter() }
        };

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://localhost:5000/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void SetAccessToken(string token)
        {
            _accessToken = token;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                var fullUrl = $"{_httpClient.BaseAddress}{endpoint}";
                System.Diagnostics.Debug.WriteLine($"[ApiService] GET {fullUrl}");
                System.Diagnostics.Debug.WriteLine($"[ApiService] Authorization: {(_httpClient.DefaultRequestHeaders.Authorization != null ? "Bearer ***" : "None")}");
                
                var response = await _httpClient.GetAsync(endpoint);
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[ApiService] Response Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[ApiService] Response Content: {content.Substring(0, Math.Min(500, content.Length))}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<ApiResponse<T>>(content, JsonSettings);
                    if (result != null && result.Data is object data)
                    {
                        var dataIsNull = data == null || (data is System.Collections.ICollection collection && collection.Count == 0);
                        System.Diagnostics.Debug.WriteLine($"[ApiService] Deserialized Success: {result.Success}, Data null/empty: {dataIsNull}");
                    }
                    return result;
                }

                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(content, JsonSettings);
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = errorResponse?.Message ?? "Request failed",
                    Errors = errorResponse?.Errors != null ? new System.Collections.Generic.List<string> { errorResponse.Message } : null
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] Exception: {ex}");
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<ApiResponse<T>>(responseContent, JsonSettings);
                }

                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent, JsonSettings);
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = errorResponse?.Message ?? "Request failed"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<ApiResponse<T>>(responseContent, JsonSettings);
                }

                return new ApiResponse<T>
                {
                    Success = false,
                    Message = "Request failed"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Data = true
                    };
                }

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Delete failed"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<TResponse>(responseContent, JsonSettings);
                }

                return default(TResponse);
            }
            catch (Exception)
            {
                return default(TResponse);
            }
        }

        public async Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<TResponse>(responseContent, JsonSettings);
                }

                return default(TResponse);
            }
            catch (Exception)
            {
                return default(TResponse);
            }
        }

        public async Task<T> DeleteAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<T>(content, JsonSettings);
                }

                return default(T);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public async Task<byte[]> GetBytesAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
