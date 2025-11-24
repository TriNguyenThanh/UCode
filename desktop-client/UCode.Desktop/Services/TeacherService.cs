using System.Threading.Tasks;
using UCode.Desktop.Models;

namespace UCode.Desktop.Services
{
    public class TeacherService
    {
        private readonly ApiService _apiService;

        public TeacherService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<ApiResponse<User>> GetMyProfileAsync()
        {
            return await _apiService.GetAsync<User>("/api/v1/teachers/me");
        }

        public async Task<ApiResponse<User>> UpdateMyProfileAsync(UpdateTeacherRequest request)
        {
            return await _apiService.PutAsync<User>("/api/v1/teachers/me", request);
        }
    }
}
