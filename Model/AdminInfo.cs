using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mathly.Models
{
    [Table("AdminInfo")]
    public class AdminInfo
    {
        [Key]
        public string UserID { get; set; }
        public string AdminName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateOnly DateJoined { get; set; }

        [ForeignKey("UserID")]
        public LoginCredentials LoginCredentials { get; set; }
    }
}