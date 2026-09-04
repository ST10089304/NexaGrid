using Microsoft.EntityFrameworkCore;
using NexaGrid.API.Data;
using NexaGrid.Shared.Models;

namespace NexaGrid.API.Repositories;

public class SensorRepository : ISensorRepository
{
    private readonly NexaGridDbContext _dbContext;

    public SensorRepository(NexaGridDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Sensor>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Sensors
            .AsNoTracking()
            .Include(sensor => sensor.Device)
            .OrderByDescending(sensor => sensor.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Sensor?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Sensors
            .AsNoTracking()
            .Include(sensor => sensor.Device)
            .FirstOrDefaultAsync(
                sensor => sensor.Id == id,
                cancellationToken);
    }

    public async Task<Sensor?> GetByIdentifierAsync(
        string uniqueIdentifier,
        CancellationToken cancellationToken = default)
    {
        string normalizedIdentifier =
            uniqueIdentifier.Trim().ToUpperInvariant();

        return await _dbContext.Sensors
            .AsNoTracking()
            .Include(sensor => sensor.Device)
            .FirstOrDefaultAsync(
                sensor =>
                    sensor.UniqueIdentifier == normalizedIdentifier,
                cancellationToken);
    }

    public async Task<Device?> GetDeviceByMacAddressAsync(
        string macAddress,
        CancellationToken cancellationToken = default)
    {
        string normalizedMacAddress =
            macAddress.Trim().ToUpperInvariant();

        return await _dbContext.Devices
            .FirstOrDefaultAsync(
                device => device.MacAddress == normalizedMacAddress,
                cancellationToken);
    }

    public async Task AddDeviceAsync(
        Device device,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Devices.AddAsync(
            device,
            cancellationToken);
    }

    public async Task AddSensorAsync(
        Sensor sensor,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Sensors.AddAsync(
            sensor,
            cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}