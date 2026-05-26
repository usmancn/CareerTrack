using CareerTrack.Models.Entities;

namespace CareerTrack.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        // Kullanıcı istatistikleri
        public int TotalStudents { get; set; }
        public int TotalEmployers { get; set; }
        public int TotalSchools { get; set; }
        public int TotalUsers { get; set; }

        // Başvuru istatistikleri
        public int TotalApplications { get; set; }
        public int TotalOffered { get; set; }
        public int PendingApplications { get; set; }

        // Günlük istatistikleri
        public int PendingDailyLogs { get; set; }
        public int TotalDailyLogs { get; set; }

        // Şirket istatistikleri
        public int TotalCompanies { get; set; }
        public int PendingCompanies { get; set; }

        // İlan istatistikleri
        public int TotalJobPostings { get; set; }
        public int ActiveJobPostings { get; set; }

        // Son veriler
        public List<DailyLog> RecentDailyLogs { get; set; } = new();
        public List<JobApplication> RecentApplications { get; set; } = new();
        public List<Company> PendingCompanyList { get; set; } = new();
    }
}
