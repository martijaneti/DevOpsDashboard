using DevOpsDashboard.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsDashboard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PipelinesController : ControllerBase
{
    private readonly IAzureDevOpsService _devOps;

    public PipelinesController(IAzureDevOpsService devOps)
    {
        _devOps = devOps;
    }

    [HttpGet("{project}")]
    public async Task<IActionResult> GetRecentRuns(string project, [FromQuery] int top = 20)
    {
        var runs = await _devOps.GetRecentPipelineRunsAsync(project, top);
        return Ok(runs);
    }

    [HttpGet("running")]
    public async Task<IActionResult> GetRunningPipelines()
    {
        var runs = await _devOps.GetRunningPipelinesAsync();
        return Ok(runs);
    }
}