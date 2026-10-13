using NexaGrid.API.Services;
using NexaGrid.Shared.Models;

namespace NexaGrid.API.Tests;

public sealed class OperationalLookupServiceTests
{
    [Fact]
    public void CacheDevice_ProvidesCaseInsensitiveConstantTimeLookup()
    {
        var service = new OperationalLookupService();

        var device = new Device
        {
            Id = 1,
            Name = "Greenhouse Gateway",
            MacAddress = "AA:BB:CC:DD:EE:01",
            Manufacturer = "NexaGrid Labs"
        };

        service.CacheDevice(device);

        var found = service.TryGetDevice(
            "aa:bb:cc:dd:ee:01",
            out var cachedDevice);

        Assert.True(found);
        Assert.NotNull(cachedDevice);
        Assert.Equal(device.Id, cachedDevice.Id);
        Assert.Equal(1, service.CachedDeviceCount);
    }

    [Fact]
    public void CacheDevice_ReplacesExistingDeviceWithSameMacAddress()
    {
        var service = new OperationalLookupService();

        service.CacheDevice(new Device
        {
            Id = 1,
            Name = "Original Gateway",
            MacAddress = "AA:BB:CC:DD:EE:01"
        });

        service.CacheDevice(new Device
        {
            Id = 1,
            Name = "Updated Gateway",
            MacAddress = "aa:bb:cc:dd:ee:01"
        });

        Assert.True(service.TryGetDevice(
            "AA:BB:CC:DD:EE:01",
            out var result));

        Assert.Equal("Updated Gateway", result!.Name);
        Assert.Equal(1, service.CachedDeviceCount);
    }

    [Fact]
    public void GetTelemetryTimeline_ReturnsRecordsChronologically()
    {
        var service = new OperationalLookupService();

        var later = CreateTelemetry(
            2,
            new DateTime(2026, 10, 13, 9, 30, 0, DateTimeKind.Utc));

        var earlier = CreateTelemetry(
            1,
            new DateTime(2026, 10, 13, 9, 0, 0, DateTimeKind.Utc));

        service.RecordTelemetry(later);
        service.RecordTelemetry(earlier);

        var timeline = service.GetTelemetryTimeline();

        Assert.Equal(2, timeline.Count);
        Assert.Equal(earlier.Id, timeline[0].Id);
        Assert.Equal(later.Id, timeline[1].Id);
    }

    [Fact]
    public void DisconnectedNodes_RemainUniqueRegardlessOfLetterCase()
    {
        var service = new OperationalLookupService();

        var firstAddition = service.MarkDisconnected("HUM-001");
        var duplicateAddition = service.MarkDisconnected("hum-001");

        Assert.True(firstAddition);
        Assert.False(duplicateAddition);
        Assert.True(service.IsDisconnected("Hum-001"));
        Assert.Equal(1, service.DisconnectedNodeCount);
    }

    [Fact]
    public void MarkConnected_RemovesNodeFromDisconnectedSet()
    {
        var service = new OperationalLookupService();

        service.MarkDisconnected("TEMP-001");

        var removed = service.MarkConnected("temp-001");

        Assert.True(removed);
        Assert.False(service.IsDisconnected("TEMP-001"));
        Assert.Empty(service.GetDisconnectedNodes());
    }

    private static TelemetryRecord CreateTelemetry(
        long id,
        DateTime recordedAtUtc)
    {
        return new TelemetryRecord
        {
            Id = id,
            SensorId = 1,
            DataType = "Float",
            Value = id.ToString(),
            Unit = "Celsius",
            RecordedAtUtc = recordedAtUtc
        };
    }
}