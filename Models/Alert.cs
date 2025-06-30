using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DummyDataDog.Models
{
    [Table("alerts")]
    public class Alert
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("projectid")]
        [ForeignKey("projectid")]
        public int ProjectId { get; set; }

        public Project Project { get; set; }

        [Column("message")]
        [Required]
        public string Message { get; set; }

        [Column("severity")]
        public string Severity { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
