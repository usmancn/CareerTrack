using CareerTrack.Models.Entities;

namespace CareerTrack.Models.ViewModels
{
    /// <summary>
    /// Öğrenci başvuru ekranı — hem kendi başvuruları hem açık ilanlar.
    /// </summary>
    public class StudentApplicationIndexViewModel
    {
        /// <summary>Öğrencinin mevcut başvuruları.</summary>
        public List<JobApplication> MyApplications { get; set; } = new();

        /// <summary>Henüz başvurulmamış aktif ilanlar.</summary>
        public List<JobPosting> OpenPostings { get; set; } = new();

        /// <summary>Öğrencinin tamamlanmamış görev sayısı</summary>
        public int PendingTodosCount { get; set; }
    }
}
