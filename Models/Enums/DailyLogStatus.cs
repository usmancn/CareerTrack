namespace CareerTrack.Models.Enums
{
    public enum DailyLogStatus
    {
        Draft            = 0,  // Taslak — henüz gönderilmedi
        SentToEmployer   = 1,  // İşverene gönderildi, onay bekleniyor
        EmployerApproved = 2,  // İşveren onayladı
        EmployerRejected = 3,  // İşveren reddetti / revize istedi
        SchoolApproved   = 4,  // Okul da onayladı — staj sayılır
        SchoolRejected   = 5   // Okul reddetti / revize istedi
    }
}
