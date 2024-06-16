using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PKFAuditManagement.Models;

namespace PKFAuditManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<CustomUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for QC6 Form
        public DbSet<QC6Form> QC6Forms { get; set; }
        public DbSet<QC6FormConclusion> QC6FormConclusions { get; set; }
        public DbSet<QC6FormFeeDetail> QC6FormFeeDetails { get; set; }
        public DbSet<QC6FormTest> QC6FormTests { get; set; }
        public DbSet<QC6FormObjective> QC6FormObjectives { get; set; }
        public DbSet<QC6FormTestDescription> QC6FormTestDescriptions { get; set; }
        public DbSet<QC6SubForm> QC6SubForms { get; set; }
        public DbSet<TNATNEAssessment> TNATNEAssessments { get; set; }
        public DbSet<TNATNESectionB> TNATNESectionBs { get; set; }
        public DbSet<TNATNESectionD> TNATNESectionDs { get; set; }

        // DbSets for QC7 Form
        public DbSet<QC7Form> QC7Forms { get; set; }
        public DbSet<QC7SubForm> QC7SubForms { get; set; }
        public DbSet<QC7FormObjective> QC7FormObjectives { get; set; }
        public DbSet<QC7FormTest> QC7FormTests { get; set; }
        public DbSet<QC7FormConclusion> QC7FormConclusions { get; set; }
        public DbSet<QC7FormTestDescription> QC7FormTestDescriptions { get; set; }

        // DbSets for QC35 Form
        public DbSet<QC35Form> QC35Forms { get; set; }
        public DbSet<QC35FormTestDescription> QC35FormTestDescriptions { get; set; }
        public DbSet<QC35ChecklistItem> QC35ChecklistItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // QC6Form to QC6FormTest relationship
            modelBuilder.Entity<QC6Form>()
                .HasMany(q => q.QC6FormTests)
                .WithOne()
                .HasForeignKey(t => t.QC6FormID)
                .OnDelete(DeleteBehavior.NoAction);

            // QC6Form to QC6FormConclusion relationship
            modelBuilder.Entity<QC6Form>()
                .HasOne(q => q.QC6FormConclusion)
                .WithOne()
                .HasForeignKey<QC6FormConclusion>(t => t.QC6FormID) 
                .OnDelete(DeleteBehavior.NoAction);

            // QC7Form to QC7FormTest relationship
            modelBuilder.Entity<QC6Form>()
                .HasMany(q => q.QC6FormTests)
                .WithOne()
                .HasForeignKey(t => t.QC6FormID)
                .OnDelete(DeleteBehavior.NoAction);

            // QC7Form to QC7FormConclusion relationship
            modelBuilder.Entity<QC6Form>()
                .HasOne(q => q.QC6FormConclusion)
                .WithOne()
                .HasForeignKey<QC6FormConclusion>(t => t.QC6FormID)
                .OnDelete(DeleteBehavior.NoAction);

            new DataSeeder(modelBuilder).Seed();
        }

    }
}
