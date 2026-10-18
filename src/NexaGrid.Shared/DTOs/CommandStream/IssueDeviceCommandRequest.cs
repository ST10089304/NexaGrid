using System.ComponentModel.DataAnnotations;
using NexaGrid.Shared.Enums;

namespace NexaGrid.Shared.DTOs.CommandStream;
/*(C# Corner, 2020; Tutlane, 2019)*/
public sealed class IssueDeviceCommandRequest
{
    [Required(ErrorMessage = "The target identifier is required.")]
    [StringLength(
        100,
        MinimumLength = 3,
        ErrorMessage = "The target identifier must contain between 3 and 100 characters.")]
    public string TargetIdentifier { get; set; } = string.Empty;

    [EnumDataType(
        typeof(DeviceCommandType),
        ErrorMessage = "Select a valid device command type.")]
    public DeviceCommandType CommandType { get; set; }

    [EnumDataType(
        typeof(CommandPriority),
        ErrorMessage = "Select a valid command priority.")]
    public CommandPriority Priority { get; set; } = CommandPriority.Normal;

    [StringLength(
        250,
        ErrorMessage = "The command argument cannot exceed 250 characters.")]
    public string? Argument { get; set; }
}