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
    public class MetricsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MetricsController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet("{projectId}/range")]
        public async Task<IActionResult> GetMetricsByDateRange(int projectId, DateTime from, DateTime to)
        {
            try
            {
                var metrics = await _context.Metrics
                    .Where(m => m.ProjectId == projectId && m.CollectedAt >= from && m.CollectedAt <= to)
                    .OrderByDescending(m => m.CollectedAt)
                    .Select(m => new
                    {
                        m.Id,
                        m.MetricType,
                        m.Value,
                        m.CollectedAt
                    })
                    .ToListAsync();

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


        [HttpGet("{projectId}/cpu-usage")]
        public async Task<IActionResult> GetCpuUsageByProject(int projectId)
        {
            try
            {
                var metrics = await _context.Metrics
                    .Where(m => m.ProjectId == projectId && m.MetricType == "CPU")
                    .Include(m => m.Project)
                    .OrderByDescending(m => m.CollectedAt)
                    .Take(50)
                    .Select(m => new
                    {
                        m.Id,
                        m.MetricType,
                        m.Value,
                        ProjectName = m.Project.Name,
                        m.CollectedAt
                    })
                    .ToListAsync();

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error while fetching metrics.",
                    error = ex.Message
                });
            }
        }
    }
}
