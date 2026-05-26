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

        // LINQ GroupBy Stats
        public Dictionary<string, int> ApplicationStatusStats { get; set; } = new();
    }
}
