using NexaGrid.Shared.Models;

namespace NexaGrid.API.Services;
/*CodeProject (2018)*/
public sealed class OperationalLookupService : IOperationalLookupService
{
    private readonly Lock _syncRoot = new();

    private readonly Dictionary<string, Device> _deviceCache =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly SortedDictionary<DateTime, TelemetryRecord>
        _telemetryTimeline = new();

    private readonly HashSet<string> _disconnectedNodes =
        new(StringComparer.OrdinalIgnoreCase);
/*CodeProject (2018)*/
    public int CachedDeviceCount
    {
        get
        {
            lock (_syncRoot)
            {
                return _deviceCache.Count;
            }
        }
    }
/*CodeProject (2018)*/
    public int TimelineCount
    {
        get
        {
            lock (_syncRoot)
            {
                return _telemetryTimeline.Count;
            }
        }
    }
/*CodeProject (2018)*/
    public int DisconnectedNodeCount
    {
        get
        {
            lock (_syncRoot)
            {
                return _disconnectedNodes.Count;
            }
        }
    }
/*CodeProject (2018)*/
    public void CacheDevice(Device device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var macAddress = device.MacAddress.Trim();

        if (string.IsNullOrWhiteSpace(macAddress))
        {
            throw new ArgumentException(
                "A device MAC address is required.",
                nameof(device));
        }

        lock (_syncRoot)
        {
            _deviceCache[macAddress] = device;
        }
    }
/*CodeProject (2018)*/
    public bool TryGetDevice(string macAddress, out Device? device)
    {
        if (string.IsNullOrWhiteSpace(macAddress))
        {
            device = null;
            return false;
        }

        lock (_syncRoot)
        {
            return _deviceCache.TryGetValue(macAddress.Trim(), out device);
        }
    }
/*CodeProject (2018)*/
    public void RecordTelemetry(TelemetryRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        lock (_syncRoot)
        {
            var timelineKey = record.RecordedAtUtc;

            while (_telemetryTimeline.ContainsKey(timelineKey))
            {
                timelineKey = timelineKey.AddTicks(1);
            }

            _telemetryTimeline.Add(timelineKey, record);
        }
    }
/*CodeProject (2018)*/
    public IReadOnlyList<TelemetryRecord> GetTelemetryTimeline(
        DateTime? fromUtc = null,
        DateTime? toUtc = null)
    {
        lock (_syncRoot)
        {
            IEnumerable<KeyValuePair<DateTime, TelemetryRecord>> query =
                _telemetryTimeline;

            if (fromUtc.HasValue)
            {
                query = query.Where(item => item.Key >= fromUtc.Value);
            }

            if (toUtc.HasValue)
            {
                query = query.Where(item => item.Key <= toUtc.Value);
            }

            return query
                .Select(item => item.Value)
                .ToArray();
        }
    }
/*CodeProject (2018)*/
    public bool MarkDisconnected(string nodeIdentifier)
    {
        var normalizedIdentifier = NormalizeIdentifier(nodeIdentifier);

        lock (_syncRoot)
        {
            return _disconnectedNodes.Add(normalizedIdentifier);
        }
    }
/*CodeProject (2018)*/
    public bool MarkConnected(string nodeIdentifier)
    {
        var normalizedIdentifier = NormalizeIdentifier(nodeIdentifier);

        lock (_syncRoot)
        {
            return _disconnectedNodes.Remove(normalizedIdentifier);
        }
    }
/*CodeProject (2018)*/
    public bool IsDisconnected(string nodeIdentifier)
    {
        if (string.IsNullOrWhiteSpace(nodeIdentifier))
        {
            return false;
        }

        lock (_syncRoot)
        {
            return _disconnectedNodes.Contains(nodeIdentifier.Trim());
        }
    }
/*CodeProject (2018)*/
    public IReadOnlyList<string> GetDisconnectedNodes()
    {
        lock (_syncRoot)
        {
            return _disconnectedNodes
                .OrderBy(identifier => identifier)
                .ToArray();
        }
    }
/*CodeProject (2018)*/
    private static string NormalizeIdentifier(string nodeIdentifier)
    {
        if (string.IsNullOrWhiteSpace(nodeIdentifier))
        {
            throw new ArgumentException(
                "A node identifier is required.",
                nameof(nodeIdentifier));
        }

        return nodeIdentifier.Trim();
    }
}