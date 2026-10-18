using NexaGrid.Shared.DTOs;
using NexaGrid.Shared.DTOs.Predictive;

namespace NexaGrid.Desktop.Services;

public sealed class PredictiveActionApiService
{
    private readonly ApiClient _apiClient;

    public PredictiveActionApiService(ApiClient apiClient)
    {
        _apiClient =
            apiClient
            ?? throw new ArgumentNullException(nameof(apiClient));
    }

    public async Task<int> RecordActivityAsync(
        RecordUserActivityRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ApiResponse<int>? response =
            await _apiClient.PostAsync<
                RecordUserActivityRequest,
                ApiResponse<int>>(
                "/api/predictive-actions/activity",
                request,
                cancellationToken);

        if (response is null)
        {
            throw new ApiException(
                "The API returned an empty activity response.",
                500);
        }

        if (!response.Success)
        {
            throw new ApiException(response.Message, 400);
        }

        return response.Data;
    }

    public async Task<IReadOnlyList<PredictiveSuggestionResponse>>
        GetSuggestionsAsync(
            string operatorIdentifier,
            int activeAlertCount = 0,
            CancellationToken cancellationToken = default)
    {
        ValidateOperatorIdentifier(operatorIdentifier);

        if (activeAlertCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(activeAlertCount),
                "The active alert count cannot be negative.");
        }

        string encodedOperator =
            Uri.EscapeDataString(operatorIdentifier.Trim());

        string endpoint =
            "/api/predictive-actions/suggestions" +
            $"?operatorIdentifier={encodedOperator}" +
            $"&activeAlertCount={activeAlertCount}";

        ApiResponse<List<PredictiveSuggestionResponse>>? response =
            await _apiClient.GetAsync<
                ApiResponse<List<PredictiveSuggestionResponse>>>(
                endpoint,
                cancellationToken);

        if (response is null)
        {
            throw new ApiException(
                "The API returned an empty predictive-suggestion response.",
                500);
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