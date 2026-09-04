namespace NexaGrid.Shared.DTOs;

public class TelemetrySeedResponse
{
    public string SensorIdentifier { get; set; } = string.Empty;

    public int RequestedReadingCount { get; set; }

    public int CreatedReadingCount { get; set; }

    public int BatchCount { get; set; }

    public int BatchSize { get; set; }

    public int AnomalyCount { get; set; }

    public double ProcessingTimeMilliseconds { get; set; }

    public DateTime StartedAtUtc { get; set; }

    public DateTime CompletedAtUtc { get; set; }
}