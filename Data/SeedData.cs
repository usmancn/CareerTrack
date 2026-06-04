using CareerTrack.Models.Constants;
using CareerTrack.Models.Entities;
using CareerTrack.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Data
{
    public static class SeedData
    {
        public static async Task Initialize(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            // ── Rolleri oluştur ─────────────────────────────────
            foreach (var role in AppRoles.All)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // ── Admin ────────────────────────────────────────────
            var adminEmail = "admin@careertrack.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Prof. Dr. Ahmet Çelik",
                    Department = "Staj Koordinatörlüğü",
                    EmailConfirmed = true
                };
                var r = await userManager.CreateAsync(admin, "Admin123!");
                if (r.Succeeded) await userManager.AddToRoleAsync(admin, AppRoles.Admin);
            }

            // ── Okul ─────────────────────────────────────────────
            var schoolEmail = "okul@careertrack.com";
            if (await userManager.FindByEmailAsync(schoolEmail) == null)
            {
                var school = new ApplicationUser
                {
                    UserName = schoolEmail,
                    Email = schoolEmail,
                    FullName = "Dr. Elif Kaya",
                    Department = "Bilgisayar Mühendisliği",
                    EmailConfirmed = true
                };
                var r = await userManager.CreateAsync(school, "Okul123!");
                if (r.Succeeded) await userManager.AddToRoleAsync(school, AppRoles.School);
            }

            // ── Sunum verisi yoksa ekle ───────────────────────────
            if (await context.Companies.AnyAsync()) return; // Zaten eklenmiş, tekrar ekleme

            // ── Şirketler ─────────────────────────────────────────
            var companies = new List<Company>
            {
                new() { Name = "TechCorp Yazılım A.Ş.",     Sector = "Bilgi Teknolojileri", Location = "İstanbul", IsApproved = true  },
                new() { Name = "Global Finans Bank",          Sector = "Bankacılık",          Location = "Ankara",   IsApproved = true  },
                new() { Name = "DataMind Yapay Zeka Ltd.",   Sector = "Yapay Zeka",           Location = "İzmir",    IsApproved = true  },
                new() { Name = "Nova E-Ticaret A.Ş.",        Sector = "E-Ticaret",            Location = "Bursa",    IsApproved = false }, // onay bekliyor
            };
            context.Companies.AddRange(companies);
            await context.SaveChangesAsync();

            // ── İşverenler ────────────────────────────────────────
            var e1 = new ApplicationUser { UserName = "isveren@careertrack.com",  Email = "isveren@careertrack.com",  FullName = "Ayşe Kaya",  Department = "İnsan Kaynakları", CompanyId = companies[0].Id, EmailConfirmed = true };
            var e2 = new ApplicationUser { UserName = "isveren2@careertrack.com", Email = "isveren2@careertrack.com", FullName = "Burak Demir", Department = "İşe Alım",        CompanyId = companies[1].Id, EmailConfirmed = true };
            var e3 = new ApplicationUser { UserName = "isveren3@careertrack.com", Email = "isveren3@careertrack.com", FullName = "Cemil Öz",   Department = "Teknik Lider",     CompanyId = companies[2].Id, EmailConfirmed = true };
            foreach (var e in new[] { e1, e2, e3 })
            {
                await userManager.CreateAsync(e, "Isveren123!");
                await userManager.AddToRoleAsync(e, AppRoles.Employer);
            }

            // ── İlanlar ───────────────────────────────────────────
            var postings = new List<JobPosting>
            {
                new() { CompanyId = companies[0].Id, EmployerId = e1.Id, Title = ".NET Backend Stajyeri",     Description = "Microservice mimarisi ile .NET ortamında çalışacak, C# ve Entity Framework tecrübesi olan stajyer aranmaktadır.",  Quota = 2, StartDate = DateTime.Today.AddDays(10), EndDate = DateTime.Today.AddDays(40), IsActive = true, CreatedAt = DateTime.Today.AddDays(-5) },
                new() { CompanyId = companies[1].Id, EmployerId = e2.Id, Title = "Veri Bilimi Stajyeri",      Description = "Python ve SQL kullanarak büyük veri setlerinde analiz yapacak stajyer aranıyor. Pandas ve NumPy bilgisi tercih sebebidir.", Quota = 1, StartDate = DateTime.Today.AddDays(15), EndDate = DateTime.Today.AddDays(45), IsActive = true, CreatedAt = DateTime.Today.AddDays(-2) },
                new() { CompanyId = companies[2].Id, EmployerId = e3.Id, Title = "Yapay Zeka Araştırmacısı", Description = "LLM modelleri üzerine araştırma yapacak, Python ve PyTorch bilen stajyer aranıyor.",                                   Quota = 1, StartDate = DateTime.Today.AddDays(5),  EndDate = DateTime.Today.AddDays(35), IsActive = true, CreatedAt = DateTime.Today.AddDays(-8) },
            };
            context.JobPostings.AddRange(postings);
            await context.SaveChangesAsync();

            // ── Öğrenciler ────────────────────────────────────────
            var s1 = new ApplicationUser { UserName = "ogrenci@careertrack.com",  Email = "ogrenci@careertrack.com",  FullName = "Mehmet Yıldız",  Department = "Bilgisayar Mühendisliği", EmailConfirmed = true };
            var s2 = new ApplicationUser { UserName = "zeynep@careertrack.com",   Email = "zeynep@careertrack.com",   FullName = "Zeynep Demir",   Department = "Yazılım Mühendisliği",    EmailConfirmed = true };
            var s3 = new ApplicationUser { UserName = "can@careertrack.com",      Email = "can@careertrack.com",      FullName = "Can Özkan",      Department = "Bilişim Sistemleri",      EmailConfirmed = true };
            var s4 = new ApplicationUser { UserName = "fatma@careertrack.com",    Email = "fatma@careertrack.com",    FullName = "Fatma Şahin",    Department = "Bilgisayar Mühendisliği", EmailConfirmed = true };
            var s5 = new ApplicationUser { UserName = "ali@careertrack.com",      Email = "ali@careertrack.com",      FullName = "Ali Vefa",       Department = "Yazılım Mühendisliği",    EmailConfirmed = true };
            var s6 = new ApplicationUser { UserName = "selin@careertrack.com",    Email = "selin@careertrack.com",    FullName = "Selin Arslan",   Department = "Bilgisayar Mühendisliği", EmailConfirmed = true };
            foreach (var s in new[] { s1, s2, s3, s4, s5, s6 })
            {
                await userManager.CreateAsync(s, "Ogrenci123!");
                await userManager.AddToRoleAsync(s, AppRoles.Student);
            }

            // ── Başvurular — Her Senaryo ──────────────────────────
            var apps = new List<JobApplication>
            {
                // 1) Okul Onayı Bekliyor
                new() { StudentId = s1.Id, CompanyId = companies[0].Id, Position = "Backend Stajyeri",      ApplicationDate = DateTime.Today.AddDays(-1),  Status = ApplicationStatus.SchoolPending,  InternshipType = InternshipType.MandatoryShort },
                // 2) Okul Revize İstedi
                new() { StudentId = s2.Id, CompanyId = companies[1].Id, Position = "Veri Analisti",         ApplicationDate = DateTime.Today.AddDays(-3),  Status = ApplicationStatus.SchoolRevision, SchoolNote = "Başvuru formundaki tarihler eksik, lütfen düzeltin." },
                // 3) Okul Onayladı → İşveren Ön Elemede
                new() { StudentId = s3.Id, CompanyId = companies[0].Id, Position = "Full-Stack Geliştirici",ApplicationDate = DateTime.Today.AddDays(-5),  Status = ApplicationStatus.PreScreening,  SchoolNote = "Uygun görülmüştür." },
                // 4) Yetenek Testi
                new() { StudentId = s4.Id, CompanyId = companies[0].Id, Position = "Mobil Geliştirici",     ApplicationDate = DateTime.Today.AddDays(-8),  Status = ApplicationStatus.AptitudeTest,  SchoolNote = "Onaylandı." },
                // 5) Mülakat Aşamasında
                new() { StudentId = s5.Id, CompanyId = companies[2].Id, Position = "AI Researcher",         ApplicationDate = DateTime.Today.AddDays(-12), Status = ApplicationStatus.Interview,     SchoolNote = "Onaylandı.", EmployerNote = "Mülakat Cuma 14:00'de, Teams üzerinden." },
                // 6) İşveren Kabul Etti → Staj Devam Ediyor
                new() { StudentId = s6.Id, CompanyId = companies[2].Id, Position = "ML Mühendisi",          ApplicationDate = DateTime.Today.AddDays(-20), Status = ApplicationStatus.EmployerAccepted, SchoolNote = "Onaylandı.", EmployerNote = "Ekibe hoş geldin!", InternshipStartDate = DateTime.Today.AddDays(-3), InternshipEndDate = DateTime.Today.AddDays(27), TotalInternshipDays = 30 },
                // 7) Tamamlandı (geçmiş staj)
                new() { StudentId = s1.Id, CompanyId = companies[1].Id, Position = "Sistem Destek",          ApplicationDate = DateTime.Today.AddDays(-90), Status = ApplicationStatus.Completed,    SchoolNote = "Staj defteri onaylandı.", EmployerNote = "Çok başarılı bir stajyerdi.", InternshipStartDate = DateTime.Today.AddDays(-80), InternshipEndDate = DateTime.Today.AddDays(-50), TotalInternshipDays = 30 },
                // 8) Reddedildi
                new() { StudentId = s2.Id, CompanyId = companies[2].Id, Position = "Backend Stajyeri",       ApplicationDate = DateTime.Today.AddDays(-7),  Status = ApplicationStatus.Rejected,      SchoolNote = "Uygun görülmüştür.", EmployerNote = "Teknik değerlendirmede yeterli puan alınamadı." },
            };
            context.JobApplications.AddRange(apps);
            await context.SaveChangesAsync();

            // ── Staj Günlükleri ───────────────────────────────────
            var logs = new List<DailyLog>
            {
                // Günlük 1 — Kabul edilen stajyer, okul onaylı
                new() { StudentId = s6.Id, JobApplicationId = apps[5].Id, DayNumber = 1, LogDate = DateTime.Today.AddDays(-3), Content = "İlk gün şirkete giriş yapıldı. Bilgisayar ve geliştirme ortamı kuruldu. Ekip üyeleriyle tanışma toplantısı gerçekleştirildi. Proje takvimi ve beklentiler konuşuldu.", Status = DailyLogStatus.SchoolApproved, IsEmployerApproved = true, EmployerNote = "Güzel başlangıç!", IsSchoolApproved = true, SchoolNote = "Yeterli." },
                // Günlük 2 — İşveren onaylı, okul bekliyor
                new() { StudentId = s6.Id, JobApplicationId = apps[5].Id, DayNumber = 2, LogDate = DateTime.Today.AddDays(-2), Content = "Şirketin kullandığı PostgreSQL veritabanı incelendi. Entity-Relation diyagramı çizildi. Projenin mevcut endpointleri Swagger üzerinden test edildi. Akşam ekip ile code review yapıldı.", Status = DailyLogStatus.EmployerApproved, IsEmployerApproved = true, EmployerNote = "Verimli çalışma." },
                // Günlük 3 — İşverene gönderildi, bekleniyor
                new() { StudentId = s6.Id, JobApplicationId = apps[5].Id, DayNumber = 3, LogDate = DateTime.Today.AddDays(-1), Content = "İlk feature branch oluşturuldu. Kullanıcı kayıt servisinde JWT token entegrasyonu üzerine çalışıldı. Birim testler yazıldı ve pipeline'da tüm testler başarıyla geçti.", Status = DailyLogStatus.SentToEmployer },
                // Geçmiş staj — tamamlanmış günlük
                new() { StudentId = s1.Id, JobApplicationId = apps[6].Id, DayNumber = 20, LogDate = DateTime.Today.AddDays(-60), Content = "Projenin production ortamına deploy süreci yönetildi. Blue-green deployment stratejisi kullanılarak sıfır downtime ile geçiş sağlandı. Tüm hata logları JIRA'ya aktarıldı ve sprint retrospective toplantısına katılındı.", Status = DailyLogStatus.SchoolApproved, IsEmployerApproved = true, IsSchoolApproved = true, EmployerNote = "Mükemmel iş!", SchoolNote = "Staj defteri eksiksiz ve yeterliliği kanıtlanmıştır." },
            };
            context.DailyLogs.AddRange(logs);

            // ── To-Do'lar ─────────────────────────────────────────
            var todos = new List<ToDo>
            {
                new() { StudentId = s6.Id, TaskTitle = "Proje Mimarisi Diyagramı Çiz",   IsCompleted = false, DueDate = DateTime.Today.AddDays(2),  CreatedAt = DateTime.Today.AddDays(-2),  JobApplicationId = apps[5].Id },
                new() { StudentId = s6.Id, TaskTitle = "Docker Compose Kurulumu Tamamla", IsCompleted = true,  DueDate = DateTime.Today.AddDays(-1), CreatedAt = DateTime.Today.AddDays(-3),  JobApplicationId = apps[5].Id },
                new() { StudentId = s6.Id, TaskTitle = "Unit Test Coverage %80'e Çıkar",  IsCompleted = false, DueDate = DateTime.Today.AddDays(5),  CreatedAt = DateTime.Today.AddDays(-1),  JobApplicationId = apps[5].Id },
                new() { StudentId = s1.Id, TaskTitle = "Staj Defterini Sisteme Yükle",    IsCompleted = false, DueDate = DateTime.Today.AddDays(1),  CreatedAt = DateTime.Today.AddDays(-1) },
            };
            context.ToDos.AddRange(todos);

            // ── İşveren Görevleri ─────────────────────────────────
            var tasks = new List<StudentTask>
            {
                new() { JobApplicationId = apps[5].Id, AssignedByEmployerId = e3.Id, Title = "API Dokümantasyonu Hazırla", Description = "Swagger UI kullanarak tüm endpoint'leri Türkçe ve İngilizce olarak belgele.", DueDate = DateTime.Today.AddDays(3), IsCompleted = false, CreatedAt = DateTime.Today.AddDays(-1) },
                new() { JobApplicationId = apps[5].Id, AssignedByEmployerId = e3.Id, Title = "Kod İncelemesi Yap",          Description = "Authentication servisindeki 3 pull request'i incele ve geri bildirim ver.",    DueDate = DateTime.Today.AddDays(1), IsCompleted = true,  CompletedAt = DateTime.Today, CreatedAt = DateTime.Today.AddDays(-2) },
            };
            context.StudentTasks.AddRange(tasks);

            await context.SaveChangesAsync();
        }
    }
}
