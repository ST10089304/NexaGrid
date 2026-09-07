using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using NexaGrid.API.Repositories;
using NexaGrid.Shared.DTOs;
using NexaGrid.Shared.Enums;
using NexaGrid.Shared.Generics;
using NexaGrid.Shared.Models;
using NexaGrid.Shared.Structures;

namespace NexaGrid.API.Services;
/*CodeProject (2018)*/
public class TelemetryService : ITelemetryService
{
    private readonly ITelemetryRepository _telemetryRepository;

    public TelemetryService(
        ITelemetryRepository telemetryRepository)
    {
        _telemetryRepository = telemetryRepository;
    }
/*CodeProject (2018)*/
    public async Task<TelemetryResponse> IngestAsync(
        IngestTelemetryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Value.ValueKind is
            JsonValueKind.Undefined or JsonValueKind.Null)
        {
            throw new ArgumentException(
                "A telemetry value is required.");
        }

        Sensor? sensor =
            await _telemetryRepository.GetSensorByIdentifierAsync(
                request.SensorIdentifier,
                cancellationToken);

        if (sensor is null)
        {
            throw new KeyNotFoundException(
                $"Sensor '{request.SensorIdentifier}' was not found.");
        }

        DateTime recordedAtUtc =
            request.RecordedAtUtc?.ToUniversalTime()
            ?? DateTime.UtcNow;

        ProcessedTelemetry processed =
            ProcessTypedValue(
                request,
                recordedAtUtc);

        var record = new TelemetryRecord
        {
            SensorId = sensor.Id,
            DataType = request.DataType.ToString(),
            Value = processed.StoredValue,
            Unit = request.Unit.Trim(),
            IsAnomaly = processed.IsAnomaly,
            RecordedAtUtc = recordedAtUtc
        };

        sensor.LastReadingAtUtc = recordedAtUtc;

        sensor.Status = processed.IsAnomaly
            ? SensorStatus.Warning
            : SensorStatus.Online;

        await _telemetryRepository.AddAsync(
            record,
            cancellationToken);

        await _telemetryRepository.SaveChangesAsync(
            cancellationToken);

        return MapToResponse(
            record,
            sensor.UniqueIdentifier);
    }
/*CodeProject (2018)*/
    public async Task<IReadOnlyList<TelemetryResponse>> GetRecentAsync(
        string sensorIdentifier,
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sensorIdentifier))
        {
            throw new ArgumentException(
                "The sensor identifier is required.");
        }

        if (limit is < 1 or > 500)
        {
            throw new ArgumentException(
                "The telemetry limit must be between 1 and 500.");
        }

        List<TelemetryRecord> records =
            await _telemetryRepository.GetRecentAsync(
                sensorIdentifier,
                limit,
                cancellationToken);

        return records
            .Select(record =>
                MapToResponse(
                    record,
                    record.Sensor?.UniqueIdentifier
                        ?? sensorIdentifier))
            .ToList();
    }
/*CodeProject (2018)*/
    public async Task<TelemetrySeedResponse> SeedAsync(
        string sensorIdentifier,
        int count,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sensorIdentifier))
        {
            throw new ArgumentException(
                "The sensor identifier is required.");
        }

        if (count is < 1 or > 10_000)
        {
            throw new ArgumentException(
                "The generated reading count must be between 1 and 10,000.");
        }

        Sensor? sensor =
            await _telemetryRepository.GetSensorByIdentifierAsync(
                sensorIdentifier,
                cancellationToken);

        if (sensor is null)
        {
            throw new KeyNotFoundException(
                $"Sensor '{sensorIdentifier}' was not found.");
        }

        const int batchSize = 250;

        DateTime startedAtUtc = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        var recentBuffer =
            new TelemetryBuffer<TelemetryPacket<float>>(
                Math.Min(count, 1_000));

        var generatedPackets =
            new List<TelemetryPacket<float>>(count);

        for (int index = 0; index < count; index++)
        {
            bool createAnomaly =
                index > 0 && index % 25 == 0;

            float value = createAnomaly
                ? Random.Shared.NextSingle() * 15f + 40f
                : Random.Shared.NextSingle() * 8f + 20f;

            var packet = new TelemetryPacket<float>
            {
                SensorIdentifier = sensor.UniqueIdentifier,
                Value = MathF.Round(value, 2),
                Unit = "Celsius",
                RecordedAtUtc = startedAtUtc.AddSeconds(index),
                IsAnomaly = createAnomaly
            };

            generatedPackets.Add(packet);
            recentBuffer.Add(packet);
        }

        TelemetryPacket<float>[][] jaggedBatches =
            TelemetryBatchProcessor.CreateBatches(
                generatedPackets,
                batchSize);

        List<TelemetryPacket<float>> optimisedPackets =
            TelemetryBatchProcessor.FlattenToList(
                jaggedBatches);

        List<TelemetryRecord> records =
            optimisedPackets
                .Select(packet => new TelemetryRecord
                {
                    SensorId = sensor.Id,
                    DataType = TelemetryDataType.Float.ToString(),
                    Value = packet.Value.ToString(
                        CultureInfo.InvariantCulture),
                    Unit = packet.Unit,
                    IsAnomaly = packet.IsAnomaly,
                    RecordedAtUtc = packet.RecordedAtUtc
                })
                .ToList();

        await _telemetryRepository.AddRangeAsync(
            records,
            cancellationToken);

        sensor.LastReadingAtUtc =
            records[^1].RecordedAtUtc;

        sensor.Status = records.Any(
            record => record.IsAnomaly)
            ? SensorStatus.Warning
            : SensorStatus.Online;

        await _telemetryRepository.SaveChangesAsync(
            cancellationToken);

        stopwatch.Stop();

        return new TelemetrySeedResponse
        {
            SensorIdentifier = sensor.UniqueIdentifier,
            RequestedReadingCount = count,
            CreatedReadingCount = records.Count,
            BatchCount = jaggedBatches.Length,
            BatchSize = batchSize,
            AnomalyCount = records.Count(
                record => record.IsAnomaly),
            ProcessingTimeMilliseconds = Math.Round(
                stopwatch.Elapsed.TotalMilliseconds,
                2),
            StartedAtUtc = startedAtUtc,
            CompletedAtUtc = DateTime.UtcNow
        };
    }
