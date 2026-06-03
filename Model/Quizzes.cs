using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mathly.Models
{
    [Table("Quizzes")]
    public class Quizzes
    {
        [Key]
        public string QuizID { get; set; }
        public string TopicID { get; set; }
        public string QuizTitle { get; set; }

        [ForeignKey("TopicID")]
        public Topic Topic { get; set; }
    }
}