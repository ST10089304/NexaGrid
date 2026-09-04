using NexaGrid.Shared.DTOs;

namespace NexaGrid.API.Services;

public interface ISensorService
{
    Task<IReadOnlyList<SensorResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<SensorResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<SensorResponse> RegisterAsync(
        RegisterSensorRequest request,
        CancellationToken cancellationToken = default);
}