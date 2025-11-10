using System;

namespace UCode.Desktop.Models
{
    public class Class
    {
        public string ClassId { get; set; }
        public string ClassName { get; set; }
        public string ClassCode { get; set; }
        public string TeacherId { get; set; }
        public string TeacherName { get; set; }
        public string Semester { get; set; }
        public string Description { get; set; }
        public string CoverImage { get; set; }
        public int StudentCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class Assignment
    {
        public string AssignmentId { get; set; }
        public AssignmentType AssignmentType { get; set; }
        public AssignmentStatus Status { get; set; }
        public string ClassId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string AssignedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
        public int? TotalPoints { get; set; }
        public int? TotalProblems { get; set; }
        public bool AllowLateSubmission { get; set; }
    }

    public enum AssignmentType
    {
        HOMEWORK,
        EXAM,
        PRACTICE,
        CONTEST
    }

    public enum AssignmentStatus
    {
        DRAFT,
        SCHEDULED,
        ACTIVE,
        ENDED,
        GRADED
    }
}
