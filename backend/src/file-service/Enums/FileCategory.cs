namespace file_service.Enums;

/// <summary>
/// Loại file được phép upload
/// </summary>
public enum FileCategory
{
    /// <summary>
    /// Tài liệu bài tập (PDF, DOCX, TXT)
    /// </summary>
    AssignmentDocument, //= 1,
    
    /// <summary>
    /// Code submission (ZIP, RAR, C, CPP, JAVA, PY, JS, etc.)
    /// </summary>
    CodeSubmission, //= 2,
    
    /// <summary>
    /// Hình ảnh (JPG, PNG, GIF, SVG)
    /// </summary>
    Image, //= 3,
    
    /// <summary>
    /// Avatar người dùng (JPG, PNG)
    /// </summary>
    Avatar, //= 4,
    
    /// <summary>
    /// Test case files (TXT, IN, OUT)
    /// </summary>
    TestCase, //= 5,
    
    /// <summary>
    /// Tài liệu tham khảo (PDF, DOCX, PPTX)
    /// </summary>
    Reference, //= 6,
    
    /// <summary>
    /// Tài liệu chung - PDF, Word, Excel, PowerPoint
    /// </summary>
    Document, //= 7
}
