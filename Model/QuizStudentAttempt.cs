using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mathly.Models
{
    [Table("QuizStudentAttempt")]
    public class QuizStudentAttempt
    {
        [Key]
        public string AttemptID { get; set; }
        public string StudentID { get; set; }
        public string QuizID { get; set; }
        public string QuestionID { get; set; }
        public string StudentAnswer { get; set; }
        public bool IsCorrect { get; set; }

        [ForeignKey("StudentID")]
        public StudentInfo Student { get; set; }

        [ForeignKey("QuizID")]
        public Quizzes Quiz { get; set; }

        [ForeignKey("QuestionID")]
        public QuizQuestions Question { get; set; }
    }
}