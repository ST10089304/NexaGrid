using Microsoft.EntityFrameworkCore;
using NexaGrid.API.Data;
using NexaGrid.Shared.Models;

namespace NexaGrid.API.Repositories;

public class TelemetryRepository : ITelemetryRepository
{
    private readonly NexaGridDbContext _dbContext;

    public TelemetryRepository(NexaGridDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Sensor?> GetSensorByIdentifierAsync(
        string sensorIdentifier,
        CancellationToken cancellationToken = default)
    {
        string normalizedIdentifier =
            sensorIdentifier.Trim().ToUpperInvariant();

        return await _dbContext.Sensors
            .FirstOrDefaultAsync(
                sensor =>
                    sensor.UniqueIdentifier == normalizedIdentifier,
                cancellationToken);
    }

    public async Task<TelemetryRecord?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TelemetryRecords
            .AsNoTracking()
            .Include(record => record.Sensor)
            .FirstOrDefaultAsync(
                record => record.Id == id,
                cancellationToken);
    }

    public async Task<List<TelemetryRecord>> GetRecentAsync(
        string sensorIdentifier,
        int limit,
        CancellationToken cancellationToken = default)
    {
        string normalizedIdentifier =
            sensorIdentifier.Trim().ToUpperInvariant();

        return await _dbContext.TelemetryRecords
            .AsNoTracking()
            .Include(record => record.Sensor)
            .Where(record =>
                record.Sensor != null &&
                record.Sensor.UniqueIdentifier ==
                    normalizedIdentifier)
            .OrderByDescending(record => record.RecordedAtUtc)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        TelemetryRecord telemetryRecord,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.TelemetryRecords.AddAsync(
            telemetryRecord,
            cancellationToken);
    }
public async Task AddRangeAsync(
    IEnumerable<TelemetryRecord> telemetryRecords,
    CancellationToken cancellationToken = default)
{
    await _dbContext.TelemetryRecords.AddRangeAsync(
        telemetryRecords,
        cancellationToken);
}
    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}