using DummyDataDog.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DummyDataDog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LogsController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet("{projectId}/range")]
        public async Task<IActionResult> GetLogsByDateRange(int projectId, DateTime from, DateTime to)
        {
            try
            {
                var logs = await _context.Logs
                    .Where(l => l.ProjectId == projectId && l.LoggedAt >= from && l.LoggedAt <= to)
                    .OrderByDescending(l => l.LoggedAt)
                    .Select(l => new
                    {
                        l.Id,
                        l.LogLevel,
                        l.Message,
                        l.LoggedAt
                    })
                    .ToListAsync();

                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("latest/{projectId}")]
        public async Task<IActionResult> GetLatestLogsByProject(int projectId)
        {
            try
            {
                var logs = await _context.Logs
                    .Where(l => l.ProjectId == projectId)
                    .Include(l => l.Project)
                    .OrderByDescending(l => l.LoggedAt)
                    .Take(50)
                    .Select(l => new
                    {
                        l.Id,
                        l.LogLevel,
                        l.Message,
                        ProjectName = l.Project.Name,
                        l.LoggedAt
                    })
                    .ToListAsync();

                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error while fetching logs.",
                    error = ex.Message
                });
            }
        }
    }
}
