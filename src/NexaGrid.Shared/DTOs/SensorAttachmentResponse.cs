namespace NexaGrid.Shared.DTOs;
/* OWASP Foundation (2026) */
public class SensorAttachmentResponse
{
    public int Id { get; set; }

    public int SensorId { get; set; }

    public string SensorIdentifier { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public string FormattedFileSize { get; set; } = string.Empty;

    public DateTime UploadedAtUtc { get; set; }

    public string DownloadUrl { get; set; } = string.Empty;
}