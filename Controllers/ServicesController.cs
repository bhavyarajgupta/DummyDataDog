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
    public class ServicesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServicesController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet("{projectId}/range")]
        public async Task<IActionResult> GetServicesByDateRange(int projectId, DateTime from, DateTime to)
        {
            try
            {
                var services = await _context.Services
                    .Where(s => s.ProjectId == projectId && s.CheckedAt >= from && s.CheckedAt <= to)
                    .OrderByDescending(s => s.CheckedAt)
                    .Select(s => new
                    {
                        s.Id,
                        s.ServiceName,
                        s.Status,
                        s.CheckedAt
                    })
                    .ToListAsync();

                return Ok(services);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{projectId}/health")]
        public async Task<IActionResult> GetServiceHealthByProject(int projectId)
        {
            try
            {
                var services = await _context.Services
                    .Where(s => s.ProjectId == projectId)
                    .Include(s => s.Project)
                    .OrderByDescending(s => s.CheckedAt)
                    .Select(s => new
                    {
                        s.ServiceName,
                        s.Status,
                        ProjectName = s.Project.Name,
                        s.CheckedAt
                    })
                    .ToListAsync();

                var overallStatus = services.Any(s => s.Status == "critical") ? "critical"
                                   : services.Any(s => s.Status == "warning") ? "warning"
                                   : "healthy";

                return Ok(new
                {
                    overall_status = overallStatus,
                    services,
                    checked_at = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error while fetching service health.",
                    error = ex.Message
                });
            }
        }
    }
}
