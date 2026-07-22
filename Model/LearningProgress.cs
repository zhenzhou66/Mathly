using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mathly.Models
{
    [Table("LearningProgress")]
    public class LearningProgress
    {
        [Key]
        public string ProgressID { get; set; }

        [Column("userID")]
        public string StudentID { get; set; }

        public string TopicID { get; set; }
        public double ProgressPercentage { get; set; }

        [ForeignKey("StudentID")]
        public StudentInfo Student { get; set; }

        [ForeignKey("TopicID")]
        public Topic Topic { get; set; }
    }
}