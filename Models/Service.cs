using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DummyDataDog.Models
{
    [Table("services")]
    public class Service
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }


        [Column("projectid")]
        [ForeignKey("projectid")]
        public int ProjectId { get; set; }

        public Project Project { get; set; }

        [Column("servicename")]
        [Required]
        public string ServiceName { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("checkedat")]
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    }
}
