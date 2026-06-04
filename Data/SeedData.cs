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
            // ── 1) Rolleri oluştur ─────────────────────────────────
            foreach (var role in AppRoles.All)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            // ── 2) Admin ──────────────────────────────────────────
            var adminEmail = "admin@careertrack.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail, Email = adminEmail,
                    FullName = "Sistem Koordinatörü",
                    Department = "Akademik Koordinatörlük", EmailConfirmed = true
                };
                var r = await userManager.CreateAsync(admin, "Admin123!");
                if (r.Succeeded) await userManager.AddToRoleAsync(admin, AppRoles.Admin);
            }

            // ── 3) Okul ───────────────────────────────────────────
            var schoolEmail = "okul@careertrack.com";
            if (await userManager.FindByEmailAsync(schoolEmail) == null)
            {
                var school = new ApplicationUser
                {
                    UserName = schoolEmail, Email = schoolEmail,
                    FullName = "Dr. Yılmaz Kaya",
                    Department = "Bilgisayar Mühendisliği", EmailConfirmed = true
                };
                var r = await userManager.CreateAsync(school, "Okul123!");
                if (r.Succeeded) await userManager.AddToRoleAsync(school, AppRoles.School);
            }

            // ── 4) Şirketler (yoksa oluştur) ──────────────────────
            List<Company> companies;
            if (!await context.Companies.AnyAsync())
            {
                companies = new List<Company>
                {
                    new() { Name = "TechCorp Yazılım A.Ş.",   Sector = "Bilgi Teknolojileri", Location = "İstanbul", IsApproved = true  },
                    new() { Name = "Global Finans Bank",        Sector = "Bankacılık",          Location = "Ankara",   IsApproved = true  },
                    new() { Name = "DataMind Yapay Zeka Ltd.", Sector = "Yapay Zeka",           Location = "İzmir",    IsApproved = true  },
                    new() { Name = "Nova E-Ticaret A.Ş.",      Sector = "E-Ticaret",            Location = "Bursa",    IsApproved = false },
                };
                context.Companies.AddRange(companies);
                await context.SaveChangesAsync();
            }
            else
            {
                companies = await context.Companies.OrderBy(c => c.Id).ToListAsync();
            }

            // ── 5) İşverenler (yoksa oluştur) ─────────────────────
            async Task<ApplicationUser> GetOrCreateEmployer(string email, string name, string dept, Company company)
            {
                var u = await userManager.FindByEmailAsync(email);
                if (u == null)
                {
                    u = new ApplicationUser { UserName = email, Email = email, FullName = name, Department = dept, CompanyId = company.Id, EmailConfirmed = true };
                    var r = await userManager.CreateAsync(u, "Isveren123!");
                    if (r.Succeeded) await userManager.AddToRoleAsync(u, AppRoles.Employer);
                }
                return u;
            }

            var e1 = await GetOrCreateEmployer("isveren@careertrack.com",  "Ayşe Kaya",   "İnsan Kaynakları", companies[0]);
            var e2 = await GetOrCreateEmployer("isveren2@careertrack.com", "Burak Demir", "İşe Alım",         companies[1]);
            var e3 = await GetOrCreateEmployer("isveren3@careertrack.com", "Cemil Öz",   "Teknik Lider",     companies[2]);

            // ── 6) Öğrenciler (yoksa oluştur) ─────────────────────
            async Task<ApplicationUser> GetOrCreateStudent(string email, string name)
            {
                var u = await userManager.FindByEmailAsync(email);
                if (u == null)
                {
                    u = new ApplicationUser { UserName = email, Email = email, FullName = name, Department = "Bilgisayar Mühendisliği", EmailConfirmed = true };
                    var r = await userManager.CreateAsync(u, "Ogrenci123!");
                    if (r.Succeeded) await userManager.AddToRoleAsync(u, AppRoles.Student);
                }
                return u;
            }

            var s1 = await GetOrCreateStudent("ogrenci@careertrack.com",  "Mehmet Yıldız");
            var s2 = await GetOrCreateStudent("zeynep@careertrack.com",   "Zeynep Arslan");
            var s3 = await GetOrCreateStudent("can@careertrack.com",      "Can Erdoğan");
            var s4 = await GetOrCreateStudent("fatma@careertrack.com",    "Fatma Şahin");
            var s5 = await GetOrCreateStudent("ali@careertrack.com",      "Ali Çelik");
            var s6 = await GetOrCreateStudent("selin@careertrack.com",    "Selin Demir");

            // ── 7) Başvurular (yoksa oluştur) ─────────────────────
            if (!await context.JobApplications.AnyAsync())
            {
                var apps = new List<JobApplication>
                {
                    // 1) Okul Onayı Bekliyor
                    new() { StudentId = s1.Id, CompanyId = companies[0].Id, Position = "Backend Stajyeri",       ApplicationDate = DateTime.Today.AddDays(-1),  Status = ApplicationStatus.SchoolPending,   InternshipType = InternshipType.MandatoryShort },
                    // 2) Okul Revize İstedi
                    new() { StudentId = s2.Id, CompanyId = companies[1].Id, Position = "Veri Analisti",           ApplicationDate = DateTime.Today.AddDays(-3),  Status = ApplicationStatus.SchoolRevision,  SchoolNote = "Başvuru formundaki tarihler eksik, lütfen düzeltin." },
                    // 3) Okul Onayladı → İşveren Ön Elemede
                    new() { StudentId = s3.Id, CompanyId = companies[0].Id, Position = "Full-Stack Geliştirici",  ApplicationDate = DateTime.Today.AddDays(-5),  Status = ApplicationStatus.PreScreening,    SchoolNote = "Uygun görülmüştür." },
                    // 4) Yetenek Testi
                    new() { StudentId = s4.Id, CompanyId = companies[0].Id, Position = "Mobil Geliştirici",       ApplicationDate = DateTime.Today.AddDays(-8),  Status = ApplicationStatus.AptitudeTest,    SchoolNote = "Onaylandı." },
                    // 5) Mülakat Aşaması
                    new() { StudentId = s5.Id, CompanyId = companies[2].Id, Position = "ML Mühendisi",            ApplicationDate = DateTime.Today.AddDays(-12), Status = ApplicationStatus.Interview,       SchoolNote = "Onaylandı.", EmployerNote = "Teknik mülakata davet edildi." },
                    // 6) Kabul + Aktif Staj (günlük var)
                    new() { StudentId = s6.Id, CompanyId = companies[2].Id, Position = "Backend Geliştirici",     ApplicationDate = DateTime.Today.AddDays(-20), Status = ApplicationStatus.SchoolApproved,  SchoolNote = "Onaylandı.", EmployerNote = "Memnunuz.", InternshipStartDate = DateTime.Today.AddDays(-15), InternshipType = InternshipType.LongTerm, TotalInternshipDays = 60 },
                    // 7) Tamamlandı
                    new() { StudentId = s1.Id, CompanyId = companies[1].Id, Position = "DevOps Stajyeri",         ApplicationDate = DateTime.Today.AddDays(-90), Status = ApplicationStatus.Completed,       SchoolNote = "Staj başarıyla tamamlanmıştır.", EmployerNote = "Mükemmel performans.", InternshipStartDate = DateTime.Today.AddDays(-75), InternshipEndDate = DateTime.Today.AddDays(-15), TotalInternshipDays = 60 },
                    // 8) Reddedildi
                    new() { StudentId = s2.Id, CompanyId = companies[2].Id, Position = "Veri Bilimci",            ApplicationDate = DateTime.Today.AddDays(-25), Status = ApplicationStatus.Rejected,        SchoolNote = "Onaylanmıştı.", EmployerNote = "Pozisyon dolduruldu." },
                };
                context.JobApplications.AddRange(apps);
                await context.SaveChangesAsync();

                // ── 8) Günlük Loglar ──────────────────────────────
                var logs = new List<DailyLog>
                {
                    new() { StudentId = s6.Id, JobApplicationId = apps[5].Id, DayNumber = 1,  LogDate = DateTime.Today.AddDays(-14), Content = "İlk gün şirkete giriş yapıldı. Bilgisayar ve geliştirme ortamı kuruldu. Ekip üyeleriyle tanışma toplantısı gerçekleştirildi. Proje takvimi ve beklentiler konuşuldu.", Status = DailyLogStatus.SchoolApproved, IsEmployerApproved = true, IsSchoolApproved = true, EmployerNote = "Güzel başlangıç!", SchoolNote = "Yeterli." },
                    new() { StudentId = s6.Id, JobApplicationId = apps[5].Id, DayNumber = 2,  LogDate = DateTime.Today.AddDays(-13), Content = "Şirketin kullandığı PostgreSQL veritabanı incelendi. Entity-Relation diyagramı çizildi. Mevcut endpointler Swagger üzerinden test edildi. Akşam ekip ile code review yapıldı.", Status = DailyLogStatus.EmployerApproved, IsEmployerApproved = true, EmployerNote = "Verimli çalışma." },
                    new() { StudentId = s6.Id, JobApplicationId = apps[5].Id, DayNumber = 3,  LogDate = DateTime.Today.AddDays(-12), Content = "İlk feature branch oluşturuldu. JWT token entegrasyonu üzerine çalışıldı. Birim testler yazıldı ve CI/CD pipeline başarıyla geçti.", Status = DailyLogStatus.SentToEmployer },
                    new() { StudentId = s6.Id, JobApplicationId = apps[5].Id, DayNumber = 4,  LogDate = DateTime.Today.AddDays(-11), Content = "API dokümantasyonu Swagger üzerinden güncellendi. 2 endpoint için hata düzeltmesi yapıldı. Sprint review toplantısına katıldım.", Status = DailyLogStatus.SentToEmployer },
                    new() { StudentId = s1.Id, JobApplicationId = apps[6].Id, DayNumber = 20, LogDate = DateTime.Today.AddDays(-60), Content = "Projenin production ortamına deploy süreci yönetildi. Blue-green deployment stratejisi kullanılarak sıfır downtime ile geçiş sağlandı. Sprint retrospective toplantısına katıldım.", Status = DailyLogStatus.SchoolApproved, IsEmployerApproved = true, IsSchoolApproved = true, EmployerNote = "Mükemmel iş!", SchoolNote = "Staj defteri eksiksiz." },
                };
                context.DailyLogs.AddRange(logs);

                // ── 9) To-Do'lar ──────────────────────────────────
                var todos = new List<ToDo>
                {
                    new() { StudentId = s6.Id, TaskTitle = "Proje Mimarisi Diyagramı Çiz",   IsCompleted = false, DueDate = DateTime.Today.AddDays(2),  CreatedAt = DateTime.Today.AddDays(-2), JobApplicationId = apps[5].Id },
                    new() { StudentId = s6.Id, TaskTitle = "Docker Compose Kurulumu Tamamla", IsCompleted = true,  DueDate = DateTime.Today.AddDays(-1), CreatedAt = DateTime.Today.AddDays(-3), JobApplicationId = apps[5].Id },
                    new() { StudentId = s6.Id, TaskTitle = "Unit Test Coverage %80'e Çıkar",  IsCompleted = false, DueDate = DateTime.Today.AddDays(5),  CreatedAt = DateTime.Today.AddDays(-1), JobApplicationId = apps[5].Id },
                    new() { StudentId = s1.Id, TaskTitle = "Staj Defterini Sisteme Yükle",    IsCompleted = false, DueDate = DateTime.Today.AddDays(1),  CreatedAt = DateTime.Today.AddDays(-1) },
                };
                context.ToDos.AddRange(todos);

                // ── 10) İşveren Görevleri ─────────────────────────
                var tasks = new List<StudentTask>
                {
                    new() { JobApplicationId = apps[5].Id, AssignedByEmployerId = e3.Id, Title = "API Dokümantasyonu Hazırla",  Description = "Swagger UI kullanarak tüm endpointleri Türkçe ve İngilizce olarak belgele.",    DueDate = DateTime.Today.AddDays(3), IsCompleted = false, CreatedAt = DateTime.Today.AddDays(-1) },
                    new() { JobApplicationId = apps[5].Id, AssignedByEmployerId = e3.Id, Title = "Kod İncelemesi Yap",           Description = "Authentication servisindeki 3 pull requesti incele ve geri bildirim ver.",       DueDate = DateTime.Today.AddDays(1), IsCompleted = true,  CompletedAt = DateTime.Today, CreatedAt = DateTime.Today.AddDays(-2) },
                };
                context.StudentTasks.AddRange(tasks);

                await context.SaveChangesAsync();
            }
        }
    }
}
