using System;

namespace UCode.Desktop.Models
{
    public class PracticeCategory
    {
        public string CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public int ProblemCount { get; set; }
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
