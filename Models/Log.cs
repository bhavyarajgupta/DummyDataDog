using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DummyDataDog.Models
{
    [Table("logs")]
    public class Log
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }


        [Column("projectid")]
        [ForeignKey("projectid")]
        public int ProjectId { get; set; }

        public Project Project { get; set; }


        [Column("loglevel")]
        public string LogLevel { get; set; }

        [Column("message")]
        public string Message { get; set; }

        [Column("loggedat")]
        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    }
}
