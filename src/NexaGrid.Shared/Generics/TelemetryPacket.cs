namespace NexaGrid.Shared.Generics;

/// <summary>
/// Stores strongly typed telemetry without converting every value
/// to object or using unnecessary boxing and unboxing.
/// </summary>
public sealed class TelemetryPacket<T>
{
    public required string SensorIdentifier { get; init; }

    public required T Value { get; init; }

    public string Unit { get; init; } = string.Empty;

    public DateTime RecordedAtUtc { get; init; } = DateTime.UtcNow;

    public string DataType => typeof(T).Name;

    public bool IsAnomaly { get; set; }
}