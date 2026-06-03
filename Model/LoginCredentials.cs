using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mathly.Models
{
    [Table("LoginCredentials")]
    public class LoginCredentials
    {
        [Key]
        public string UserID { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}