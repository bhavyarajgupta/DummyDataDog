using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DummyDataDog.Models
{
    [Table("chatlogs")]
    public class ChatLog
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("userid")]
        public int UserId { get; set; }

        [Column("prompt")]
        public string Prompt { get; set; }

        [Column("response")]
        public string Response { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Column("projectid")]
        public int? ProjectId { get; set; }

        [Column("datatype")]
        public string DataType { get; set; }

        [Column("timerange")]
        public string TimeRange { get; set; }

        [Column("sessionid")]
        public string SessionId { get; set; }

    }
}
