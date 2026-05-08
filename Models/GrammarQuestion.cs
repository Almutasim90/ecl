using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECL.Models
{
    public class GrammarQuestion
    {
        public const string LevelBeginner     = "Beginner";
        public const string LevelIntermediate = "Intermediate";
        public const string LevelAdvanced     = "Advanced";

        public static readonly string[] AllowedLevels = new[]
        {
            LevelBeginner, LevelIntermediate, LevelAdvanced
        };

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Question Number")]
        public int Qno { get; set; }

        [Display(Name = "Grammar Type")]
        public string GrammarType { get; set; } = string.Empty;

        [Display(Name = "Difficulty Level")]
        public string? Level { get; set; }

        [Display(Name = "Question Text")]
        public string QuestionText { get; set; } = string.Empty;

        [Display(Name = " Option A")]
        public string OptionA { get; set; } = string.Empty;

        [Display(Name = " Option B")]
        public string OptionB { get; set; } = string.Empty;

        [Display(Name = " Option C")]
        public string OptionC { get; set; } = string.Empty;

        [Display(Name = " Option D")]
        public string OptionD { get; set; } = string.Empty;

        [Display(Name = "Correct Option")]
        public int CorrectOption { get; set; }

        [Display(Name = "Explanation")]
        [DataType(DataType.MultilineText)]
        public string? Explanation { get; set; }
    }
}

