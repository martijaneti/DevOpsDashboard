namespace DevOpsDashboard.Core.Models;

public class PipelineRun
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;  // running, succeeded, failed
    public string BranchName { get; set; } = string.Empty;
    public string TriggeredBy { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? FinishTime { get; set; }
    public TimeSpan? Duration => FinishTime.HasValue
        ? FinishTime.Value - StartTime
        : DateTime.UtcNow - StartTime;
}

public class InfrastructureMetric
{
    public string ResourceName { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;  // CpuPercentage, MemoryPercentage
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ApplicationError
{
    public string Severity { get; set; } = string.Empty;  // Error, Critical
    public string Message { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int Count { get; set; }
}