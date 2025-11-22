using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UCode.Desktop.Services
{
    public enum FileType
    {
        Image,
        Document
    }

    public class FileUploadResult
    {
        [JsonProperty("fileUrl")]
        public string FileUrl { get; set; } = string.Empty;

        [JsonProperty("fileName")]
        public string FileName { get; set; } = string.Empty;
    }

    public class FileUploadService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public FileUploadService()
        {
            _httpClient = new HttpClient();
            // TODO: Get base URL from configuration
            _baseUrl = "http://localhost:5000"; // Replace with your actual API base URL
        }

        public async Task<FileUploadResult> UploadFileAsync(string filePath, FileType fileType)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            using var content = new MultipartFormDataContent();
            var fileStream = File.OpenRead(filePath);
            var streamContent = new StreamContent(fileStream);
            
            var fileName = Path.GetFileName(filePath);
            var fileExtension = Path.GetExtension(filePath);
            
            // Set content type based on file extension
            streamContent.Headers.ContentType = GetContentType(fileExtension);
            content.Add(streamContent, "file", fileName);
            content.Add(new StringContent(fileType.ToString()), "type");

            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/files/upload", content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<FileUploadResult>(jsonResponse);

                return result ?? throw new Exception("Failed to parse upload response");
            }
            finally
            {
                fileStream?.Dispose();
            }
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
                _ => new MediaTypeHeaderValue("application/octet-stream"),
            };
        }
    }
}
