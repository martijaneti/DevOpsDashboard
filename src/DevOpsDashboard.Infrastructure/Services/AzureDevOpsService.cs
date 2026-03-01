using DevOpsDashboard.Core.Interfaces;
using DevOpsDashboard.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace DevOpsDashboard.Infrastructure.Services;

public class AzureDevOpsService : IAzureDevOpsService
{
    private readonly AzureDevOpsOptions _options;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AzureDevOpsService> _logger;
    private readonly VssConnection _connection;

    public AzureDevOpsService(
        IOptions<AzureDevOpsOptions> options,
        IMemoryCache cache,
        ILogger<AzureDevOpsService> logger)
    {
        _options = options.Value;
        _cache = cache;
        _logger = logger;

        var credentials = new VssBasicCredential(string.Empty, _options.PersonalAccessToken);
        _connection = new VssConnection(new Uri(_options.OrganizationUrl), credentials);
    }

    //calls the Azure DevOps Build API and fetches the latest runs for a given project.
    public async Task<IEnumerable<PipelineRun>> GetRecentPipelineRunsAsync(
        string project, int top = 20)
    {
        var cacheKey = $"pipelines:recent:{project}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            try
            {
                var client = await _connection.GetClientAsync<BuildHttpClient>();
                var builds = await client.GetBuildsAsync(
                    project,
                    top: top,
                    queryOrder: BuildQueryOrder.QueueTimeDescending);

                return builds.Select(MapBuild).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch pipeline runs for {Project}", project);
                return Enumerable.Empty<PipelineRun>();
            }
        }) ?? Enumerable.Empty<PipelineRun>();
    }

    // fans out across all your configured projects simultaneously using Task.WhenAll 
    // — rather than fetching one project, waiting, then the next. All projects are queried in parallel.
    public async Task<IEnumerable<PipelineRun>> GetRunningPipelinesAsync()
    {
        return await _cache.GetOrCreateAsync("pipelines:running", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
            var tasks = _options.Projects.Select(async project =>
            {
                var client = await _connection.GetClientAsync<BuildHttpClient>();
                var builds = await client.GetBuildsAsync(
                    project,
                    statusFilter: BuildStatus.InProgress);
                return builds.Select(MapBuild);
            });

            var results = await Task.WhenAll(tasks);
            return results.SelectMany(r => r).ToList();
        }) ?? Enumerable.Empty<PipelineRun>();
    }

    private static PipelineRun MapBuild(Build b) => new()
    {
        Id = b.Id.ToString(),
        Name = b.Definition?.Name ?? "Unknown",
        Project = b.Project?.Name ?? string.Empty,
        Status = b.Status?.ToString().ToLowerInvariant() ?? "unknown",
        BranchName = b.SourceBranch?.Replace("refs/heads/", "") ?? string.Empty,
        TriggeredBy = b.RequestedBy?.DisplayName ?? "system",
        StartTime = b.StartTime ?? DateTime.UtcNow,
        FinishTime = b.FinishTime
    };
}