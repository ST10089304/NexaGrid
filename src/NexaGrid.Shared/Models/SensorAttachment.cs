namespace NexaGrid.Shared.Models;

public class SensorAttachment
{
    public int Id { get; set; }

    public int SensorId { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public string StoragePath { get; set; } = string.Empty;

    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

    public Sensor? Sensor { get; set; }
}