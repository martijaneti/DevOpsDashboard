using DevOpsDashboard.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsDashboard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HardwareController : ControllerBase
{
    private readonly IHardwareMonitorService _hardware;

    public HardwareController(IHardwareMonitorService hardware)
    {
        _hardware = hardware;
    }

    [HttpGet]
    public IActionResult GetMetrics()
    {
        var metrics = _hardware.GetHardwareMetrics();
        return Ok(metrics);
    }
}