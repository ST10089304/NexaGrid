using NexaGrid.Shared.DTOs.Predictive;
using NexaGrid.Shared.Enums;

namespace NexaGrid.API.Services;
/*CodeProject (2018)*/
public sealed class PredictiveActionService : IPredictiveActionService
{
    private const int SuggestionThreshold = 3;

    private readonly Lock _syncRoot = new();
    private readonly IOperationalLookupService _operationalLookupService;

    private readonly Dictionary<
        string,
        Dictionary<string, ActivityPattern>> _operatorPatterns =
        new(StringComparer.OrdinalIgnoreCase);
/*CodeProject (2018)*/
    public PredictiveActionService(
        IOperationalLookupService operationalLookupService)
    {
        _operationalLookupService = operationalLookupService;
    }
/*CodeProject (2018)*/
    public void RecordActivity(RecordUserActivityRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var operatorIdentifier = NormalizeRequired(
            request.OperatorIdentifier,
            nameof(request.OperatorIdentifier));

        var activityValue = NormalizeRequired(
            request.ActivityValue,
            nameof(request.ActivityValue));

        var targetIdentifier = request.TargetIdentifier?.Trim();
        var patternKey = CreatePatternKey(
            request.ActivityType,
            activityValue,
            targetIdentifier);

        lock (_syncRoot)
        {
            if (!_operatorPatterns.TryGetValue(
                    operatorIdentifier,
                    out var patterns))
            {
                patterns = new Dictionary<string, ActivityPattern>(
                    StringComparer.OrdinalIgnoreCase);

                _operatorPatterns[operatorIdentifier] = patterns;
            }

            if (!patterns.TryGetValue(patternKey, out var pattern))
            {
                pattern = new ActivityPattern
                {
                    ActivityType = request.ActivityType,
                    ActivityValue = activityValue,
                    TargetIdentifier = targetIdentifier
                };

                patterns[patternKey] = pattern;
            }

            pattern.Count++;
            pattern.LastOccurredAtUtc = request.OccurredAtUtc;
        }
    }
/*CodeProject (2018)*/
    public IReadOnlyList<PredictiveSuggestionResponse> GenerateSuggestions(
        string operatorIdentifier,
        int activeAlertCount = 0)
    {
        var normalizedOperator = NormalizeRequired(
            operatorIdentifier,
            nameof(operatorIdentifier));

        var suggestions = new List<PredictiveSuggestionResponse>();

        AddDisconnectedNodeSuggestions(suggestions);
        AddAlertSuggestion(suggestions, activeAlertCount);

        lock (_syncRoot)
        {
            if (_operatorPatterns.TryGetValue(
                    normalizedOperator,
                    out var patterns))
            {
                foreach (var pattern in patterns.Values
                             .Where(item =>
                                 item.Count >= SuggestionThreshold))
                {
                    suggestions.Add(
                        CreatePatternSuggestion(pattern));
                }
            }
        }

        return suggestions
            .OrderBy(suggestion =>
                (int)suggestion.RecommendedPriority)
            .ThenByDescending(suggestion =>
                suggestion.ConfidenceScore)
            .ThenBy(suggestion => suggestion.Title)
            .ToArray();
    }
/*CodeProject (2018)*/
    public int GetRecordedActivityCount(string operatorIdentifier)
    {
        if (string.IsNullOrWhiteSpace(operatorIdentifier))
        {
            return 0;
        }

        lock (_syncRoot)
        {
            if (!_operatorPatterns.TryGetValue(
                    operatorIdentifier.Trim(),
                    out var patterns))
            {
                return 0;
            }

            return patterns.Values.Sum(pattern => pattern.Count);
        }
    }
/*CodeProject (2018)*/
    private void AddDisconnectedNodeSuggestions(
        ICollection<PredictiveSuggestionResponse> suggestions)
    {
        foreach (var nodeIdentifier in
                 _operationalLookupService.GetDisconnectedNodes())
        {
            suggestions.Add(new PredictiveSuggestionResponse
            {
                Code = $"DISCONNECTED-{nodeIdentifier}",
                Title = "Restore disconnected node",
                Description =
                    $"Investigate and restore connectivity for {nodeIdentifier}.",
                Reason =
                    "The node currently appears in the unique disconnected-node set.",
                TargetIdentifier = nodeIdentifier,
                RecommendedCommandType = DeviceCommandType.Reset,
                RecommendedPriority = CommandPriority.Critical,
                ConfidenceScore = 0.98,
                SupportingActivityCount = 1
            });
        }
    }
/*CodeProject (2018)*/
    private static void AddAlertSuggestion(
        ICollection<PredictiveSuggestionResponse> suggestions,
        int activeAlertCount)
    {
        if (activeAlertCount < SuggestionThreshold)
        {
            return;
        }

        suggestions.Add(new PredictiveSuggestionResponse
        {
            Code = "ACTIVE-ALERT-TRIAGE",
            Title = "Prioritize active alert triage",
            Description =
                $"Review the {activeAlertCount} active alerts before processing standard commands.",
            Reason =
                "The active-alert threshold has been reached.",
            RecommendedPriority = CommandPriority.Critical,
            ConfidenceScore = CalculateConfidence(activeAlertCount),
            SupportingActivityCount = activeAlertCount
        });
    }
/*CodeProject (2018)*/
    private static PredictiveSuggestionResponse CreatePatternSuggestion(
        ActivityPattern pattern)
    {
        return pattern.ActivityType switch
        {
            UserActivityType.Search =>
                CreateSearchSuggestion(pattern),

            UserActivityType.Command =>
                CreateCommandSuggestion(pattern),

            _ => throw new InvalidOperationException(
                "Unsupported user activity type.")
        };
    }
/*CodeProject (2018)*/
    private static PredictiveSuggestionResponse CreateSearchSuggestion(
        ActivityPattern pattern)
    {
        return new PredictiveSuggestionResponse
        {
            Code = $"SEARCH-{CreateCodeValue(pattern.ActivityValue)}",
            Title = "Reuse frequent search",
            Description =
                $"Apply or save the frequently used '{pattern.ActivityValue}' filter.",
            Reason =
                $"The operator searched for this value {pattern.Count} times.",
            TargetIdentifier = pattern.TargetIdentifier,
            RecommendedPriority = CommandPriority.Normal,
            ConfidenceScore = CalculateConfidence(pattern.Count),
            SupportingActivityCount = pattern.Count
        };
    }
/*CodeProject (2018)*/
    private static PredictiveSuggestionResponse CreateCommandSuggestion(
        ActivityPattern pattern)
    {
        DeviceCommandType? recommendedCommand = null;

        if (Enum.TryParse<DeviceCommandType>(
                pattern.ActivityValue,
                true,
                out var parsedCommand))
        {
            recommendedCommand = parsedCommand;
        }

        return new PredictiveSuggestionResponse
        {
            Code = $"COMMAND-{CreateCodeValue(pattern.ActivityValue)}",
            Title = "Repeat frequent device action",
            Description = pattern.TargetIdentifier is null
                ? $"Consider repeating or automating the '{pattern.ActivityValue}' command."
                : $"Consider repeating or automating the '{pattern.ActivityValue}' command for {pattern.TargetIdentifier}.",
            Reason =
                $"The operator issued this command {pattern.Count} times.",
            TargetIdentifier = pattern.TargetIdentifier,
            RecommendedCommandType = recommendedCommand,
            RecommendedPriority = CommandPriority.High,
            ConfidenceScore = CalculateConfidence(pattern.Count),
            SupportingActivityCount = pattern.Count
        };
    }
/*CodeProject (2018)*/
    private static string CreatePatternKey(
        UserActivityType activityType,
        string activityValue,
        string? targetIdentifier)
    {
        return string.Join(
            "|",
            activityType,
            activityValue,
            targetIdentifier ?? string.Empty);
    }
/*CodeProject (2018)*/
    private static string CreateCodeValue(string value)
    {
        return value
            .Trim()
            .Replace(' ', '-')
            .ToUpperInvariant();
    }
/*CodeProject (2018)*/
    private static double CalculateConfidence(int activityCount)
    {
        return Math.Min(
            0.99,
            0.55 + activityCount * 0.08);
    }
/*CodeProject (2018)*/
    private static string NormalizeRequired(
        string value,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "A non-empty value is required.",
                parameterName);
        }

        return value.Trim();
    }
/*CodeProject (2018)*/
    private sealed class ActivityPattern
    {
        public UserActivityType ActivityType { get; init; }
        public string ActivityValue { get; init; } = string.Empty;
        public string? TargetIdentifier { get; init; }
        public int Count { get; set; }
        public DateTime LastOccurredAtUtc { get; set; }
    }
}