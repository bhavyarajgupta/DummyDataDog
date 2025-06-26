using System;
using Microsoft.AspNetCore.Mvc;

namespace DummyDataDog.Controllers
{
    [ApiController]
    [Route("api/metrics")]
    public class MetricsController : ControllerBase
    {
        [HttpGet("cpu-usage")]
        public IActionResult GetCpuUsage()
        {
            var metrics = new
            {
                metric = "system.cpu.user",
                host = "server-1",
                interval = "1m",
                unit = "percent",
                data = new[]
                {
                    new { timestamp = DateTime.UtcNow.AddMinutes(-3), value = 35.5 },
                    new { timestamp = DateTime.UtcNow.AddMinutes(-2), value = 37.8 },
                    new { timestamp = DateTime.UtcNow.AddMinutes(-1), value = 40.2 }
                }
            };

            return Ok(metrics);
        }
    }
}
