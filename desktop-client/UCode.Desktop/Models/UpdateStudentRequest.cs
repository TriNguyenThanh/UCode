using System;

namespace UCode.Desktop.Models
{
    public class UpdateStudentRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Major { get; set; }
        public int? ClassYear { get; set; }
    }
}
