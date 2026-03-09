using DevOpsDashboard.Core.Models;

namespace DevOpsDashboard.Core.Interfaces;

public interface IAzureDevOpsService
{
    Task<IEnumerable<PipelineRun>> GetRecentPipelineRunsAsync(string project, int top = 20);
    Task<IEnumerable<PipelineRun>> GetRunningPipelinesAsync();
}

public interface IAzureMonitorService
{
    Task<IEnumerable<InfrastructureMetric>> GetCurrentMetricsAsync();
    Task<IEnumerable<ApplicationError>> GetApplicationErrorsAsync(TimeSpan window);
    //TimeSpan window parameter lets the caller decide — last 1 hour, last 24 hours, etc.
}

public interface IHardwareMonitorService
{
    IEnumerable<HardwareMetric> GetHardwareMetrics();
}