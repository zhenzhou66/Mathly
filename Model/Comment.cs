using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mathly.Models
{
    [Table("Comment")]
    public class Comment
    {
        [Key]
        public string CommentID { get; set; }
        public string UserID { get; set; }
        public string DiscussionID { get; set; }
        public string CommentText { get; set; }
        public DateTime PostedDate { get; set; }

        [ForeignKey("UserID")]
        public LoginCredentials User { get; set; }

        [ForeignKey("DiscussionID")]
        public Discussion Discussion { get; set; }
    }
}