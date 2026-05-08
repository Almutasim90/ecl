namespace ECL.Models
{
    /// <summary>
    /// View-model for the grammar-quiz launcher (StartQuiz). Carries the list of
    /// available topics ("GrammarType" values) and the difficulty levels that are
    /// actually present in the data so the picker only shows useful options.
    /// </summary>
    public class GrammarStartQuizViewModel
    {
        public IList<string> Topics { get; set; } = new List<string>();
        public IList<string> Levels { get; set; } = new List<string>();
    }
}
