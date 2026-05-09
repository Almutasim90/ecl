namespace ECL.Models;

public sealed class SaveAttemptRequest
{
    public QuizMode Mode { get; set; }

    public int? FormNumber { get; set; }
    public string? GrammarType { get; set; }
    public string? Level { get; set; }

    public int TotalQuestions { get; set; }
    public int CorrectCount { get; set; }

    public DateTime? StartedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }

    public List<SaveAttemptAnswerDto>? Answers { get; set; }
}

public sealed class SaveAttemptAnswerDto
{
    public int QuestionQno { get; set; }
    public int? SelectedOption { get; set; }
    public int CorrectOption { get; set; }
}

public sealed class SaveAttemptResponse
{
    public long AttemptId { get; set; }
}

