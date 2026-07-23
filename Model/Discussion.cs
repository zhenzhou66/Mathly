using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mathly.Models
{
    [Table("Discussion")]
    public class Discussion
    {
        [Key]
        public string DiscussionID { get; set; }
        public string UserID { get; set; }
        public string QuestionTitle { get; set; }
        public string QuestionText { get; set; }
        public DateTime PostedDate { get; set; }

        [ForeignKey("UserID")]
        public LoginCredentials User { get; set; }
    }
}