using System;
using Newtonsoft.Json;

namespace UCode.Desktop.Models
{
    public class Class
    {
        [JsonProperty("classId")]
        public string ClassId { get; set; } = string.Empty;
        
        [JsonProperty("name")]
        public string ClassName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("classCode")]
        public string ClassCode { get; set; } = string.Empty;
        
        [JsonProperty("teacherId")]
        public string TeacherId { get; set; } = string.Empty;
        
        [JsonProperty("teacherName")]
        public string TeacherName { get; set; } = string.Empty;
        
        [JsonProperty("semester")]
        public string Semester { get; set; } = string.Empty;
        
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;
        
        [JsonProperty("coverImage")]
        public string CoverImage { get; set; } = string.Empty;
        
        [JsonProperty("studentCount")]
        public int StudentCount { get; set; }
        
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
        
        [JsonProperty("updatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }
}
