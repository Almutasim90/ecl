using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECL.Models
{
    public class ListeningQuestion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        [Display(Name = "Question Number")]
        public int Qno { get; set; }
        [Display(Name = "Form Number")]
        public int FormNumber { get; set; }

        [Display(Name = "Question Text")]
        public string? AudioFile { get; set; }
        [Display(Name = " Option A")]
        public string? OptionA { get; set; }
        [Display(Name = " Option B")]
        public string? OptionB { get; set; }
        [Display(Name = " Option C")]
        public string? OptionC { get; set; }
        [Display(Name = " Option D")]
        public string? OptionD { get; set; }
        [Display(Name = "Correct Option")]
        public int CorrectOption { get; set; }

        [NotMapped]
        public string? AudioPath => $"/AUDIO/{AudioFile}";
    }
}
