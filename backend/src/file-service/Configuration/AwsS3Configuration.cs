namespace file_service.Configuration;

public class AwsS3Configuration
{
    public string Profile { get; set; } = "default";
    public string Region { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
}
