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
        public DbSet<Interview> Interviews { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<DailyLog> DailyLogs { get; set; }
        public DbSet<ToDo> ToDos { get; set; }

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

            // Interview → JobApplication (1-N)
            builder.Entity<Interview>()
                .HasOne(i => i.JobApplication)
                .WithMany(a => a.Interviews)
                .HasForeignKey(i => i.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Offer → JobApplication (1-1)
            builder.Entity<Offer>()
                .HasOne(o => o.JobApplication)
                .WithOne(a => a.Offer)
                .HasForeignKey<Offer>(o => o.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            // DailyLog → ApplicationUser
            builder.Entity<DailyLog>()
                .HasOne(d => d.Student)
                .WithMany(u => u.DailyLogs)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ToDo → ApplicationUser
            builder.Entity<ToDo>()
                .HasOne(t => t.Student)
                .WithMany(u => u.ToDos)
                .HasForeignKey(t => t.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
