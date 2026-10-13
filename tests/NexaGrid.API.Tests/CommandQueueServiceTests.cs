using NexaGrid.API.Services;
using NexaGrid.Shared.Commands;
using NexaGrid.Shared.Enums;

namespace NexaGrid.API.Tests;

public sealed class CommandQueueServiceTests
{
    [Fact]
    public void TryDequeue_ProcessesCriticalCommandBeforeStandardCommands()
    {
        var service = new CommandQueueService();

        var firstStandard = CreateCommand(
            "TEMP-001",
            CommandPriority.Normal);

        var secondStandard = CreateCommand(
            "HUM-001",
            CommandPriority.Low);

        var critical = CreateCommand(
            "PRESSURE-001",
            CommandPriority.Critical);

        service.Enqueue(firstStandard);
        service.Enqueue(secondStandard);
        service.Enqueue(critical);

        Assert.True(service.TryDequeue(out var firstResult));
        Assert.Equal(critical.Id, firstResult!.Id);
        Assert.Equal(CommandStatus.Processing, firstResult.Status);

        Assert.True(service.TryDequeue(out var secondResult));
        Assert.Equal(firstStandard.Id, secondResult!.Id);

        Assert.True(service.TryDequeue(out var thirdResult));
        Assert.Equal(secondStandard.Id, thirdResult!.Id);
    }

    [Fact]
    public void StandardQueue_PreservesFirstInFirstOutOrder()
    {
        var service = new CommandQueueService();

        var first = CreateCommand("DEVICE-001", CommandPriority.Normal);
        var second = CreateCommand("DEVICE-002", CommandPriority.Normal);
        var third = CreateCommand("DEVICE-003", CommandPriority.Low);

        service.Enqueue(first);
        service.Enqueue(second);
        service.Enqueue(third);

        Assert.True(service.TryDequeue(out var firstResult));
        Assert.True(service.TryDequeue(out var secondResult));
        Assert.True(service.TryDequeue(out var thirdResult));

        Assert.Equal(first.Id, firstResult!.Id);
        Assert.Equal(second.Id, secondResult!.Id);
        Assert.Equal(third.Id, thirdResult!.Id);
    }

    [Fact]
    public void TryUndo_ReturnsMostRecentlyProcessedCommand()
    {
        var service = new CommandQueueService();

        var first = CreateCommand("DEVICE-001", CommandPriority.Normal);
        var second = CreateCommand("DEVICE-002", CommandPriority.Normal);

        service.RecordProcessed(first);
        service.RecordProcessed(second);

        Assert.True(service.TryUndo(out var result));

        Assert.Equal(second.Id, result!.Id);
        Assert.Equal(CommandStatus.Undone, result.Status);
        Assert.Equal(1, service.HistoryCount);
    }

    [Fact]
    public void PendingAndHistoryCounts_ReflectCollectionState()
    {
        var service = new CommandQueueService();

        var normal = CreateCommand("DEVICE-001", CommandPriority.Normal);
        var critical = CreateCommand("DEVICE-002", CommandPriority.Critical);

        service.Enqueue(normal);
        service.Enqueue(critical);

        Assert.Equal(2, service.PendingCount);
        Assert.Equal(1, service.PriorityCount);

        Assert.True(service.TryDequeue(out var processed));
        service.RecordProcessed(processed!);

        Assert.Equal(1, service.PendingCount);
        Assert.Equal(1, service.HistoryCount);
    }

    private static DeviceCommand CreateCommand(
        string targetIdentifier,
        CommandPriority priority)
    {
        return new DeviceCommand
        {
            TargetIdentifier = targetIdentifier,
            CommandType = DeviceCommandType.Toggle,
            Priority = priority
        };
    }
}