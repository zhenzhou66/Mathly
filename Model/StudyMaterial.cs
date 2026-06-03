using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mathly.Models
{
    [Table("StudyMaterial")]
    public class StudyMaterial
    {
        [Key]
        public string MaterialID { get; set; }
        public string TopicID { get; set; }
        public string FileName { get; set; }

        [ForeignKey("TopicID")]
        public Topic Topic { get; set; }
    }
}