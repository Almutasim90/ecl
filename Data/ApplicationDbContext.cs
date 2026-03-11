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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("public");

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
        }
    }
}
