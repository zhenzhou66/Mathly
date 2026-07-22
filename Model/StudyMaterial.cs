using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mathly.Models
{
    [Table("StudyMaterial")]
    public class StudyMaterial
    {
        [Key]
        [Column("materialID")]
        public string MaterialID { get; set; }

        [Column("topicID")]
        public string TopicID { get; set; }

        [Column("fileName")]
        public byte[] FileName { get; set; }

        [ForeignKey("TopicID")]
        public Topic Topic { get; set; }
    }
}