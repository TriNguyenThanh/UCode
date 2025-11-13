using System.Collections.Generic;
using System.Threading.Tasks;
using UCode.Desktop.Models;

namespace UCode.Desktop.Services
{
    public class DatasetService
    {
        private readonly ApiService _apiService;

        public DatasetService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<ApiResponse<List<Dataset>>> GetDatasetsByProblemAsync(string problemId)
        {
            return await _apiService.GetAsync<List<Dataset>>($"/api/v1/problems/get-datasets?problemId={problemId}");
        }

        public async Task<ApiResponse<Dataset>> CreateDatasetAsync(CreateDatasetRequest request)
        {
            return await _apiService.PostAsync<Dataset>("/api/v1/datasets/create", request);
        }

        public async Task<ApiResponse<Dataset>> UpdateDatasetAsync(UpdateDatasetRequest request)
        {
            return await _apiService.PutAsync<Dataset>("/api/v1/datasets/update", request);
        }

        public async Task<ApiResponse<bool>> DeleteDatasetAsync(string datasetId)
        {
            return await _apiService.DeleteAsync($"/api/v1/datasets/del?uid={datasetId}");
        }
    }

    public class CreateDatasetRequest
    {
        public string ProblemId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Kind { get; set; } = "SAMPLE";
        public List<TestCaseRequest> TestCases { get; set; } = new();
    }

    public class UpdateDatasetRequest
    {
        public string DatasetId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Kind { get; set; } = "SAMPLE";
        public string ProblemId { get; set; } = string.Empty;
        public List<TestCaseRequest> TestCases { get; set; } = new();
    }

    public class TestCaseRequest
    {
        public string InputRef { get; set; } = string.Empty;
        public string OutputRef { get; set; } = string.Empty;
        public int IndexNo { get; set; }
    }
}

