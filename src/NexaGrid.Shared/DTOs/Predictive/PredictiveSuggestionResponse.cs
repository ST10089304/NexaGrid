using NexaGrid.Shared.Enums;

namespace NexaGrid.Shared.DTOs.Predictive;
/*(C# Corner, 2020; Tutlane, 2019)*/
public sealed class PredictiveSuggestionResponse
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? TargetIdentifier { get; set; }
    public DeviceCommandType? RecommendedCommandType { get; set; }
    public CommandPriority RecommendedPriority { get; set; }
    public double ConfidenceScore { get; set; }
    public int SupportingActivityCount { get; set; }
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
}