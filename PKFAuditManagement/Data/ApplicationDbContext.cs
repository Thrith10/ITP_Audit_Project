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
        public DbSet<QCDocument> QCDocuments { get; set; }

        // DbSets for QC7 Form
        public DbSet<QC7Form> QC7Forms { get; set; }
        public DbSet<QC7FormConclusion> QC7FormConclusions { get; set; }
        public DbSet<QC7FormFeeDetail> QC7FormFeeDetails { get; set; }
        public DbSet<QC7FormTest> QC7FormTests { get; set; }
        public DbSet<QC7FormObjective> QC7FormObjectives { get; set; }
        public DbSet<QC7FormTestDescription> QC7FormTestDescriptions { get; set; }
        public DbSet<QC7SubForm> QC7SubForms { get; set; }

        // DbSets for QC35 Form
        public DbSet<QC35Form> QC35Forms { get; set; }
        public DbSet<QC35FormTestDescription> QC35FormTestDescriptions { get; set; }
        public DbSet<QC35ChecklistItem> QC35ChecklistItems { get; set; }

        // DbSet for Signed FS 
        public DbSet<SignedFSForm> SignedFSForm { get; set; }

        // DbSet for Quizzes
        public DbSet<Quiz> Quiz { get; set; }
        public DbSet<Questions> Questions { get; set; }
        public DbSet<Option> Option { get; set; }
        public DbSet<Participants> Participants { get; set; }
        public DbSet<QuizResponse> QuizResponse { get; set; }
        public DbSet<Attempt> Attempt { get; set; }
        public DbSet<FeedbackForm> FeedbackForms { get; set; }
        public DbSet<FeedbackQuestion> FeedbackQuestions { get; set; }
        public DbSet<FeedbackResponse> FeedbackResponses { get; set; }
        // DbSet properties for Self-Assessment and Quiz entities
        public DbSet<SelfAssessmentForm> SelfAssessmentForms { get; set; }
        public DbSet<SelfAssessmentQuestion> SelfAssessmentQuestions { get; set; }
        public DbSet<SelfAssessmentResponse> SelfAssessmentResponses { get; set; }



        // DbSet for Chatbot
        public DbSet<ChatbotDocument> ChatbotDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure QuizID as GUID
            modelBuilder.Entity<Quiz>()
                .Property(q => q.QuizID)
                .HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<Quiz>()
                .HasMany(q => q.Questions)
                .WithOne(qn => qn.Quiz)
                .HasForeignKey(qn => qn.QuizID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Option>()
                .HasOne(o => o.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(o => o.QuestionID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizResponse>()
                .HasOne(qr => qr.Attempt)
                .WithMany(a => a.QuizResponses)
                .HasForeignKey(qr => qr.AttemptID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizResponse>()
                .HasOne(qr => qr.Question)
                .WithMany()
                .HasForeignKey(qr => qr.QuestionID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attempt>()
                .HasOne(a => a.Quiz)
                .WithMany()
                .HasForeignKey(a => a.QuizID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure FeedbackForm and its relationships
            modelBuilder.Entity<FeedbackForm>()
          .Property(ff => ff.FeedbackFormID)
          .HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<FeedbackForm>()
                .HasMany(ff => ff.Questions)
                .WithOne(q => q.FeedbackForm)
                .HasForeignKey(q => q.FeedbackFormID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.FeedbackForm)
                .WithMany() // Assuming a one-to-one relationship from Quiz to FeedbackForm
                .HasForeignKey(q => q.FeedbackFormID)
                .OnDelete(DeleteBehavior.Restrict); // Use Restrict instead of Cascade to avoid cycle conflicts

            modelBuilder.Entity<Quiz>()
                .HasMany(q => q.FeedbackResponses)
                .WithOne(fr => fr.Quiz)
                .HasForeignKey(fr => fr.QuizID)
                .OnDelete(DeleteBehavior.Restrict); // Use Restrict instead of Cascade to avoid cycle conflicts

            modelBuilder.Entity<FeedbackResponse>()
                .Property(fr => fr.FeedbackResponseID)
                .HasDefaultValueSql("NEWID()");

            // SelfAssessmentForm has a one-to-many relationship with SelfAssessmentQuestion
            modelBuilder.Entity<SelfAssessmentForm>()
                .HasMany(saf => saf.Questions)
                .WithOne(q => q.SelfAssessmentForm)
                .HasForeignKey(q => q.SelfAssessmentFormID)
                .OnDelete(DeleteBehavior.Cascade);

            // SelfAssessmentResponse has a one-to-many relationship with SelfAssessmentQuestion
            modelBuilder.Entity<SelfAssessmentResponse>()
                .HasOne(sar => sar.SelfAssessmentQuestion)
                .WithMany()
                .HasForeignKey(sar => sar.SelfAssessmentQuestionID)
                .OnDelete(DeleteBehavior.Cascade);

            // SelfAssessmentResponse has a one-to-one relationship with Quiz (or foreign key relationship)
            modelBuilder.Entity<SelfAssessmentResponse>()
                .HasOne(sar => sar.Quiz)
                .WithMany()
                .HasForeignKey(sar => sar.QuizID)
                .OnDelete(DeleteBehavior.Cascade);

            // SelfAssessmentResponse has an enum type property `Stage` stored as a string
            modelBuilder.Entity<SelfAssessmentResponse>()
                .Property(sar => sar.Stage)
                .HasConversion<string>();

            new DataSeeder(modelBuilder).Seed();
        }

    }
}
