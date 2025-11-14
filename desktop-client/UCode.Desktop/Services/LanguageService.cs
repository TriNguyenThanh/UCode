using System.Collections.Generic;
using System.Threading.Tasks;
using UCode.Desktop.Models;

namespace UCode.Desktop.Services
{
    public class LanguageService
    {
        private readonly ApiService _apiService;

        public LanguageService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<ApiResponse<List<Language>>> GetAllLanguagesAsync(bool enabledOnly = false)
        {
            var url = $"/api/v1/languages?enabledOnly={enabledOnly}";
            return await _apiService.GetAsync<List<Language>>(url);
        }

        public async Task<ApiResponse<Language>> GetLanguageByCodeAsync(string code)
        {
            return await _apiService.GetAsync<Language>($"/api/v1/languages/by-code?code={code}");
        }

        public async Task<ApiResponse<bool>> EnableLanguageAsync(string languageId)
        {
            return await _apiService.PostAsync<bool>($"/api/v1/languages/{languageId}/enable", null);
        }

        public async Task<ApiResponse<List<ProblemLanguage>>> GetProblemLanguagesAsync(string problemId)
        {
            return await _apiService.GetAsync<List<ProblemLanguage>>($"/api/v1/problems/{problemId}/available-languages");
        }

        public async Task<ApiResponse<List<ProblemLanguage>>> AddOrUpdateProblemLanguagesAsync(string problemId, List<ProblemLanguageRequest> languages)
        {
            return await _apiService.PostAsync<List<ProblemLanguage>>($"/api/v1/problems/{problemId}/languages", languages);
        }
    }
}

