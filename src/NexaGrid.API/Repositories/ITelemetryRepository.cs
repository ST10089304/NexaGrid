using NexaGrid.Shared.Models;

namespace NexaGrid.API.Repositories;
/*C# Corner (2021) Repository Pattern In C# With Entity Framework*/public interface ITelemetryRepository
{
    Task<Sensor?> GetSensorByIdentifierAsync(
        string sensorIdentifier,
        CancellationToken cancellationToken = default);

    Task<TelemetryRecord?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<List<TelemetryRecord>> GetRecentAsync(
        string sensorIdentifier,
        int limit,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        TelemetryRecord telemetryRecord,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
    IEnumerable<TelemetryRecord> telemetryRecords,
    CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}