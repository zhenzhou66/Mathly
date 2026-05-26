using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mathly.Models
{
    [Table("Notification")]
    public class Notification
    {
        [Key]
        public string NotificationID { get; set; }
        public string UserID { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public bool IsRead { get; set; }

        [ForeignKey("UserID")]
        public LoginCredentials User { get; set; }
    }
}