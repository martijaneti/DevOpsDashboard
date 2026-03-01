using Azure.Identity;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using DevOpsDashboard.Core.Interfaces;
using DevOpsDashboard.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DevOpsDashboard.Infrastructure.Services;

public class AzureMonitorService : IAzureMonitorService
{
    private readonly AzureMonitorOptions _options;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AzureMonitorService> _logger;
    private readonly MetricsQueryClient _metricsClient;
    private readonly LogsQueryClient _logsClient;

    private const string ErrorsKql = """
        union exceptions, traces
        | where timestamp > ago({0}m)
        | where severityLevel >= 3
        | summarize Count = count() by
            tostring(outerMessage),
            tostring(operation_Name),
            severityLevel,
            bin(timestamp, 5m)
        | order by timestamp desc
        | take 50
        """;

    public AzureMonitorService(
        IOptions<AzureMonitorOptions> options,
        IMemoryCache cache,
        ILogger<AzureMonitorService> logger)
    {
        _options = options.Value;
        _cache = cache;
        _logger = logger;

        var credential = new DefaultAzureCredential();
        _metricsClient = new MetricsQueryClient(credential);
        _logsClient = new LogsQueryClient(credential);
    }

    //queries Azure Monitor for CPU and Memory percentages over the last 5 minutes
    public async Task<IEnumerable<InfrastructureMetric>> GetCurrentMetricsAsync()
    {
        return await _cache.GetOrCreateAsync("monitor:metrics", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            try
            {
                var resourceId = $"/subscriptions/{_options.SubscriptionId}" +
                                 $"/resourceGroups/{_options.ResourceGroupName}";

                var response = await _metricsClient.QueryResourceAsync(
                    resourceId,
                    new[] { "CpuPercentage", "MemoryPercentage" },
                    new MetricsQueryOptions
                    {
                        TimeRange = new QueryTimeRange(TimeSpan.FromMinutes(5))
                    });

                return response.Value.Metrics
                    .SelectMany(metric => metric.TimeSeries
                        .SelectMany(ts => ts.Values
                            .Where(v => v.Average.HasValue)
                            .Select(v => new InfrastructureMetric
                            {
                                ResourceName = _options.ResourceGroupName,
                                MetricName = metric.Name,
                                Value = v.Average!.Value,
                                Timestamp = v.TimeStamp.UtcDateTime
                            })))
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch infrastructure metrics");
                return Enumerable.Empty<InfrastructureMetric>();
            }
        }) ?? Enumerable.Empty<InfrastructureMetric>();
    }

    //runs a KQL query against your Log Analytics workspace.
    //KQL (Kusto Query Language) is Azure's query language, similar to SQL.
    public async Task<IEnumerable<ApplicationError>> GetApplicationErrorsAsync(TimeSpan window)
    {
        var cacheKey = $"monitor:errors:{(int)window.TotalMinutes}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            try
            {
                var kql = string.Format(ErrorsKql, (int)window.TotalMinutes);
                var response = await _logsClient.QueryWorkspaceAsync(
                    _options.LogAnalyticsWorkspaceId,
                    kql,
                    new QueryTimeRange(window));

                return response.Value.Table.Rows.Select(row => new ApplicationError
                {
                    Message = row["outerMessage"]?.ToString() ?? "Unknown error",
                    Source = row["operation_Name"]?.ToString() ?? string.Empty,
                    Severity = MapSeverity(row["severityLevel"]?.ToString()),
                    Timestamp = DateTime.Parse(row["timestamp"]?.ToString() ?? DateTime.UtcNow.ToString()),
                    Count = int.TryParse(row["Count"]?.ToString(), out var c) ? c : 1
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch application errors");
                return Enumerable.Empty<ApplicationError>();
            }
        }) ?? Enumerable.Empty<ApplicationError>();
    }

    private static string MapSeverity(string? level) => level switch
    {
        "4" => "Critical",
        "3" => "Error",
        "2" => "Warning",
        _ => "Error"
    };
}