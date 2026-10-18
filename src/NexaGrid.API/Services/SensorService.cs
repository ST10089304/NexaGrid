using NexaGrid.API.Repositories;
using NexaGrid.Shared.DTOs;
using NexaGrid.Shared.Enums;
using NexaGrid.Shared.Models;

namespace NexaGrid.API.Services;

/*
 * Device lookup data is mirrored in an in-memory Dictionary keyed by MAC
 * address for efficient operational access (Microsoft, 2026).
 */
public class SensorService : ISensorService
{
    private readonly ISensorRepository _sensorRepository;
    private readonly IOperationalLookupService _operationalLookupService;

    public SensorService(
        ISensorRepository sensorRepository,
        IOperationalLookupService operationalLookupService)
    {
        _sensorRepository = sensorRepository;
        _operationalLookupService = operationalLookupService;
    }

    public async Task<IReadOnlyList<SensorResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        List<Sensor> sensors =
            await _sensorRepository.GetAllAsync(cancellationToken);

        foreach (Sensor sensor in sensors)
        {
            CacheDevice(sensor.Device);

            SynchronizeConnectionState(
                sensor.UniqueIdentifier,
                sensor.Status);
        }

        return sensors
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<SensorResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        Sensor? sensor =
            await _sensorRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (sensor is null)
        {
            return null;
        }

        CacheDevice(sensor.Device);

        SynchronizeConnectionState(
            sensor.UniqueIdentifier,
            sensor.Status);

        return MapToResponse(sensor);
    }

    public async Task<SensorResponse> RegisterAsync(
        RegisterSensorRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        string normalizedIdentifier =
            request.UniqueIdentifier
                .Trim()
                .ToUpperInvariant();

        string normalizedMacAddress =
            request.MacAddress
                .Trim()
                .Replace('-', ':')
                .ToUpperInvariant();

        Sensor? existingSensor =
            await _sensorRepository.GetByIdentifierAsync(
                normalizedIdentifier,
                cancellationToken);

        if (existingSensor is not null)
        {
            throw new InvalidOperationException(
                $"A sensor with identifier '{normalizedIdentifier}' already exists.");
        }

        Device? device =
            await _sensorRepository.GetDeviceByMacAddressAsync(
                normalizedMacAddress,
                cancellationToken);

        if (device is null)
        {
            device = new Device
            {
                Name = request.DeviceName.Trim(),
                MacAddress = normalizedMacAddress,
                Manufacturer = request.Manufacturer.Trim(),
                RegisteredAtUtc = DateTime.UtcNow
            };

            await _sensorRepository.AddDeviceAsync(
                device,
                cancellationToken);
        }

        var sensor = new Sensor
        {
            Device = device,
            Name = request.SensorName.Trim(),
            UniqueIdentifier = normalizedIdentifier,
            DeploymentLocation =
                request.DeploymentLocation.Trim(),
            Category = request.Category,
            Status = SensorStatus.Online,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _sensorRepository.AddSensorAsync(
            sensor,
            cancellationToken);

        await _sensorRepository.SaveChangesAsync(
            cancellationToken);

        /*
         * Populate the Part 2 Dictionary only after persistence succeeds.
         * The sensor identifier is also removed from the disconnected HashSet.
         */
        _operationalLookupService.CacheDevice(device);
        _operationalLookupService.MarkConnected(
            sensor.UniqueIdentifier);

        return MapToResponse(sensor);
    }

    private void CacheDevice(Device? device)
    {
        if (device is null
            || string.IsNullOrWhiteSpace(device.MacAddress))
        {
            return;
        }

        _operationalLookupService.CacheDevice(device);
    }

    private void SynchronizeConnectionState(
        string sensorIdentifier,
        SensorStatus status)
    {
        if (status == SensorStatus.Offline)
        {
            _operationalLookupService.MarkDisconnected(
                sensorIdentifier);

            return;
        }

        _operationalLookupService.MarkConnected(
            sensorIdentifier);
    }

    private static SensorResponse MapToResponse(Sensor sensor)
    {
        return new SensorResponse
        {
            Id = sensor.Id,
            DeviceId = sensor.DeviceId,
            DeviceName = sensor.Device?.Name ?? string.Empty,
            MacAddress = sensor.Device?.MacAddress ?? string.Empty,
            Manufacturer =
                sensor.Device?.Manufacturer ?? string.Empty,
            SensorName = sensor.Name,
            UniqueIdentifier = sensor.UniqueIdentifier,
            DeploymentLocation = sensor.DeploymentLocation,
            Category = sensor.Category,
            Status = sensor.Status,
            CreatedAtUtc = sensor.CreatedAtUtc,
            LastReadingAtUtc = sensor.LastReadingAtUtc
        };
    }
}