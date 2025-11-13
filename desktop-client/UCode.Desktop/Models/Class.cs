using System;

namespace UCode.Desktop.Models
{
    public class Class
    {
        public string ClassId { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string TeacherId { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CoverImage { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
