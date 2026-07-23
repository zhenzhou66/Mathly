using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mathly.Models
{
    [Table("StudentBadges")]
    public class StudentBadges
    {
        [Key]
        [Column("studentBadgeID")]
        public string StudentBadgeID { get; set; }

        [Column("userID")]
        public string StudentID { get; set; }

        [Column("badgeID")]
        public string BadgeID { get; set; }

        [Column("earnedDate")]
        public DateOnly EarnedDate { get; set; }

        [ForeignKey("StudentID")]
        public StudentInfo Student { get; set; }

        [ForeignKey("BadgeID")]
        public Badges Badge { get; set; }
    }
}