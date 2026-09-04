using NexaGrid.Shared.Enums;

namespace NexaGrid.Shared.Models;

public class Sensor
{
    public int Id { get; set; }

    public int DeviceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string UniqueIdentifier { get; set; } = string.Empty;

    public string DeploymentLocation { get; set; } = string.Empty;

    public SensorCategory Category { get; set; }

    public SensorStatus Status { get; set; } = SensorStatus.Online;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? LastReadingAtUtc { get; set; }

    public Device? Device { get; set; }

    public List<TelemetryRecord> TelemetryRecords { get; set; } = [];

    public List<SensorAttachment> Attachments { get; set; } = [];
}