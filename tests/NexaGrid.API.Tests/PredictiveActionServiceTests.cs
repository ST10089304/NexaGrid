using NexaGrid.API.Services;
using NexaGrid.Shared.DTOs.Predictive;
using NexaGrid.Shared.Enums;

namespace NexaGrid.API.Tests;

public sealed class PredictiveActionServiceTests
{
    [Fact]
    public void GenerateSuggestions_BelowThreshold_DoesNotSuggestPattern()
    {
        var service = CreateService();

        RecordActivity(
            service,
            "operator-1",
            UserActivityType.Search,
            "Warning");

        RecordActivity(
            service,
            "operator-1",
            UserActivityType.Search,
            "Warning");

        var suggestions = service.GenerateSuggestions("operator-1");

        Assert.Empty(suggestions);
    }

    [Fact]
    public void GenerateSuggestions_ThreeRepeatedSearches_ReturnsSearchSuggestion()
    {
        var service = CreateService();

        for (var count = 0; count < 3; count++)
        {
            RecordActivity(
                service,
                "operator-1",
                UserActivityType.Search,
                "Environmental");
        }

        var suggestion = Assert.Single(
            service.GenerateSuggestions("operator-1"));

        Assert.Equal("SEARCH-ENVIRONMENTAL", suggestion.Code);
        Assert.Equal("Reuse frequent search", suggestion.Title);
        Assert.Equal(CommandPriority.Normal, suggestion.RecommendedPriority);
        Assert.Equal(3, suggestion.SupportingActivityCount);
        Assert.Null(suggestion.RecommendedCommandType);
        Assert.True(suggestion.ConfidenceScore > 0);
    }

    [Fact]
    public void GenerateSuggestions_ThreeRepeatedCommands_ReturnsCommandSuggestion()
    {
        var service = CreateService();

        for (var count = 0; count < 3; count++)
        {
            RecordActivity(
                service,
                "operator-1",
                UserActivityType.Command,
                DeviceCommandType.Reset.ToString(),
                "HUM-001");
        }

        var suggestion = Assert.Single(
            service.GenerateSuggestions("operator-1"));

        Assert.Equal("COMMAND-RESET", suggestion.Code);
        Assert.Equal("Repeat frequent device action", suggestion.Title);
        Assert.Equal("HUM-001", suggestion.TargetIdentifier);
        Assert.Equal(
            DeviceCommandType.Reset,
            suggestion.RecommendedCommandType);
        Assert.Equal(CommandPriority.High, suggestion.RecommendedPriority);
        Assert.Equal(3, suggestion.SupportingActivityCount);
    }

    [Fact]
    public void GenerateSuggestions_IsPersonalizedPerOperator()
    {
        var service = CreateService();

        for (var count = 0; count < 3; count++)
        {
            RecordActivity(
                service,
                "operator-a",
                UserActivityType.Search,
                "Offline");
        }

        var operatorASuggestions =
            service.GenerateSuggestions("operator-a");

        var operatorBSuggestions =
            service.GenerateSuggestions("operator-b");

        Assert.Single(operatorASuggestions);
        Assert.Empty(operatorBSuggestions);
    }

    [Fact]
    public void GenerateSuggestions_ActiveAlertThreshold_RanksAlertFirst()
    {
        var service = CreateService();

        for (var count = 0; count < 3; count++)
        {
            RecordActivity(
                service,
                "operator-1",
                UserActivityType.Command,
                DeviceCommandType.Reset.ToString(),
                "HUM-001");
        }

        var suggestions = service.GenerateSuggestions(
            "operator-1",
            activeAlertCount: 4);

        Assert.Equal(2, suggestions.Count);
        Assert.Equal("ACTIVE-ALERT-TRIAGE", suggestions[0].Code);
        Assert.Equal(
            CommandPriority.Critical,
            suggestions[0].RecommendedPriority);
        Assert.Equal("COMMAND-RESET", suggestions[1].Code);
        Assert.Equal(
            CommandPriority.High,
            suggestions[1].RecommendedPriority);
    }

    [Fact]
    public void GenerateSuggestions_DisconnectedNode_ReturnsRestoreRecommendation()
    {
        var lookupService = new OperationalLookupService();
        lookupService.MarkDisconnected("gateway-001");

        var service = new PredictiveActionService(lookupService);

        var suggestion = Assert.Single(
            service.GenerateSuggestions("operator-1"));

        Assert.Equal(
            "DISCONNECTED-gateway-001",
            suggestion.Code);

        Assert.Equal(
            "Restore disconnected node",
            suggestion.Title);

        Assert.Equal(
            "gateway-001",
            suggestion.TargetIdentifier);

        Assert.Equal(
            DeviceCommandType.Reset,
            suggestion.RecommendedCommandType);

        Assert.Equal(
            CommandPriority.Critical,
            suggestion.RecommendedPriority);
    }

    [Fact]
    public void GetRecordedActivityCount_ReturnsTotalForSpecifiedOperator()
    {
        var service = CreateService();

        RecordActivity(
            service,
            "operator-a",
            UserActivityType.Search,
            "Warning");

        RecordActivity(
            service,
            "operator-a",
            UserActivityType.Command,
            DeviceCommandType.Reset.ToString(),
            "HUM-001");

        RecordActivity(
            service,
            "operator-a",
            UserActivityType.Search,
            "Environmental");

        RecordActivity(
            service,
            "operator-b",
            UserActivityType.Search,
            "Offline");

        Assert.Equal(
            3,
            service.GetRecordedActivityCount("operator-a"));

        Assert.Equal(
            1,
            service.GetRecordedActivityCount("operator-b"));

        Assert.Equal(
            0,
            service.GetRecordedActivityCount("operator-c"));
    }

    private static PredictiveActionService CreateService()
    {
        return new PredictiveActionService(
            new OperationalLookupService());
    }

    private static void RecordActivity(
        IPredictiveActionService service,
        string operatorIdentifier,
        UserActivityType activityType,
        string activityValue,
        string? targetIdentifier = null)
    {
        service.RecordActivity(new RecordUserActivityRequest
        {
            OperatorIdentifier = operatorIdentifier,
            ActivityType = activityType,
            ActivityValue = activityValue,
            TargetIdentifier = targetIdentifier,
            OccurredAtUtc = DateTime.UtcNow
        });
    }
}