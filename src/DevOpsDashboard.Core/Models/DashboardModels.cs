public class HardwareMetric
{
    public string HardwareName { get; set; } = string.Empty;
    public string HardwareType { get; set; } = string.Empty;  // CPU, GPU, RAM
    public string SensorName { get; set; } = string.Empty;    // Core Temp, Fan Speed etc
    public string SensorType { get; set; } = string.Empty;    // Temperature, Load, Fan
    public float Value { get; set; }
    public string Unit { get; set; } = string.Empty;          // °C, %, RPM
    public DateTime Timestamp { get; set; }
}