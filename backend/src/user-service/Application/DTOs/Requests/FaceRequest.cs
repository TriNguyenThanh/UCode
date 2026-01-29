#region Request/Response DTOs

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request đăng ký khuôn mặt
/// </summary>
public class FaceRegisterRequest
{
    /// <summary>
    /// Ảnh khuôn mặt dạng base64 (có thể có hoặc không có data:image prefix)
    /// </summary>
    [Required]
    public string Image { get; set; } = string.Empty;
}

/// <summary>
/// Request xác thực khuôn mặt
/// </summary>
public class FaceVerifyRequest
{
    /// <summary>
    /// Ảnh khuôn mặt dạng base64
    /// </summary>
    [Required]
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Ngưỡng similarity (0.0 - 1.0), default 0.4
    /// </summary>
    public float? Threshold { get; set; }
}

#endregion
