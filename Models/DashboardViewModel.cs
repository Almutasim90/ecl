namespace ECL.Models;

/// <summary>
/// View model for dashboard statistics and quick actions.
/// </summary>
public class DashboardViewModel
{
    public int ListeningQuestionsCount { get; set; }
    public int ReadingQuestionsCount { get; set; }
    public int ListeningFormsCount { get; set; }
    public int ReadingFormsCount { get; set; }
    public int TotalQuestionsCount => ListeningQuestionsCount + ReadingQuestionsCount;
}
