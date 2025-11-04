using Microsoft.AspNetCore.Mvc;
using file_service.Models;
using file_service.Services;
using file_service.Enums;

namespace file_service.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IS3Service _s3Service;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IS3Service s3Service, ILogger<FilesController> logger)
    {
        _s3Service = s3Service;
        _logger = logger;
    }

    /// <summary>
    /// Upload a file to S3 with category-based validation
    /// </summary>
    /// <param name="file">File to upload</param>
    /// <param name="category">File category (1=AssignmentDocument, 2=CodeSubmission, 3=Image, 4=Avatar, 5=TestCase, 6=Reference)</param>
    /// <param name="customFileName">Custom file name (optional)</param>
    [HttpPost("upload")]
    [RequestSizeLimit(100_000_000)] // 100MB global limit
    [ProducesResponseType(typeof(ApiResponse<FileUploadResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadFile(
        [FromForm] IFormFile file, 
        [FromForm] FileCategory category,
        [FromForm] string? customFileName = null)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("No file provided"));
            }

            // Validate category
            if (!FileCategoryConfiguration.IsValidCategory(category))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Invalid file category: {category}"));
            }

            var result = await _s3Service.UploadFileAsync(file, category, customFileName);
            return Ok(ApiResponse<FileUploadResponse>.SuccessResponse(result, "File uploaded successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "File validation failed");
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error uploading file", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Download a file from S3
    /// </summary>
    [HttpGet("download/{*key}")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadFile(string key)
    {
        try
        {
            var fileExists = await _s3Service.FileExistsAsync(key);
            if (!fileExists)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("File not found"));
            }

            var stream = await _s3Service.DownloadFileAsync(key);
            var fileName = Path.GetFileName(key);
            
            return File(stream, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error downloading file", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Delete a file from S3
    /// </summary>
    [HttpDelete("{*key}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFile(string key)
    {
        try
        {
            var fileExists = await _s3Service.FileExistsAsync(key);
            if (!fileExists)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("File not found"));
            }

            var result = await _s3Service.DeleteFileAsync(key);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "File deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error deleting file", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get a presigned URL for a file
    /// </summary>
    [HttpPost("presigned-url")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPresignedUrl([FromBody] PresignedUrlRequest request)
    {
        try
        {
            var fileExists = await _s3Service.FileExistsAsync(request.Key);
            if (!fileExists)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("File not found"));
            }

            var url = await _s3Service.GetPresignedUrlAsync(request.Key, request.ExpirationMinutes);
            return Ok(ApiResponse<string>.SuccessResponse(url, "Presigned URL generated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error generating presigned URL", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// List files in S3 bucket
    /// </summary>
    [HttpGet("list")]
    [ProducesResponseType(typeof(ApiResponse<List<Models.FileInfo>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListFiles([FromQuery] string? prefix = null)
    {
        try
        {
            var files = await _s3Service.ListFilesAsync(prefix);
            return Ok(ApiResponse<List<Models.FileInfo>>.SuccessResponse(files, $"Found {files.Count} files"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error listing files", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Check if a file exists
    /// </summary>
    [HttpHead("{*key}")]
    [HttpGet("exists/{*key}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> FileExists(string key)
    {
        try
        {
            var exists = await _s3Service.FileExistsAsync(key);
            return Ok(ApiResponse<bool>.SuccessResponse(exists, exists ? "File exists" : "File does not exist"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error checking file existence", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get all available file categories with their configurations
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult GetFileCategories()
    {
        try
        {
            var categories = Enum.GetValues<FileCategory>()
                .Select(category =>
                {
                    var config = FileCategoryConfiguration.GetConfiguration(category);
                    return new
                    {
                        id = (int)category,
                        name = category.ToString(),
                        folderPath = config.FolderPath,
                        maxFileSizeMB = config.MaxFileSizeBytes / (1024.0 * 1024.0),
                        allowedExtensions = config.AllowedExtensions,
                        allowedMimeTypes = config.AllowedMimeTypes
                    };
                })
                .ToList();

            return Ok(ApiResponse<object>.SuccessResponse(categories, "File categories retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file categories");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error getting file categories", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get specific file category configuration
    /// </summary>
    [HttpGet("categories/{category}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult GetFileCategoryConfig(FileCategory category)
    {
        try
        {
            if (!FileCategoryConfiguration.IsValidCategory(category))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Invalid file category: {category}"));
            }

            var config = FileCategoryConfiguration.GetConfiguration(category);
            var result = new
            {
                id = (int)category,
                name = category.ToString(),
                folderPath = config.FolderPath,
                maxFileSizeMB = config.MaxFileSizeBytes / (1024.0 * 1024.0),
                maxFileSizeBytes = config.MaxFileSizeBytes,
                allowedExtensions = config.AllowedExtensions,
                allowedMimeTypes = config.AllowedMimeTypes
            };

            return Ok(ApiResponse<object>.SuccessResponse(result, "Category configuration retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category configuration");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error getting category configuration", new List<string> { ex.Message }));
        }
    }
}
