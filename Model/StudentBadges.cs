using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mathly.Models
{
    [Table("StudentBadges")]
    public class StudentBadges
    {
        [Key]
        public string StudentBadgeID { get; set; }
        public string StudentID { get; set; }
        public string BadgeID { get; set; }
        public DateOnly EarnedDate { get; set; }

        [ForeignKey("StudentID")]
        public StudentInfo Student { get; set; }

        [ForeignKey("BadgeID")]
        public Badges Badge { get; set; }
    }
}