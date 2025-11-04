namespace file_service.Models;

public class PresignedUrlRequest
{
    public string Key { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}
