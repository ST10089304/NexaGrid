using NexaGrid.Shared.DTOs;

namespace NexaGrid.Desktop.Services;

public class SensorApiService
{
    private readonly ApiClient _apiClient;

    public SensorApiService(
        ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<
        IReadOnlyList<SensorResponse>>
        GetAllAsync(
            CancellationToken cancellationToken = default)
    {
        ApiResponse<List<SensorResponse>>? response =
            await _apiClient.GetAsync<
                ApiResponse<List<SensorResponse>>>(
                "/api/sensors",
                cancellationToken);

        return response?.Data
            ?? [];
    }

    public async Task<SensorResponse> RegisterAsync(
        RegisterSensorRequest request,
        CancellationToken cancellationToken = default)
    {
        ApiResponse<SensorResponse>? response =
            await _apiClient.PostAsync<
                RegisterSensorRequest,
                ApiResponse<SensorResponse>>(
                "/api/sensors",
                request,
                cancellationToken);

        return response?.Data
            ?? throw new ApiException(
                "The API did not return the registered sensor.",
                500);
    }
}