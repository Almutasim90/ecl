using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECL.Models;

public sealed class QuizAttempt
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public int StudentUserId { get; set; }
    public StudentUser? StudentUser { get; set; }

    public QuizMode Mode { get; set; }

    // Listening/Reading: FormNumber, Grammar: nullable (topic/level stored separately)
    public int? FormNumber { get; set; }
    public string? GrammarType { get; set; }
    public string? Level { get; set; }

    public int TotalQuestions { get; set; }
    public int CorrectCount { get; set; }

    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAtUtc { get; set; }

    public ICollection<QuizAttemptAnswer> Answers { get; set; } = new List<QuizAttemptAnswer>();
}

