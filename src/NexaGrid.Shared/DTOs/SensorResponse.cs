using NexaGrid.Shared.Enums;

namespace NexaGrid.Shared.DTOs;

public class SensorResponse
{
    public int Id { get; set; }

    public int DeviceId { get; set; }

    public string DeviceName { get; set; } = string.Empty;

    public string MacAddress { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public string SensorName { get; set; } = string.Empty;

    public string UniqueIdentifier { get; set; } = string.Empty;

    public string DeploymentLocation { get; set; } = string.Empty;

    public SensorCategory Category { get; set; }

    public SensorStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? LastReadingAtUtc { get; set; }
}