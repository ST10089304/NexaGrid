using NexaGrid.Shared.DTOs;

namespace NexaGrid.Desktop.Services;

public sealed class TelemetryApiService
{
    private readonly ApiClient _apiClient;

    public TelemetryApiService(
        ApiClient apiClient)
    {
        _apiClient =
            apiClient
            ?? throw new ArgumentNullException(
                nameof(apiClient));
    }

    public async Task<TelemetryResponse> IngestAsync(
        IngestTelemetryRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ApiResponse<TelemetryResponse>? response =
            await _apiClient.PostAsync<
                IngestTelemetryRequest,
                ApiResponse<TelemetryResponse>>(
                "/api/telemetry",
                request,
                cancellationToken);

        if (response is null)
        {
            throw new ApiException(
                "The API returned an empty telemetry response.",
                500);
        }

        if (!response.Success)
        {
            throw new ApiException(
                response.Message,
                400);
        }

        if (response.Data is null)
        {
            throw new ApiException(
                "The telemetry reading was accepted, but no data was returned.",
                500);
        }

        return response.Data;
    }

    public async Task<IReadOnlyList<TelemetryResponse>>
        GetRecentAsync(
            string sensorIdentifier,
            int limit = 50,
            CancellationToken cancellationToken = default)
    {
        ValidateSensorIdentifier(
            sensorIdentifier);

        if (limit is < 1 or > 1_000)
        {
            throw new ArgumentOutOfRangeException(
                nameof(limit),
                "The telemetry limit must be between 1 and 1,000.");
        }

        string encodedIdentifier =
            Uri.EscapeDataString(
                sensorIdentifier.Trim());

        string endpoint =
            $"/api/telemetry/sensor/{encodedIdentifier}?limit={limit}";

        ApiResponse<List<TelemetryResponse>>? response =
            await _apiClient.GetAsync<
                ApiResponse<List<TelemetryResponse>>>(
                endpoint,
                cancellationToken);

        if (response is null)
        {
            throw new ApiException(
                "The API returned an empty telemetry-history response.",
                500);
        }

        if (!response.Success)
        {
            throw new ApiException(
                response.Message,
                400);
        }

        return response.Data
            ?? new List<TelemetryResponse>();
    }

    public async Task<TelemetrySeedResponse> SeedAsync(
        string sensorIdentifier,
        int count,
        CancellationToken cancellationToken = default)
    {
        ValidateSensorIdentifier(
            sensorIdentifier);

        if (count is < 1 or > 10_000)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count),
                "The seed count must be between 1 and 10,000.");
        }

        string encodedIdentifier =
            Uri.EscapeDataString(
                sensorIdentifier.Trim());

        string endpoint =
            $"/api/telemetry/seed/{encodedIdentifier}?count={count}";

        ApiResponse<TelemetrySeedResponse>? response =
            await _apiClient.PostAsync<
                object,
                ApiResponse<TelemetrySeedResponse>>(
                endpoint,
                new { },
                cancellationToken);

        if (response is null)
        {
            throw new ApiException(
                "The API returned an empty telemetry-seed response.",
                500);
        }

        if (!response.Success)
        {
            throw new ApiException(
                response.Message,
                400);
        }

        if (response.Data is null)
        {
            throw new ApiException(
                "Telemetry was generated, but no processing summary was returned.",
                500);
        }

        return response.Data;
    }

    public Task<bool> IsApiHealthyAsync(
        CancellationToken cancellationToken = default)
    {
        return _apiClient.IsHealthyAsync(
            cancellationToken);
    }

    private static void ValidateSensorIdentifier(
        string sensorIdentifier)
    {
        if (string.IsNullOrWhiteSpace(
                sensorIdentifier))
        {
            throw new ArgumentException(
                "A sensor identifier must be selected.",
                nameof(sensorIdentifier));
        }
    }
}