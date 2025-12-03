using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UCode.Desktop.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        
        [JsonProperty("errors")]
        public object ErrorsRaw { get; set; }
        
        [JsonIgnore]
        public List<string> Errors 
        { 
            get
            {
                if (ErrorsRaw == null) return null;
                
                try
                {
                    // Try to parse as JArray (JSON array)
                    if (ErrorsRaw is JArray jArray)
                    {
                        return jArray.ToObject<List<string>>();
                    }
                    
                    // Try to parse as JObject (JSON object/dictionary)
                    if (ErrorsRaw is JObject jObject)
                    {
                        var errors = new List<string>();
                        foreach (var prop in jObject.Properties())
                        {
                            if (prop.Value is JArray valueArray)
                            {
                                errors.AddRange(valueArray.Select(v => v.ToString()));
                            }
                            else
                            {
                                errors.Add(prop.Value.ToString());
                            }
                        }
                        return errors;
                    }
                    
                    // Fallback: convert to string
                    return new List<string> { ErrorsRaw.ToString() };
                }
                catch
                {
                    return new List<string> { ErrorsRaw.ToString() };
                }
            }
            set
            {
                ErrorsRaw = value;
            }
        }
        
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
    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Total { get => TotalCount; set => TotalCount = value; }
        public int PageNumber { get; set; }
        public int Page { get => PageNumber; set => PageNumber = value; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    public class ErrorResponse
    {
        public string Error { get; set; }
        public string Message { get; set; }
        public DateTime? Timestamp { get; set; }
        public string Path { get; set; }
        
        [JsonProperty("errors")]
        public object ErrorsRaw { get; set; }
        
        [JsonIgnore]
        public Dictionary<string, string[]> Errors 
        { 
            get
            {
                if (ErrorsRaw == null) return null;
                
                try
                {
                    if (ErrorsRaw is JObject jObject)
                    {
                        return jObject.ToObject<Dictionary<string, string[]>>();
                    }
                }
                catch { }
                
                return null;
            }
        }
        
        [JsonIgnore]
        public List<string> ErrorList
        {
            get
            {
                if (ErrorsRaw == null) return null;
                
                try
                {
                    // Try to parse as JArray
                    if (ErrorsRaw is JArray jArray)
                    {
                        return jArray.ToObject<List<string>>();
                    }
                    
                    // Try to parse as JObject and flatten
                    if (ErrorsRaw is JObject jObject)
                    {
                        var errors = new List<string>();
                        foreach (var prop in jObject.Properties())
                        {
                            if (prop.Value is JArray valueArray)
                            {
                                errors.AddRange(valueArray.Select(v => v.ToString()));
                            }
                            else
                            {
                                errors.Add(prop.Value.ToString());
                            }
                        }
                        return errors;
                    }
                }
                catch { }
                
                return null;
            }
        }
    }
}
