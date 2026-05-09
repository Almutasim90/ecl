using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECL.Models;

public sealed class QuizAttemptAnswer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long QuizAttemptId { get; set; }
    public QuizAttempt? QuizAttempt { get; set; }

    // Question identifiers depend on mode; store qno and mode for lookup.
    public QuizMode Mode { get; set; }
    public int QuestionQno { get; set; }

    public int? SelectedOption { get; set; } // 1-4 or null for skipped
    public int CorrectOption { get; set; }   // 1-4
    public bool IsCorrect { get; set; }
}

