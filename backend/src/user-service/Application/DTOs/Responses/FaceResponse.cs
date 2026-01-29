/// <summary>
/// Response đăng ký khuôn mặt
/// </summary>
public class FaceRegisterResponse
{
    public string UserId { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime RegisteredAt { get; set; }
}

/// <summary>
/// Response xác thực khuôn mặt
/// </summary>
public class FaceVerifyResponse
{
    public string UserId { get; set; } = string.Empty;
    public bool IsMatch { get; set; }
    public float Similarity { get; set; }
    public float Threshold { get; set; }
    public DateTime VerifiedAt { get; set; }
}

