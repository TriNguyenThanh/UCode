namespace AssignmentService.Application.DTOs.Responses;

/// <summary>
/// Generic paged response DTO
/// </summary>
/// <typeparam name="T">Type of data in the response</typeparam>
public class PagedResponse<T>
{
    /// <summary>
    /// The actual data items
    /// </summary>
    public List<T> Data { get; set; } = new();
    
    /// <summary>
    /// Current page number
    /// </summary>
    public int Page { get; set; }
    
    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Total number of items
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }
    
    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPrevious => Page > 1;
    
    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNext => Page < TotalPages;
}
