using System;
using Microsoft.AspNetCore.Mvc;


namespace DummyDataDog.Controllers
{
    [ApiController]
    [Route("api/alerts")]
    public class AlertsController : ControllerBase
    {
        [HttpGet("active")]
        public IActionResult GetActiveAlerts()
        {
            var alerts = new[]
            {
                new {
                    alert_id = "alert-102",
                    status = "triggered",
                    severity = "critical",
                    service = "Database",
                    description = "Database connection pool limit exceeded",
                    started_at = DateTime.UtcNow.AddMinutes(-10)
                },
                new {
                    alert_id = "alert-103",
                    status = "triggered",
                    severity = "warning",
                    service = "API Gateway",
                    description = "High response latency detected",
                    started_at = DateTime.UtcNow.AddMinutes(-5)
                }
            };

            return Ok(alerts);
        }
    }
}
