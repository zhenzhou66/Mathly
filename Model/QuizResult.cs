using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mathly.Models
{
    [Table("QuizResult")]
    public class QuizResult
    {
        [Key]
        public string ResultID { get; set; }

        [Column("userID")]
        public string StudentID { get; set; }
        public string QuizID { get; set; }
        public string AttemptID { get; set; }
        public int TotalQuestionsAmount { get; set; }
        public int TotalCorrectAnswer { get; set; }
        public double Score { get; set; }

        [ForeignKey("StudentID")]
        public StudentInfo Student { get; set; }

        [ForeignKey("QuizID")]
        public Quizzes Quiz { get; set; }

        [ForeignKey("AttemptID")]
        public QuizStudentAttempt Attempt { get; set; }
    }
}