using NexaGrid.Shared.Enums;

namespace NexaGrid.Shared.DTOs;

public class TelemetryResponse
{
    public long Id { get; set; }

    public int SensorId { get; set; }

    public string SensorIdentifier { get; set; } = string.Empty;

    public TelemetryDataType DataType { get; set; }

    public string Value { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public bool IsAnomaly { get; set; }

    public string StatusMessage { get; set; } = string.Empty;

    public DateTime RecordedAtUtc { get; set; }
}