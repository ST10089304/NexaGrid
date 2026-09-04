using NexaGrid.Shared.Models;

namespace NexaGrid.API.Repositories;

public interface IAttachmentRepository
{
    Task<Sensor?> GetSensorAsync(
        int sensorId,
        CancellationToken cancellationToken = default);

    Task<SensorAttachment?> GetByIdAsync(
        int attachmentId,
        CancellationToken cancellationToken = default);

    Task<List<SensorAttachment>> GetBySensorIdAsync(
        int sensorId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        SensorAttachment attachment,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}