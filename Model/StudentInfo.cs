using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mathly.Models
{
    [Table("StudentInfo")]
    public class StudentInfo
    {
        [Key]
        public string UserID { get; set; }
        public string StudentName { get; set; }
        public int StudentAge { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string School { get; set; }
        public DateOnly BirthDate { get; set; }
        public string StudyLevel { get; set; }
        public int ExpPoints { get; set; }
        public DateOnly DateJoined { get; set; }
    }
}