using System;

namespace UCode.Desktop.Models.Admin
{
    public class AdminDashboardStats
    {
        public int TotalUsers { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalClasses { get; set; }
        public int TotalProblems { get; set; }
        public int TotalAssignments { get; set; }
        public int TotalSubmissions { get; set; }
        public int ActiveUsersToday { get; set; }
        public int ActiveUsersThisWeek { get; set; }
        public int ActiveUsersThisMonth { get; set; }
        public int SubmissionsToday { get; set; }
        public int SubmissionsThisWeek { get; set; }
        public int SubmissionsThisMonth { get; set; }
        public decimal AverageSuccessRate { get; set; }
        public DateTime LastUpdated { get; set; }

        // Growth metrics
        public int NewUsersThisWeek { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int NewClassesThisWeek { get; set; }
        public int NewClassesThisMonth { get; set; }
        public int NewProblemsThisWeek { get; set; }
        public int NewProblemsThisMonth { get; set; }

        // Percentage changes
        public double UserGrowthPercentage { get; set; }
        public double SubmissionGrowthPercentage { get; set; }
        public double ClassGrowthPercentage { get; set; }
    }
}
