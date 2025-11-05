namespace file_service.Models;

public class FileInfo
{
    public string FileName { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public string Url { get; set; } = string.Empty;
}
