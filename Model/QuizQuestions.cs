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
        public string ChoiceA { get; set; }
        public string ChoiceB { get; set; }
        public string ChoiceC { get; set; }
        public string ChoiceD { get; set; }
        public char Answer { get; set; }

        [ForeignKey("QuizID")]
        public Quizzes Quiz { get; set; }
    }
}