using CareerTrack.Models.Entities;

namespace CareerTrack.Models.ViewModels
{
    /// <summary>
    /// Okul panelinde belirli bir öğrencinin detaylı bilgilerini göstermek için ViewModel.
    /// </summary>
    public class SchoolStudentDetailViewModel
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string Email { get; set; } = string.Empty;

        // İstatistikler
        public int TotalApplications { get; set; }
        public int OfferedCount { get; set; }
        public int RejectedCount { get; set; }
        public int TotalDailyLogs { get; set; }
        public int ApprovedDailyLogs { get; set; }

        // Detay listeleri
        public List<JobApplication> Applications { get; set; } = new();
        public List<DailyLog> DailyLogs { get; set; } = new();
    }
}
