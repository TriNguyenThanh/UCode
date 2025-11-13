using System;
using System.Collections.Generic;

namespace UCode.Desktop.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; }
        public DateTime? Timestamp { get; set; }
    }

    public class PagedResponse<T>
    {
        public List<T> Items { get; set; }
        
        public int TotalCount { get; set; }
        
        public int Page { get; set; }
        
        public int PageSize { get; set; }
        
        public int TotalPages { get; set; }
        
        public bool HasPrevious { get; set; }
        
        public bool HasNext { get; set; }
    }

    public class ErrorResponse
    {
        public string Error { get; set; }
        public string Message { get; set; }
        public DateTime? Timestamp { get; set; }
        public string Path { get; set; }
        public Dictionary<string, string[]> Errors { get; set; }
    }
}
