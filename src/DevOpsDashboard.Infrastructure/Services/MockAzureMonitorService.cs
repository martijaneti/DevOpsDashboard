using DevOpsDashboard.Core.Interfaces;
using DevOpsDashboard.Core.Models;

namespace DevOpsDashboard.Infrastructure.Services;

public class MockAzureMonitorService : IAzureMonitorService
{
    private static readonly Random Rng = new();
    private static readonly string[] Resources = ["app-service-api", "app-service-frontend", "sql-server"];

    public Task<IEnumerable<InfrastructureMetric>> GetCurrentMetricsAsync()
    {
        var metrics = Resources.SelectMany(resource => new[]
        {
            new InfrastructureMetric
            {
                ResourceName = resource,
                MetricName = "CpuPercentage",
                Value = Math.Round(Rng.NextDouble() * 100, 1),
                Timestamp = DateTime.UtcNow
            },
            new InfrastructureMetric
            {
                ResourceName = resource,
                MetricName = "MemoryPercentage",
                Value = Math.Round(Rng.NextDouble() * 100, 1),
                Timestamp = DateTime.UtcNow
            }
        });

        return Task.FromResult(metrics);
    }

    public Task<IEnumerable<ApplicationError>> GetApplicationErrorsAsync(TimeSpan window)
    {
        var errors = new List<ApplicationError>
        {
            new() { Severity = "Critical", Message = "Database connection timeout", Source = "OrderService", Timestamp = DateTime.UtcNow.AddMinutes(-5), Count = 12 },
            new() { Severity = "Error", Message = "Null reference in payment handler", Source = "PaymentService", Timestamp = DateTime.UtcNow.AddMinutes(-18), Count = 3 },
            new() { Severity = "Error", Message = "Failed to send email notification", Source = "NotificationService", Timestamp = DateTime.UtcNow.AddMinutes(-34), Count = 7 },
            new() { Severity = "Warning", Message = "Slow query detected (>2s)", Source = "ReportingService", Timestamp = DateTime.UtcNow.AddMinutes(-45), Count = 22 },
        };

        return Task.FromResult<IEnumerable<ApplicationError>>(errors);
    }
}