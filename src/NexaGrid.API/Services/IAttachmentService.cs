using NexaGrid.Shared.DTOs;

namespace NexaGrid.API.Services;
/*CodeProject (2018)*/
public interface IAttachmentService
{
    Task<SensorAttachmentResponse> UploadAsync(
        int sensorId,
        IFormFile file,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SensorAttachmentResponse>> GetBySensorIdAsync(
        int sensorId,
        CancellationToken cancellationToken = default);

    Task<AttachmentDownloadResult> DownloadAsync(
        int attachmentId,
        CancellationToken cancellationToken = default);
}