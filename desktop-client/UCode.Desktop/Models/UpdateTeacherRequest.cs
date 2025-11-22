using System;

namespace UCode.Desktop.Models
{
    public class UpdateTeacherRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Department { get; set; }
        public string Title { get; set; }
    }
}
