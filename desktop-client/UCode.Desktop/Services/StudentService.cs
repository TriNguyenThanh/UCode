using System.Threading.Tasks;
using UCode.Desktop.Models;

namespace UCode.Desktop.Services
{
    public class StudentService
    {
        private readonly ApiService _apiService;

        public StudentService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<ApiResponse<Student>> GetMyProfileAsync()
        {
            return await _apiService.GetAsync<Student>("/api/v1/students/me");
        }

        public async Task<ApiResponse<Student>> UpdateMyProfileAsync(UpdateStudentRequest request)
        {
            return await _apiService.PutAsync<Student>("/api/v1/students/me", request);
        }
    }
}
