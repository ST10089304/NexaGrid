using NexaGrid.Shared.DTOs.Predictive;

namespace NexaGrid.API.Services;

public interface IPredictiveActionService
{
    void RecordActivity(RecordUserActivityRequest request);

    IReadOnlyList<PredictiveSuggestionResponse> GenerateSuggestions(
        string operatorIdentifier,
        int activeAlertCount = 0);

    int GetRecordedActivityCount(string operatorIdentifier);
}