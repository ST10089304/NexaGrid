using NexaGrid.Shared.Models;

namespace NexaGrid.API.Services;
/*CodeProject (2018)*/
public interface IOperationalLookupService
{
    int CachedDeviceCount { get; }
    int TimelineCount { get; }
    int DisconnectedNodeCount { get; }

    void CacheDevice(Device device);
    bool TryGetDevice(string macAddress, out Device? device);

    void RecordTelemetry(TelemetryRecord record);
    IReadOnlyList<TelemetryRecord> GetTelemetryTimeline(
        DateTime? fromUtc = null,
        DateTime? toUtc = null);

    bool MarkDisconnected(string nodeIdentifier);
    bool MarkConnected(string nodeIdentifier);
    bool IsDisconnected(string nodeIdentifier);
    IReadOnlyList<string> GetDisconnectedNodes();
}