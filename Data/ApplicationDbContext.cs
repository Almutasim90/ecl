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

            // Explicitly set default schema to 'public' for PostgreSQL
            modelBuilder.HasDefaultSchema("public");
        }
    }
}
