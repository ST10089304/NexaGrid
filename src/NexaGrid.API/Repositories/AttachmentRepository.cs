using Microsoft.EntityFrameworkCore;
using NexaGrid.API.Data;
using NexaGrid.Shared.Models;

namespace NexaGrid.API.Repositories;
/*CodeProject (2014) Generic Repository Pattern in C#*/
/*C# Corner (2021) Repository Pattern In C# With Entity Framework*/
public class AttachmentRepository : IAttachmentRepository
{
    private readonly NexaGridDbContext _dbContext;

    public AttachmentRepository(
        NexaGridDbContext dbContext)
    {
        _dbContext = dbContext;
    }
/*CodeProject (2014) Generic Repository Pattern in C#*/
/*C# Corner (2021) Repository Pattern In C# With Entity Framework*/
    public async Task<Sensor?> GetSensorAsync(
        int sensorId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Sensors
            .AsNoTracking()
            .FirstOrDefaultAsync(
                sensor => sensor.Id == sensorId,
                cancellationToken);
    }
/*CodeProject (2014) Generic Repository Pattern in C#*/
/*C# Corner (2021) Repository Pattern In C# With Entity Framework*/
    public async Task<SensorAttachment?> GetByIdAsync(
        int attachmentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SensorAttachments
            .AsNoTracking()
            .Include(attachment => attachment.Sensor)
            .FirstOrDefaultAsync(
                attachment => attachment.Id == attachmentId,
                cancellationToken);
    }

    public async Task<List<SensorAttachment>> GetBySensorIdAsync(
        int sensorId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SensorAttachments
            .AsNoTracking()
            .Include(attachment => attachment.Sensor)
            .Where(attachment =>
                attachment.SensorId == sensorId)
            .OrderByDescending(attachment =>
                attachment.UploadedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        SensorAttachment attachment,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SensorAttachments.AddAsync(
            attachment,
            cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}