/*CodeProject (2018)*/
    private static ProcessedTelemetry ProcessTypedValue(
        IngestTelemetryRequest request,
        DateTime recordedAtUtc)
    {
        return request.DataType switch
        {
            TelemetryDataType.Float =>
                ProcessFloat(
                    request,
                    recordedAtUtc),

            TelemetryDataType.Integer =>
                ProcessInteger(
                    request,
                    recordedAtUtc),

            TelemetryDataType.Boolean =>
                ProcessBoolean(
                    request,
                    recordedAtUtc),

            _ => throw new ArgumentException(
                "The telemetry data type is unsupported.")
        };
    }
/*CodeProject (2018)*/
    private static ProcessedTelemetry ProcessFloat(
        IngestTelemetryRequest request,
        DateTime recordedAtUtc)
    {
        if (request.Value.ValueKind != JsonValueKind.Number
            || !request.Value.TryGetSingle(out float value))
        {
            throw new ArgumentException(
                "The telemetry value must be a valid float.");
        }

        var packet = new TelemetryPacket<float>
        {
            SensorIdentifier =
                request.SensorIdentifier
                    .Trim()
                    .ToUpperInvariant(),
            Value = value,
            Unit = request.Unit.Trim(),
            RecordedAtUtc = recordedAtUtc
        };

        packet.IsAnomaly = IsOutsideRange(
            value,
            request.MinimumAcceptableValue,
            request.MaximumAcceptableValue);

        return new ProcessedTelemetry(
            packet.Value.ToString(
                CultureInfo.InvariantCulture),
            packet.IsAnomaly);
    }
/*CodeProject (2018)*/
    private static ProcessedTelemetry ProcessInteger(
        IngestTelemetryRequest request,
        DateTime recordedAtUtc)
    {
        if (request.Value.ValueKind != JsonValueKind.Number
            || !request.Value.TryGetInt32(out int value))
        {
            throw new ArgumentException(
                "The telemetry value must be a valid integer.");
        }

        var packet = new TelemetryPacket<int>
        {
            SensorIdentifier =
                request.SensorIdentifier
                    .Trim()
                    .ToUpperInvariant(),
            Value = value,
            Unit = request.Unit.Trim(),
            RecordedAtUtc = recordedAtUtc
        };

        packet.IsAnomaly = IsOutsideRange(
            value,
            request.MinimumAcceptableValue,
            request.MaximumAcceptableValue);

        return new ProcessedTelemetry(
            packet.Value.ToString(
                CultureInfo.InvariantCulture),
            packet.IsAnomaly);
    }
/*CodeProject (2018)*/
    private static ProcessedTelemetry ProcessBoolean(
        IngestTelemetryRequest request,
        DateTime recordedAtUtc)
    {
        if (request.Value.ValueKind is not
            JsonValueKind.True and not JsonValueKind.False)
        {
            throw new ArgumentException(
                "The telemetry value must be true or false.");
        }

        bool value = request.Value.GetBoolean();

        var packet = new TelemetryPacket<bool>
        {
            SensorIdentifier =
                request.SensorIdentifier
                    .Trim()
                    .ToUpperInvariant(),
            Value = value,
            Unit = request.Unit.Trim(),
            RecordedAtUtc = recordedAtUtc
        };

        packet.IsAnomaly =
            request.ExpectedBooleanValue.HasValue
            && value != request.ExpectedBooleanValue.Value;

        return new ProcessedTelemetry(
            packet.Value
                .ToString()
                .ToLowerInvariant(),
            packet.IsAnomaly);
    }
/*CodeProject (2018)*/
    private static bool IsOutsideRange(
        double value,
        double? minimum,
        double? maximum)
    {
        bool belowMinimum =
            minimum.HasValue
            && value < minimum.Value;

        bool aboveMaximum =
            maximum.HasValue
            && value > maximum.Value;

        return belowMinimum || aboveMaximum;
    }
/*CodeProject (2018)*/
    private static TelemetryResponse MapToResponse(
        TelemetryRecord record,
        string sensorIdentifier)
    {
        Enum.TryParse(
            record.DataType,
            ignoreCase: true,
            out TelemetryDataType dataType);

        return new TelemetryResponse
        {
            Id = record.Id,
            SensorId = record.SensorId,
            SensorIdentifier =
                sensorIdentifier
                    .Trim()
                    .ToUpperInvariant(),
            DataType = dataType,
            Value = record.Value,
            Unit = record.Unit,
            IsAnomaly = record.IsAnomaly,
            StatusMessage = record.IsAnomaly
                ? "Anomaly detected - investigate this reading."
                : "Reading is within the expected range.",
            RecordedAtUtc = record.RecordedAtUtc
        };
    }
/*CodeProject (2018)*/
    private readonly record struct ProcessedTelemetry(
        string StoredValue,
        bool IsAnomaly);
}
