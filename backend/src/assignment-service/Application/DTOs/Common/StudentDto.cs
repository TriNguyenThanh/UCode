namespace AssignmentService.Application.DTOs.Common
{
    public class StudentDto {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public int EnrollmentYear { get; set; } = 0;
        public string Major { get; set; } = string.Empty;
        public int ClassYear { get; set; }
    }
}