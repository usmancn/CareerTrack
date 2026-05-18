using CareerTrack.Models.Entities;

namespace CareerTrack.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalApplications { get; set; }
        public int TotalOffered { get; set; }
        public int PendingDailyLogs { get; set; }
        public int TotalDailyLogs { get; set; }

        public List<DailyLog> RecentDailyLogs { get; set; } = new();
        public List<JobApplication> RecentApplications { get; set; } = new();
    }
}
