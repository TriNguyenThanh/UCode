using System.Collections.Generic;
using System.Threading.Tasks;
using UCode.Desktop.Models;

namespace UCode.Desktop.Services
{
    public class TagService
    {
        private readonly ApiService _apiService;

        public TagService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<ApiResponse<List<Tag>>> GetAllTagsAsync()
        {
            return await _apiService.GetAsync<List<Tag>>("/api/v1/tags");
        }

        public async Task<ApiResponse<bool>> AddTagsToProblemAsync(string problemId, List<string> tagIds)
        {
            return await _apiService.PostAsync<bool>($"/api/v1/problems/{problemId}/tags", tagIds);
        }

        public async Task<ApiResponse<bool>> RemoveTagFromProblemAsync(string problemId, string tagId)
        {
            return await _apiService.DeleteAsync($"/api/v1/problems/{problemId}/tags/{tagId}");
        }
    }
}

