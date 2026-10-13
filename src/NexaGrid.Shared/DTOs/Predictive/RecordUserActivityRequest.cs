using System.ComponentModel.DataAnnotations;
using NexaGrid.Shared.Enums;

namespace NexaGrid.Shared.DTOs.Predictive;

public sealed class RecordUserActivityRequest
{
    [Required(ErrorMessage = "The operator identifier is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string OperatorIdentifier { get; set; } = string.Empty;

    [EnumDataType(
        typeof(UserActivityType),
        ErrorMessage = "Select a valid user activity type.")]
    public UserActivityType ActivityType { get; set; }

    [Required(ErrorMessage = "The activity value is required.")]
    [StringLength(150, MinimumLength = 1)]
    public string ActivityValue { get; set; } = string.Empty;

    [StringLength(100)]
    public string? TargetIdentifier { get; set; }

    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}