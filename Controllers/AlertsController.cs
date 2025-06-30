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
    public class AlertsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AlertsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{projectId}/range")]
        public async Task<IActionResult> GetAlertsByDateRange(int projectId, DateTime from, DateTime to)
        {
            try
            {
                var alerts = await _context.Alerts
                    .Where(a => a.ProjectId == projectId && a.CreatedAt >= from && a.CreatedAt <= to)
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => new
                    {
                        a.Id,
                        a.Message,
                        a.Severity,
                        a.CreatedAt
                    })
                    .ToListAsync();

                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }



        [HttpGet("active/{projectId}")]
        public async Task<IActionResult> GetActiveAlertsByProject(int projectId)
        {
            try
            {
                var alerts = await _context.Alerts
                    .Where(a => a.ProjectId == projectId)
                    .Include(a => a.Project)
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => new
                    {
                        a.Id,
                        a.Message,
                        a.Severity,
                        ProjectName = a.Project.Name,
                        a.CreatedAt
                    })
                    .ToListAsync();

                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error while fetching alerts.",
                    error = ex.Message
                });
            }
        }
    }
}
