using CareerTrack.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Company> Companies { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<DailyLog> DailyLogs { get; set; }
        public DbSet<ToDo> ToDos { get; set; }
        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<StudentTask> StudentTasks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // JobApplication → Company (N-1)
            builder.Entity<JobApplication>()
                .HasOne(a => a.Company)
                .WithMany(c => c.JobApplications)
                .HasForeignKey(a => a.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // JobApplication → ApplicationUser (N-1)
            builder.Entity<JobApplication>()
                .HasOne(a => a.Student)
                .WithMany(u => u.JobApplications)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // DailyLog → ApplicationUser
            builder.Entity<DailyLog>()
                .HasOne(d => d.Student)
                .WithMany(u => u.DailyLogs)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // DailyLog → JobApplication
            builder.Entity<DailyLog>()
                .HasOne(d => d.JobApplication)
                .WithMany()
                .HasForeignKey(d => d.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            // StudentTask → JobApplication
            builder.Entity<StudentTask>()
                .HasOne(t => t.JobApplication)
                .WithMany()
                .HasForeignKey(t => t.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            // StudentTask → ApplicationUser (Employer)
            builder.Entity<StudentTask>()
                .HasOne(t => t.AssignedByEmployer)
                .WithMany()
                .HasForeignKey(t => t.AssignedByEmployerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ToDo → ApplicationUser
            builder.Entity<ToDo>()
                .HasOne(t => t.Student)
                .WithMany(u => u.ToDos)
                .HasForeignKey(t => t.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Yeni İlişkiler ─────────────────────────────────

            // ApplicationUser → Company (İşveren bağlantısı, opsiyonel)
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Company)
                .WithMany()
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Company → CreatedBy (Öğrenci tarafından önerilen şirket)
            builder.Entity<Company>()
                .HasOne(c => c.CreatedBy)
                .WithMany()
                .HasForeignKey(c => c.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Company → Employer (Şirketi ekleyen işveren)
            builder.Entity<Company>()
                .HasOne(c => c.Employer)
                .WithMany()
                .HasForeignKey(c => c.EmployerId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // JobPosting → Employer (N-1)
            builder.Entity<JobPosting>()
                .HasOne(jp => jp.Employer)
                .WithMany(u => u.JobPostings)
                .HasForeignKey(jp => jp.EmployerId)
                .OnDelete(DeleteBehavior.Cascade);

            // JobPosting → Company (N-1)
            builder.Entity<JobPosting>()
                .HasOne(jp => jp.Company)
                .WithMany(c => c.JobPostings)
                .HasForeignKey(jp => jp.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
