using System.Collections.Generic;

namespace ECL.Models;

public class DashboardPageViewModel : DashboardViewModel
{
    public IReadOnlyList<int> ListeningFormNumbers { get; set; } = new List<int>();
    public IReadOnlyList<int> ListeningFormCounts { get; set; } = new List<int>();

    public IReadOnlyList<int> ReadingFormNumbers { get; set; } = new List<int>();
    public IReadOnlyList<int> ReadingFormCounts { get; set; } = new List<int>();

    public IReadOnlyList<string> GrammarLevelLabels { get; set; } = new List<string>();
    public IReadOnlyList<int> GrammarLevelCounts { get; set; } = new List<int>();

    public IReadOnlyList<string> TopGrammarTopicLabels { get; set; } = new List<string>();
    public IReadOnlyList<int> TopGrammarTopicCounts { get; set; } = new List<int>();
}

