using NexaGrid.Shared.Enums;

namespace NexaGrid.Shared.DTOs.CommandStream;
/*(C# Corner, 2020; Tutlane, 2019)*/
public sealed class DeviceCommandResponse
{
    public Guid Id { get; set; }

    public string TargetIdentifier { get; set; } = string.Empty;

    public DeviceCommandType CommandType { get; set; }

    public CommandPriority Priority { get; set; }

    public string? Argument { get; set; }

    public CommandStatus Status { get; set; }

    public DateTime RequestedAtUtc { get; set; }

    public DateTime? ProcessedAtUtc { get; set; }

    public string? FailureReason { get; set; }
}