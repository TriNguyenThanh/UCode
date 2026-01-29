namespace UserService.Application.Interfaces.Services;

/// <summary>
/// Interface để gọi Face Recognition Service API
/// </summary>
public interface IFaceServiceClient
{
    /// <summary>
    /// Đăng ký khuôn mặt mới cho user
    /// </summary>
    /// <param name="userId">User ID (GUID string)</param>
    /// <param name="imageBase64">Ảnh khuôn mặt dạng base64</param>
    /// <returns>Kết quả đăng ký</returns>
    Task<FaceRegisterResult> RegisterFaceAsync(string userId, string imageBase64);

    /// <summary>
    /// Xác thực khuôn mặt 1:1
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="imageBase64">Ảnh khuôn mặt dạng base64</param>
    /// <param name="threshold">Ngưỡng similarity (default 0.4)</param>
    /// <returns>Kết quả xác thực</returns>
    Task<FaceVerifyResult> VerifyFaceAsync(string userId, string imageBase64, float threshold = 0.4f);

    /// <summary>
    /// Xóa khuôn mặt đã đăng ký
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>True nếu xóa thành công</returns>
    Task<bool> DeleteFaceAsync(string userId);

    /// <summary>
    /// Kiểm tra trạng thái Face Service
    /// </summary>
    /// <returns>Thông tin health check</returns>
    Task<FaceServiceHealthResult?> HealthCheckAsync();
}

/// <summary>
/// Kết quả đăng ký khuôn mặt
/// </summary>
public class FaceRegisterResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public float? Confidence { get; set; }
    public string? ImageFilename { get; set; }
    
    // Error info
    public string? MatchedUserId { get; set; }
    public float? Similarity { get; set; }
}

/// <summary>
/// Kết quả xác thực khuôn mặt
/// </summary>
public class FaceVerifyResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsMatch { get; set; }
    public float Similarity { get; set; }
    public float Threshold { get; set; }
}

/// <summary>
/// Kết quả health check của Face Service
/// </summary>
public class FaceServiceHealthResult
{
    public string Status { get; set; } = string.Empty;
    public string Detector { get; set; } = string.Empty;
    public string Recognizer { get; set; } = string.Empty;
    public int RegisteredFaces { get; set; }
}
