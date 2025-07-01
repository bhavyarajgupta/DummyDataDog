using Microsoft.EntityFrameworkCore;
using DummyDataDog.Models;

namespace DummyDataDog.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<AgentResponse> AgentResponses { get; set; }

        public DbSet<Users> Users { get; set; }
        public DbSet<ChatLog> ChatLogs { get; set; }
        public DbSet<Project> Projects { get; set; }

        public DbSet<Alert> Alerts { get; set; }
        public DbSet<Metric> Metrics { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Service> Services { get; set; }

    }

}
