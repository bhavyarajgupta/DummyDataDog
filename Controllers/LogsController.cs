using System;
using Microsoft.AspNetCore.Mvc;

namespace DummyDataDog.Controllers
{
    [ApiController]
    [Route("api/logs")]
    public class LogsController : ControllerBase
    {
        [HttpGet("latest")]
        public IActionResult GetLatestLogs()
        {
            var logs = new[]
            {
                new {
                    timestamp = DateTime.UtcNow.AddMinutes(-1),
                    level = "ERROR",
                    service = "OrderService",
                    message = "NullReferenceException at Line 54 in OrderProcessor.cs",
                    host = "server-1",
                    trace_id = "abc123xyz"
                },
                new {
                    timestamp = DateTime.UtcNow.AddMinutes(-3),
                    level = "WARN",
                    service = "PaymentService",
                    message = "Payment gateway timeout for transaction 84732",
                    host = "server-3",
                    trace_id = "def456uvw"
                }
            };

            return Ok(logs);
        }
    }
}
