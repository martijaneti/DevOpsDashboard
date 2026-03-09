using DevOpsDashboard.Core.Interfaces;
using DevOpsDashboard.Core.Models;
using LibreHardwareMonitor.Hardware;
using Microsoft.Extensions.Logging;

namespace DevOpsDashboard.Infrastructure.Services;

public class HardwareMonitorService : IHardwareMonitorService, IDisposable
{
    private readonly Computer _computer;
    private readonly ILogger<HardwareMonitorService> _logger;

    public HardwareMonitorService(ILogger<HardwareMonitorService> logger)
    {
        _logger = logger;
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsStorageEnabled = true
        };
        _computer.Open();
        _logger.LogInformation("Hardware monitor initialized");
    }

    public IEnumerable<HardwareMetric> GetHardwareMetrics()
    {
        var metrics = new List<HardwareMetric>();

        try
        {
            foreach (var hardware in _computer.Hardware)
            {
                hardware.Update();

                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.Value is null) continue;

                    metrics.Add(new HardwareMetric
                    {
                        HardwareName = hardware.Name,
                        HardwareType = hardware.HardwareType.ToString(),
                        SensorName = sensor.Name,
                        SensorType = sensor.SensorType.ToString(),
                        Value = sensor.Value.Value,
                        Unit = GetUnit(sensor.SensorType),
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Also check sub-hardware (e.g. CPU cores)
                foreach (var sub in hardware.SubHardware)
                {
                    sub.Update();
                    foreach (var sensor in sub.Sensors)
                    {
                        if (sensor.Value is null) continue;

                        metrics.Add(new HardwareMetric
                        {
                            HardwareName = $"{hardware.Name} - {sub.Name}",
                            HardwareType = hardware.HardwareType.ToString(),
                            SensorName = sensor.Name,
                            SensorType = sensor.SensorType.ToString(),
                            Value = sensor.Value.Value,
                            Unit = GetUnit(sensor.SensorType),
                            Timestamp = DateTime.UtcNow
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read hardware metrics");
        }

        return metrics;
    }

    private static string GetUnit(SensorType type) => type switch
    {
        SensorType.Temperature => "°C",
        SensorType.Load => "%",
        SensorType.Fan => "RPM",
        SensorType.Clock => "MHz",
        SensorType.Voltage => "V",
        SensorType.Power => "W",
        SensorType.Data => "GB",
        SensorType.SmallData => "MB",
        _ => string.Empty
    };

    public void Dispose()
    {
        _computer.Close();
    }
}