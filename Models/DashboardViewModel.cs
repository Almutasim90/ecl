namespace ECL.Models;

/// <summary>
/// View model for dashboard statistics and quick actions.
/// </summary>
public class DashboardViewModel
{
    public int ListeningQuestionsCount { get; set; }
    public int ReadingQuestionsCount { get; set; }
    public int GrammarQuestionsCount { get; set; }

    public int ListeningFormsCount { get; set; }
    public int ReadingFormsCount { get; set; }
    /// <summary>Distinct grammar topics (GrammarType values).</summary>
    public int GrammarTopicsCount { get; set; }

    public int GrammarLevelBeginnerCount { get; set; }
    public int GrammarLevelIntermediateCount { get; set; }
    public int GrammarLevelAdvancedCount { get; set; }

    public int TotalQuestionsCount =>
        ListeningQuestionsCount + ReadingQuestionsCount + GrammarQuestionsCount;

    /// <summary>Listening + reading exam forms (distinct form numbers).</summary>
    public int TotalListeningReadingFormsCount => ListeningFormsCount + ReadingFormsCount;
}
