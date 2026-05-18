using CareerTrack.Models.Constants;
using CareerTrack.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace CareerTrack.Data
{
    public static class SeedData
    {
        public static async Task Initialize(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            // Rolleri oluştur
            foreach (var role in AppRoles.All)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Varsayılan Admin kullanıcısı
            var adminEmail = "admin@careertrack.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Sistem Koordinatörü",
                    Department = "Akademik Koordinatörlük",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, AppRoles.Admin);
            }

            // Varsayılan Okul kullanıcısı
            var schoolEmail = "okul@careertrack.com";
            if (await userManager.FindByEmailAsync(schoolEmail) == null)
            {
                var school = new ApplicationUser
                {
                    UserName = schoolEmail,
                    Email = schoolEmail,
                    FullName = "Dr. Yılmaz (Koordinatör)",
                    Department = "Bilgisayar Mühendisliği",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(school, "Okul123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(school, AppRoles.School);
            }

            // Demo şirket (İşveren bağlantısı için)
            var demoCompany = context.Companies.FirstOrDefault(c => c.Name == "TechCorp Yazılım A.Ş.");
            if (demoCompany == null)
            {
                demoCompany = new Company
                {
                    Name = "TechCorp Yazılım A.Ş.",
                    Sector = "Bilgi Teknolojileri",
                    Location = "İstanbul, Türkiye",
                    IsApproved = true
                };
                context.Companies.Add(demoCompany);
                await context.SaveChangesAsync();
            }

            // Varsayılan İşveren kullanıcısı
            var employerEmail = "isveren@careertrack.com";
            if (await userManager.FindByEmailAsync(employerEmail) == null)
            {
                var employer = new ApplicationUser
                {
                    UserName = employerEmail,
                    Email = employerEmail,
                    FullName = "Ayşe Kaya (İK Sorumlusu)",
                    Department = "İnsan Kaynakları",
                    CompanyId = demoCompany.Id,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(employer, "Isveren123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(employer, AppRoles.Employer);
            }

            // Varsayılan Öğrenci kullanıcısı
            var studentEmail = "ogrenci@careertrack.com";
            if (await userManager.FindByEmailAsync(studentEmail) == null)
            {
                var student = new ApplicationUser
                {
                    UserName = studentEmail,
                    Email = studentEmail,
                    FullName = "Mehmet Yıldız",
                    Department = "Bilgisayar Mühendisliği",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(student, "Ogrenci123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(student, AppRoles.Student);
            }
        }
    }
}
