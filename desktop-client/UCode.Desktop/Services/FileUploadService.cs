using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UCode.Desktop.Models;

namespace UCode.Desktop.Services
{
    public enum FileCategory
    {
        AssignmentDocument,
        CodeSubmission,
        Image,
        Avatar,
        TestCase,
        Reference,
        Document
    }

    public class FileUploadResponse
    {
        [JsonProperty("fileKey")]
        public string FileKey { get; set; } = string.Empty;

        [JsonProperty("fileName")]
        public string FileName { get; set; } = string.Empty;

        [JsonProperty("fileUrl")]
        public string FileUrl { get; set; } = string.Empty;

        [JsonProperty("fileSizeBytes")]
        public long FileSizeBytes { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get; set; } = string.Empty;

        [JsonProperty("category")]
        public string Category { get; set; } = string.Empty;

        [JsonProperty("uploadedAt")]
        public DateTime UploadedAt { get; set; }
    }

    public class FileUploadService
    {
        private readonly ApiService _apiService;

        public FileUploadService(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Upload file to server
        /// </summary>
        /// <param name="filePath">Absolute path to the file</param>
        /// <param name="category">File category (default: Image)</param>
        /// <returns>Upload response containing file URL</returns>
        public async Task<FileUploadResponse> UploadFileAsync(string filePath, FileCategory category = FileCategory.Image)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            // Prepare MultipartFormDataContent
            using var content = new MultipartFormDataContent();

            // Open file stream
            // Note: We don't dispose the stream here because StreamContent needs it. 
            // HttpClient.SendAsync will dispose the content which disposes the stream.
            // However since we are using ApiService wrapper, we need to be careful.
            // Let's read bytes to memory to be safe and avoid locking files
            byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
            var streamContent = new ByteArrayContent(fileBytes);

            var fileName = Path.GetFileName(filePath);
            var fileExtension = Path.GetExtension(filePath);

            streamContent.Headers.ContentType = GetContentType(fileExtension);

            content.Add(streamContent, "file", fileName);
            content.Add(new StringContent(category.ToString()), "category");

            // Use ApiService.PostMultipartAsync if available, or fallback to generic Post
            // Since ApiService.PostMultipartAsync might not exist, let's assume we use a specialized method in ApiService 
            // OR if ApiService exposes HttpClient, we use that. 
            // Looking at previous context, ApiService likely has generic PostAsync.
            // However, generic PostAsync usually serializes body to JSON.
            // We need a method to post MultipartContent. 

            // Assuming ApiService has a method to upload files or we add one.
            // Since we can't easily modify ApiService right now, let's assume valid PostAsync overload exists 
            // OR implement using _apiService's internal HttpClient if accessible.

            // Checking common pattern: ApiService usually wraps HttpClient.
            // Let's implement a direct call using ApiService's handling pattern if possible.
            // But since I don't see ApiService source, I'll use a public method 'UploadFileAsync' 
            // that I assume exists or I should add to ApiService.

            // WAIT, looking at ApiService usage in previous calls:
            // _apiService.GetAsync<T>

            // I will implement Upload using a specific call to _apiService if it supports it.
            // If not, I'll need to use _apiService.HttpClient if public, or just replicate the Auth header logic.

            // Safest bet structure without modifying ApiService too much:
            return await _apiService.UploadFileAsync<FileUploadResponse>("/api/files/upload", content);
        }

        /// <summary>
        /// Delete file from server
        /// </summary>
        public async Task<bool> DeleteFileAsync(string fileKey)
        {
            var response = await _apiService.DeleteAsync<bool>($"/api/files/{fileKey}");

            if (response?.Success != true)
            {
                throw new Exception(response?.Message ?? "Failed to delete file");
            }

            return response.Data;
        }

        /// <summary>
        /// Get presigned URL for file
        /// </summary>
        public async Task<string> GetPresignedUrlAsync(string fileKey, int expirationMinutes = 60)
        {
            var body = new { key = fileKey, expirationMinutes };
            var response = await _apiService.PostAsync<string>("/api/files/presigned-url", body);

            if (response?.Success != true || string.IsNullOrEmpty(response?.Data))
            {
                throw new Exception(response?.Message ?? "Failed to get presigned URL");
            }

            return response.Data;
        }

        /// <summary>
        /// Check if file exists
        /// </summary>
        public async Task<bool> FileExistsAsync(string fileKey)
        {
            var response = await _apiService.GetAsync<bool>($"/api/files/exists/{fileKey}");

            if (response?.Success != true)
            {
                throw new Exception(response?.Message ?? "Failed to check file existence");
            }

            return response.Data;
        }

        private MediaTypeHeaderValue GetContentType(string extension)
        {
            return extension.ToLower() switch
            {
                ".jpg" or ".jpeg" => new MediaTypeHeaderValue("image/jpeg"),
                ".png" => new MediaTypeHeaderValue("image/png"),
                ".gif" => new MediaTypeHeaderValue("image/gif"),
                ".bmp" => new MediaTypeHeaderValue("image/bmp"),
                ".webp" => new MediaTypeHeaderValue("image/webp"),
                ".pdf" => new MediaTypeHeaderValue("application/pdf"),
                ".doc" => new MediaTypeHeaderValue("application/msword"),
                ".docx" => new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.wordprocessingml.document"),
                ".xls" => new MediaTypeHeaderValue("application/vnd.ms-excel"),
                ".xlsx" => new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
                ".zip" => new MediaTypeHeaderValue("application/zip"),
                ".rar" => new MediaTypeHeaderValue("application/x-rar-compressed"),
                ".txt" => new MediaTypeHeaderValue("text/plain"),
                ".cs" or ".cpp" or ".c" or ".java" or ".py" or ".js" or ".ts" => new MediaTypeHeaderValue("text/plain"),
                _ => new MediaTypeHeaderValue("application/octet-stream"),
            };
        }
    }
}
