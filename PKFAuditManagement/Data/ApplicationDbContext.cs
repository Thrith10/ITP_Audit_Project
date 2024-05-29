using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PKFAuditManagement.Models;

namespace PKFAuditManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<QC6Form> QC6Forms { get; set; }
        public DbSet<QC6SubForm> QC6SubForms { get; set; }
        public DbSet<QC6FormObjective> QC6FormObjectives { get; set; }
        public DbSet<QC6FormTest> QC6FormTests { get; set; }
        public DbSet<QC6FormConclusion> QC6FormConclusions { get; set; }
        public DbSet<QC6FormTestDescription> QC6FormTestDescriptions { get; set; }
        public DbSet<ContinuingEngagement> ContinuingEngagements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // QC6Form to QC6FormTest relationship
            modelBuilder.Entity<QC6Form>()
                .HasMany(q => q.QC6FormTests)
                .WithOne()
                .HasForeignKey(t => t.QC6FormID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<QC6Form>()
                .HasOne(q => q.QC6FormConclusion)
                .WithOne()
                .HasForeignKey<QC6FormConclusion>(t => t.QC6FormID) 
                .OnDelete(DeleteBehavior.NoAction);

            new DataSeeder(modelBuilder).Seed();
        }

    }
}
