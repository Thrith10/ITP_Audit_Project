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
        public DbSet<Engagement> Engagements { get; set; }
        public DbSet<EngagementDetail> EngagementDetails { get; set; }
        public DbSet<EngagementObjective> EngagementObjectives { get; set; }
        public DbSet<EngagementProcedureTest> EngagementProcedureTests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Engagement>()
                .HasOne(e => e.EngagementDetail)
                .WithOne(ed => ed.Engagement)
                .HasForeignKey<EngagementDetail>(ed => ed.EngagementId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete for EngagementDetail

            modelBuilder.Entity<Engagement>()
                .HasMany(e => e.EngagementProcedureTests)
                .WithOne(pt => pt.Engagement)
                .HasForeignKey(pt => pt.EngagementId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete for EngagementProcedureTests
        }
    }
}
