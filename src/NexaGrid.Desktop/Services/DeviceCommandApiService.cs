using NexaGrid.Shared.DTOs;
using NexaGrid.Shared.DTOs.CommandStream;

namespace NexaGrid.Desktop.Services;

public sealed class DeviceCommandApiService
{
    private readonly ApiClient _apiClient;

    public DeviceCommandApiService(ApiClient apiClient)
    {
        _apiClient =
            apiClient
            ?? throw new ArgumentNullException(nameof(apiClient));
    }

    public async Task<DeviceCommandResponse> IssueAsync(
        IssueDeviceCommandRequest request,
        string operatorIdentifier,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateOperatorIdentifier(operatorIdentifier);

        string encodedOperator =
            Uri.EscapeDataString(operatorIdentifier.Trim());

        string endpoint =
            $"/api/device-commands?operatorIdentifier={encodedOperator}";

        ApiResponse<DeviceCommandResponse>? response =
            await _apiClient.PostAsync<
                IssueDeviceCommandRequest,
                ApiResponse<DeviceCommandResponse>>(
                endpoint,
                request,
                cancellationToken);

        return RequireCommand(
            response,
            "The API did not return the queued command.");
    }

    public async Task<IReadOnlyList<DeviceCommandResponse>>
        GetPendingAsync(
            CancellationToken cancellationToken = default)
    {
        ApiResponse<List<DeviceCommandResponse>>? response =
            await _apiClient.GetAsync<
                ApiResponse<List<DeviceCommandResponse>>>(
                "/api/device-commands/pending",
                cancellationToken);

        return RequireList(
            response,
            "The API returned an empty pending-command response.");
    }

    public async Task<DeviceCommandResponse> ProcessNextAsync(
        CancellationToken cancellationToken = default)
    {
        ApiResponse<DeviceCommandResponse>? response =
            await _apiClient.PostAsync<
                object,
                ApiResponse<DeviceCommandResponse>>(
                "/api/device-commands/process-next",
                new { },
                cancellationToken);

        return RequireCommand(
            response,
            "The API did not return the processed command.");
    }

    public async Task<DeviceCommandResponse> UndoLastAsync(
        CancellationToken cancellationToken = default)
    {
        ApiResponse<DeviceCommandResponse>? response =
            await _apiClient.PostAsync<
                object,
                ApiResponse<DeviceCommandResponse>>(
                "/api/device-commands/undo-last",
                new { },
                cancellationToken);

        return RequireCommand(
            response,
            "The API did not return the undone command.");
    }

    public async Task<IReadOnlyList<DeviceCommandResponse>>
        GetHistoryAsync(
            CancellationToken cancellationToken = default)
    {
        ApiResponse<List<DeviceCommandResponse>>? response =
            await _apiClient.GetAsync<
                ApiResponse<List<DeviceCommandResponse>>>(
                "/api/device-commands/history",
                cancellationToken);

        return RequireList(
            response,
            "The API returned an empty command-history response.");
    }

    private static DeviceCommandResponse RequireCommand(
        ApiResponse<DeviceCommandResponse>? response,
        string emptyDataMessage)
    {
        if (response is null)
        {
            throw new ApiException(
                "The API returned an empty command response.",
                500);
        }

        if (!response.Success)
        {
            throw new ApiException(response.Message, 400);
        }

        return response.Data
            ?? throw new ApiException(emptyDataMessage, 500);
    }

    private static IReadOnlyList<DeviceCommandResponse> RequireList(
        ApiResponse<List<DeviceCommandResponse>>? response,
        string emptyResponseMessage)
    {
        if (response is null)
        {
            throw new ApiException(emptyResponseMessage, 500);
        }

        if (!response.Success)
        {
            throw new ApiException(response.Message, 400);
        }

        return response.Data ?? [];
    }

    private static void ValidateOperatorIdentifier(
        string operatorIdentifier)
    {
        if (string.IsNullOrWhiteSpace(operatorIdentifier))
        {
            throw new ArgumentException(
                "The operator identifier is required.",
                nameof(operatorIdentifier));
        }
    }
}