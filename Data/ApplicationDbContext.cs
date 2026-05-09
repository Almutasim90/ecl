using ECL.Models;
using Microsoft.EntityFrameworkCore;

namespace ECL.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ListeningQuestion> ListeningQuestions { get; set; }
        public DbSet<ReadingQuestion> ReadingQuestions { get; set; }
        public DbSet<GrammarQuestion> GrammarQuestions { get; set; }
        public DbSet<StudentUser> StudentUsers { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<QuizAttemptAnswer> QuizAttemptAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("public");

            modelBuilder.Entity<StudentUser>(e =>
            {
                e.ToTable("studentusers");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.Username).HasColumnName("username");
                e.Property(x => x.PasswordHash).HasColumnName("passwordhash");
                e.Property(x => x.CreatedAtUtc).HasColumnName("createdatutc");
                e.HasIndex(x => x.Username).IsUnique();
            });

            modelBuilder.Entity<QuizAttempt>(e =>
            {
                e.ToTable("quizattempts");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.StudentUserId).HasColumnName("studentuserid");
                e.Property(x => x.Mode).HasColumnName("mode");
                e.Property(x => x.FormNumber).HasColumnName("formnumber");
                e.Property(x => x.GrammarType).HasColumnName("grammartype");
                e.Property(x => x.Level).HasColumnName("level");
                e.Property(x => x.TotalQuestions).HasColumnName("totalquestions");
                e.Property(x => x.CorrectCount).HasColumnName("correctcount");
                e.Property(x => x.StartedAtUtc).HasColumnName("startedatutc");
                e.Property(x => x.FinishedAtUtc).HasColumnName("finishedatutc");

                e.HasOne(x => x.StudentUser)
                    .WithMany()
                    .HasForeignKey(x => x.StudentUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<QuizAttemptAnswer>(e =>
            {
                e.ToTable("quizattemptanswers");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.QuizAttemptId).HasColumnName("quizattemptid");
                e.Property(x => x.Mode).HasColumnName("mode");
                e.Property(x => x.QuestionQno).HasColumnName("questionqno");
                e.Property(x => x.SelectedOption).HasColumnName("selectedoption");
                e.Property(x => x.CorrectOption).HasColumnName("correctoption");
                e.Property(x => x.IsCorrect).HasColumnName("iscorrect");

                e.HasOne(x => x.QuizAttempt)
                    .WithMany(a => a.Answers)
                    .HasForeignKey(x => x.QuizAttemptId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => new { x.QuizAttemptId, x.QuestionQno }).IsUnique();
            });

            modelBuilder.Entity<ListeningQuestion>(e =>
            {
                e.ToTable("listeningquestions");
                e.HasKey(x => x.Qno);
                e.Property(x => x.Qno).HasColumnName("qno");
                e.Property(x => x.FormNumber).HasColumnName("formnumber");
                e.Property(x => x.AudioFile).HasColumnName("audiofile");
                e.Property(x => x.OptionA).HasColumnName("optiona");
                e.Property(x => x.OptionB).HasColumnName("optionb");
                e.Property(x => x.OptionC).HasColumnName("optionc");
                e.Property(x => x.OptionD).HasColumnName("optiond");
                e.Property(x => x.CorrectOption).HasColumnName("correctoption");
            });

            modelBuilder.Entity<ReadingQuestion>(e =>
            {
                e.ToTable("readingquestions");
                e.HasKey(x => x.Qno);
                e.Property(x => x.Qno).HasColumnName("qno");
                e.Property(x => x.FormNumber).HasColumnName("formnumber");
                e.Property(x => x.QuestionText).HasColumnName("questiontext");
                e.Property(x => x.OptionA).HasColumnName("optiona");
                e.Property(x => x.OptionB).HasColumnName("optionb");
                e.Property(x => x.OptionC).HasColumnName("optionc");
                e.Property(x => x.OptionD).HasColumnName("optiond");
                e.Property(x => x.CorrectOption).HasColumnName("correctoption");
            });

            modelBuilder.Entity<GrammarQuestion>(e =>
            {
                e.ToTable("grammarquestions");
                e.HasKey(x => x.Qno);
                e.Property(x => x.Qno).HasColumnName("qno");
                e.Property(x => x.GrammarType).HasColumnName("grammartype");
                e.Property(x => x.Level).HasColumnName("level");
                e.Property(x => x.QuestionText).HasColumnName("questiontext");
                e.Property(x => x.OptionA).HasColumnName("optiona");
                e.Property(x => x.OptionB).HasColumnName("optionb");
                e.Property(x => x.OptionC).HasColumnName("optionc");
                e.Property(x => x.OptionD).HasColumnName("optiond");
                e.Property(x => x.CorrectOption).HasColumnName("correctoption");
                e.Property(x => x.Explanation).HasColumnName("explanation");
            });
        }
    }
}
