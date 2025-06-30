using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DummyDataDog.Models
{
    [Table("metrics")]
    public class Metric
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }


        [Column("projectid")]
        [ForeignKey("projectid")]
        public int ProjectId { get; set; }

        public Project Project { get; set; }

        [Column("metrictype")]
        [Required]
        public string MetricType { get; set; }

        [Column("value")]
        public double Value { get; set; }

        [Column("collectedat")]
        public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
    }
}
