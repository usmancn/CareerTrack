namespace CareerTrack.Models.Enums
{
    public enum ApplicationStatus
    {
        // --- Öğrenci başvurdu, İşveren süreci ---
        Pending          = 0,  // Öğrenci başvurdu, işveren henüz incelemedi
        PreScreening     = 1,  // İşveren: Ön Eleme aşamasına aldı
        AptitudeTest     = 2,  // İşveren: Genel Yetenek Testi
        LanguageTest     = 3,  // İşveren: İngilizce Sınavı
        Interview        = 4,  // İşveren: Mülakat

        // --- İşveren kararı ---
        EmployerAccepted = 5,  // İşveren kabul etti → Okul onayı bekleniyor

        // --- Okul Onay Süreci (işveren kabul sonrası) ---
        SchoolPending    = 6,  // Okul inceliyor
        SchoolRevision   = 7,  // Okul revize istedi (öğrenci düzeltmeli)
        SchoolApproved   = 8,  // Okul onayladı → Staj başlar, günlük yazılabilir

        // --- Sonuç ---
        Rejected         = 9,  // Reddedildi (işveren veya okul)
        Completed        = 10  // Staj tamamlandı, okul kapattı
    }
}
