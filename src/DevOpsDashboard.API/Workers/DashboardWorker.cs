using DevOpsDashboard.Core.Interfaces;
using DevOpsDashboard.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DevOpsDashboard.API.Workers;

public class DashboardWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IHubContext<DashboardHub> _hub;
    private readonly ILogger<DashboardWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

    public DashboardWorker(
        IServiceProvider services,
        IHubContext<DashboardHub> hub,
        ILogger<DashboardWorker> logger)
    {
        _services = services;
        _hub = hub;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Dashboard worker started");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await PushUpdatesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pushing dashboard updates");
            }

            await Task.Delay(_interval, ct);
        }
    }

    private async Task PushUpdatesAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var devOps = scope.ServiceProvider.GetRequiredService<IAzureDevOpsService>();
        var monitor = scope.ServiceProvider.GetRequiredService<IAzureMonitorService>();

        var runningPipelines = await devOps.GetRunningPipelinesAsync();
        await _hub.Clients.All.SendAsync("PipelinesUpdated", runningPipelines, ct);

        var metrics = await monitor.GetCurrentMetricsAsync();
        await _hub.Clients.All.SendAsync("MetricsUpdated", metrics, ct);

        var errors = await monitor.GetApplicationErrorsAsync(TimeSpan.FromMinutes(60));
        await _hub.Clients.All.SendAsync("ErrorsUpdated", errors, ct);

        var hardware = scope.ServiceProvider.GetRequiredService<IHardwareMonitorService>();
        var hardwareMetrics = hardware.GetHardwareMetrics();
        await _hub.Clients.All.SendAsync("HardwareUpdated", hardwareMetrics, ct);

        _logger.LogInformation("Dashboard update pushed at {Time}", DateTime.UtcNow);
    }
}