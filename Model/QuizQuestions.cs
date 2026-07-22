using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mathly.Models
{
    [Table("QuizQuestions")]
    public class QuizQuestions
    {
        [Key]
        public string QuestionID { get; set; }
        public string QuizID { get; set; }
        public string QuestionText { get; set; }

        [Column("questionChoiceA")]
        public string ChoiceA { get; set; }

        [Column("questionChoiceB")]
        public string ChoiceB { get; set; }

        [Column("questionChoiceC")]
        public string ChoiceC { get; set; }

        [Column("questionChoiceD")]
        public string ChoiceD { get; set; }

        [Column("questionAnswer")]
        public string Answer { get; set; }

        [ForeignKey("QuizID")]
        public Quizzes Quiz { get; set; }
    }
}