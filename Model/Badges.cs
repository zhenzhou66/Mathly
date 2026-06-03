using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mathly.Models
{
    [Table("Badges")]
    public class Badges
    {
        [Key]
        public string BadgeID { get; set; }
        public int ExpPoints { get; set; }
        public byte[] BadgeImage { get; set; }
    }
}