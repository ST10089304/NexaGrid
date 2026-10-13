using NexaGrid.Shared.Enums;

namespace NexaGrid.Shared.Commands;

public sealed class DeviceCommand
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string TargetIdentifier { get; init; } = string.Empty;

    public DeviceCommandType CommandType { get; init; }

    public CommandPriority Priority { get; init; } = CommandPriority.Normal;

    public string? Argument { get; init; }

    public CommandStatus Status { get; set; } = CommandStatus.Queued;

    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    public DateTime? ProcessedAtUtc { get; set; }

    public string? FailureReason { get; set; }
}