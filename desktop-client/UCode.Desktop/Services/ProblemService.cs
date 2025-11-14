using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UCode.Desktop.Models;
using UCode.Desktop.Models.Enums;

namespace UCode.Desktop.Services
{
    public class ProblemService
    {
        private readonly ApiService _apiService;

        public ProblemService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<ApiResponse<Problem>> GetProblemAsync(string problemId)
        {
            return await _apiService.GetAsync<Problem>($"/api/v1/problems/{problemId}");
        }

        public async Task<ApiResponse<PagedResponse<Problem>>> GetMyProblemsAsync(int page = 1, int pageSize = 20)
        {
            return await _apiService.GetAsync<PagedResponse<Problem>>($"/api/v1/problems/all-problems?page={page}&pageSize={pageSize}");
        }

        public async Task<ApiResponse<Problem>> CreateProblemAsync(CreateProblemRequest request)
        {
            return await _apiService.PostAsync<Problem>("/api/v1/problems/create", request);
        }

        public async Task<ApiResponse<Problem>> UpdateProblemAsync(UpdateProblemRequest request)
        {
            return await _apiService.PutAsync<Problem>("/api/v1/problems/update", request);
        }

        public async Task<ApiResponse<bool>> DeleteProblemAsync(string problemId)
        {
            return await _apiService.DeleteAsync($"/api/v1/problems/del?uid={problemId}");
        }

        public async Task<ApiResponse<List<Dataset>>> GetDatasetsAsync(string problemId)
        {
            return await _apiService.GetAsync<List<Dataset>>($"/api/v1/problems/get-datasets?problemId={problemId}");
        }

        public async Task<ApiResponse<List<ProblemLanguage>>> GetAvailableLanguagesAsync(string problemId)
        {
            return await _apiService.GetAsync<List<ProblemLanguage>>($"/api/v1/problems/{problemId}/available-languages");
        }

        public async Task<ApiResponse<ProblemLanguage>> AddOrUpdateProblemLanguagesAsync(string problemId, List<ProblemLanguageRequest> requests)
        {
            
            return await _apiService.PostAsync<ProblemLanguage>($"/api/v1/problems/{problemId}/languages", requests);
        }
    }

    // CreateProblemRequest and UpdateProblemRequest moved to Models/Problem.cs to use enums
}

