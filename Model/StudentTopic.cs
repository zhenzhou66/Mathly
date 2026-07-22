using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mathly.Models
{
    [Table("StudentTopic")]
    public class StudentTopic
    {
        [Key]
        public string StudentTopicID { get; set; }
        [Column("userID")]
        public string StudentID { get; set; }
        public string TopicID { get; set; }
        public DateOnly SelectedDate { get; set; }

        [ForeignKey("StudentID")]
        public StudentInfo Student { get; set; }

        [ForeignKey("TopicID")]
        public Topic Topic { get; set; }
    }
}