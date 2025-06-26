
using Microsoft.AspNetCore.Mvc;
using System;

namespace DummyDataDog.Controllers
{
    [ApiController]
    [Route("api/services")]
    public class ServicesController : ControllerBase
    {
        [HttpGet("health")]
        public IActionResult GetServiceHealth()
        {
            var health = new
            {
                overall_status = "degraded",
                services = new[]
                {
                    new { name = "OrderService", status = "healthy" },
                    new { name = "PaymentService", status = "warning" },
                    new { name = "Database", status = "critical" }
                },
                checked_at = DateTime.UtcNow
            };

            return Ok(health);
        }
    }
}
