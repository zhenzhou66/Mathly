using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mathly.Models
{
    [Table("Topic")]
    public class Topic
    {
        [Key]
        public string TopicID { get; set; }
        public string TopicName { get; set; }
        public string TeacherID { get; set; }

        [ForeignKey("TeacherID")]
        public TeacherInfo Teacher { get; set; }
    }
}