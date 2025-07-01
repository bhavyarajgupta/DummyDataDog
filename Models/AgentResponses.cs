using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DummyDataDog.Models
{
    [Table("agentresponses")]
    public class AgentResponse
    {
        [Key]
        public int id { get; set; }

        public int userid { get; set; }

        [Required]
        public string sessionid { get; set; }

        [Required]
        public string type { get; set; }   // Example values: "MainDataset", "FollowupQuery"

        public DateTime createdat { get; set; } = DateTime.UtcNow;

        public int? projectid { get; set; }
        public string datatype { get; set; }

        public string prompt { get; set; }
        public string datasent { get; set; }  // The dataset or history sent to Gemini

        public string response { get; set; }  // Gemini's reply
    }
}
