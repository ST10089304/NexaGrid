using NexaGrid.API.Repositories;
using NexaGrid.Shared.DTOs;
using NexaGrid.Shared.Enums;
using NexaGrid.Shared.Models;

namespace NexaGrid.API.Services;
/*CodeProject (2018)*/
public class SensorService : ISensorService
{
    private readonly ISensorRepository _sensorRepository;

    public SensorService(ISensorRepository sensorRepository)
    {
        _sensorRepository = sensorRepository;
    }
/*CodeProject (2018)*/
    public async Task<IReadOnlyList<SensorResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        List<Sensor> sensors =
            await _sensorRepository.GetAllAsync(cancellationToken);

        return sensors
            .Select(MapToResponse)
            .ToList();
    }
/*CodeProject (2018)*/
    public async Task<SensorResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        Sensor? sensor =
            await _sensorRepository.GetByIdAsync(
                id,
                cancellationToken);

        return sensor is null ? null : MapToResponse(sensor);
    }
/*CodeProject (2018)*/
    public async Task<SensorResponse> RegisterAsync(
        RegisterSensorRequest request,
        CancellationToken cancellationToken = default)
    {
        string normalizedIdentifier =
            request.UniqueIdentifier.Trim().ToUpperInvariant();

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

        return MapToResponse(sensor);
    }
/*CodeProject (2018)*/
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