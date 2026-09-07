using NexaGrid.Shared.Models;

namespace NexaGrid.API.Repositories;
/*C# Corner (2021) Repository Pattern In C# With Entity Framework*/
public interface ISensorRepository
{
    Task<List<Sensor>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Sensor?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Sensor?> GetByIdentifierAsync(
        string uniqueIdentifier,
        CancellationToken cancellationToken = default);

    Task<Device?> GetDeviceByMacAddressAsync(
        string macAddress,
        CancellationToken cancellationToken = default);

    Task AddDeviceAsync(
        Device device,
        CancellationToken cancellationToken = default);

    Task AddSensorAsync(
        Sensor sensor,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}