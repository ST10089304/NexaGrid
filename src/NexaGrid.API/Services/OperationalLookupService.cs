using NexaGrid.Shared.Models;

namespace NexaGrid.API.Services;

public sealed class OperationalLookupService : IOperationalLookupService
{
    private readonly Lock _syncRoot = new();

    private readonly Dictionary<string, Device> _deviceCache =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly SortedDictionary<DateTime, TelemetryRecord>
        _telemetryTimeline = new();

    private readonly HashSet<string> _disconnectedNodes =
        new(StringComparer.OrdinalIgnoreCase);

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

    public bool MarkDisconnected(string nodeIdentifier)
    {
        var normalizedIdentifier = NormalizeIdentifier(nodeIdentifier);

        lock (_syncRoot)
        {
            return _disconnectedNodes.Add(normalizedIdentifier);
        }
    }

    public bool MarkConnected(string nodeIdentifier)
    {
        var normalizedIdentifier = NormalizeIdentifier(nodeIdentifier);

        lock (_syncRoot)
        {
            return _disconnectedNodes.Remove(normalizedIdentifier);
        }
    }

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

    public IReadOnlyList<string> GetDisconnectedNodes()
    {
        lock (_syncRoot)
        {
            return _disconnectedNodes
                .OrderBy(identifier => identifier)
                .ToArray();
        }
    }

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