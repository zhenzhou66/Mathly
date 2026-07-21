using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mathly.Models
{
    [Table("TeacherInfo")]
    public class TeacherInfo
    {
        [Key]
        public string UserID { get; set; }
        public string TeacherName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string TopicID { get; set; }
        public DateOnly BirthDate { get; set; }
        public string HighestQualification { get; set; }
        public DateOnly DateJoined { get; set; }

        [ForeignKey("UserID")]
        public LoginCredentials LoginCredentials { get; set; }
    }
}