using CareerTrack.Models.Entities;

namespace CareerTrack.Models.ViewModels
{
    public class EmployerDashboardViewModel
    {
        public string CompanyName { get; set; } = string.Empty;
        public int TotalPostings { get; set; }
        public int ActivePostings { get; set; }
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }

        public List<JobPosting> RecentPostings { get; set; } = new();
        public List<JobApplication> RecentApplications { get; set; } = new();
    }
}
