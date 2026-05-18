using CareerTrack.Models.Entities;

namespace CareerTrack.Models.ViewModels
{
    /// <summary>
    /// Öğrenci Dashboard ana sayfası için ViewModel.
    /// Tek sınıfta 3 farklı tablodan gelen veri taşınır.
    /// </summary>
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public string? Department { get; set; }

        // Ana veriler
        public List<JobApplication> ActiveApplications { get; set; } = new();
        public List<Interview> UpcomingInterviews { get; set; } = new();
        public List<ToDo> IncompleteToDos { get; set; } = new();

        // İstatistikler
        public int TotalApplications { get; set; }
        public int OfferedCount { get; set; }
        public int RejectedCount { get; set; }
        public int InReviewCount { get; set; }
    }
}
