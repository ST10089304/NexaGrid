namespace NexaGrid.Shared.Models;

public class TelemetryRecord
{
    public long Id { get; set; }

    public int SensorId { get; set; }

    public string DataType { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public bool IsAnomaly { get; set; }

    public DateTime RecordedAtUtc { get; set; } = DateTime.UtcNow;

    public Sensor? Sensor { get; set; }
}