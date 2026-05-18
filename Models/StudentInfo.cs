using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace testWebApp.Models
{
    [Table("StudentInfo")]
    public class StudentInfo
    {
        [Key]
        [Column("userID")]
        public string UserID { get; set; }

        [Column("studentName")]
        public string? StudentName { get; set; }

        [Column("studentAge")]
        public int? StudentAge { get; set; }

        [Column("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("school")]
        public string? School { get; set; }

        [Column("birthDate")]
        public DateTime? BirthDate { get; set; }

        [Column("dateJoined")]
        public DateTime DateJoined { get; set; }

        [Column("studyLevel")]
        public string StudyLevel { get; set; }

        [Column("expPoints")]
        public int ExpPoints { get; set; }
    }
}