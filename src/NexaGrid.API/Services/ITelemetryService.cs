using NexaGrid.Shared.DTOs;
using System.Diagnostics;
using NexaGrid.Shared.Structures;

namespace NexaGrid.API.Services;

public interface ITelemetryService
{
    Task<TelemetryResponse> IngestAsync(
        IngestTelemetryRequest request,
        CancellationToken cancellationToken = default);
Task<TelemetrySeedResponse> SeedAsync(
    string sensorIdentifier,
    int count,
    CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TelemetryResponse>> GetRecentAsync(
        string sensorIdentifier,
        int limit,
        CancellationToken cancellationToken = default);
}