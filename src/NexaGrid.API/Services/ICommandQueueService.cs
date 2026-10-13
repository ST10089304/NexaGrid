using NexaGrid.Shared.Commands;

namespace NexaGrid.API.Services;

public interface ICommandQueueService
{
    int PendingCount { get; }
    int PriorityCount { get; }
    int HistoryCount { get; }

    void Enqueue(DeviceCommand command);
    bool TryDequeue(out DeviceCommand? command);
    void RecordProcessed(DeviceCommand command);
    bool TryUndo(out DeviceCommand? command);

    IReadOnlyList<DeviceCommand> GetPendingSnapshot();
    IReadOnlyList<DeviceCommand> GetHistorySnapshot();
}