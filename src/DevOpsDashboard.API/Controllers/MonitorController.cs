using DevOpsDashboard.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsDashboard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MonitorController : ControllerBase
{
    private readonly IAzureMonitorService _monitor;

    public MonitorController(IAzureMonitorService monitor)
    {
        _monitor = monitor;
    }

    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics()
    {
        var metrics = await _monitor.GetCurrentMetricsAsync();
        return Ok(metrics);
    }

    [HttpGet("errors")]
    public async Task<IActionResult> GetErrors([FromQuery] int minutes = 60)
    {
        var errors = await _monitor.GetApplicationErrorsAsync(TimeSpan.FromMinutes(minutes));
        return Ok(errors);
    }
}