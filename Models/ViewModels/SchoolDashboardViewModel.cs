using CareerTrack.Models.Entities;

namespace CareerTrack.Models.ViewModels
{
    public class SchoolDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalApplications { get; set; }
        public int PendingDailyLogs { get; set; }
        public int TotalDailyLogs { get; set; }

        public List<DailyLog> RecentDailyLogs { get; set; } = new();
        public List<JobApplication> RecentApplications { get; set; } = new();

        /// <summary>
        /// Öğrenci listesi — belirli bir öğrenciyi seçerek detay görme.
        /// </summary>
        public List<StudentListItemViewModel> Students { get; set; } = new();
    }

    public class StudentListItemViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
