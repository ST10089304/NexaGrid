using NexaGrid.Shared.Commands;
using NexaGrid.Shared.Enums;

namespace NexaGrid.API.Services;

public sealed class CommandQueueService : ICommandQueueService
{
    private readonly Lock _syncRoot = new();
    private readonly Queue<DeviceCommand> _standardQueue = new();
    private readonly PriorityQueue<DeviceCommand, int> _priorityQueue = new();
    private readonly Stack<DeviceCommand> _commandHistory = new();

    public int PendingCount
    {
        get
        {
            lock (_syncRoot)
            {
                return _standardQueue.Count + _priorityQueue.Count;
            }
        }
    }

    public int PriorityCount
    {
        get
        {
            lock (_syncRoot)
            {
                return _priorityQueue.Count;
            }
        }
    }

    public int HistoryCount
    {
        get
        {
            lock (_syncRoot)
            {
                return _commandHistory.Count;
            }
        }
    }

    public void Enqueue(DeviceCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        lock (_syncRoot)
        {
            command.Status = CommandStatus.Queued;

            if (command.Priority is CommandPriority.Critical or CommandPriority.High)
            {
                _priorityQueue.Enqueue(command, (int)command.Priority);
                return;
            }

            _standardQueue.Enqueue(command);
        }
    }

    public bool TryDequeue(out DeviceCommand? command)
    {
        lock (_syncRoot)
        {
            if (_priorityQueue.TryDequeue(out command, out _))
            {
                command.Status = CommandStatus.Processing;
                return true;
            }

            if (_standardQueue.TryDequeue(out command))
            {
                command.Status = CommandStatus.Processing;
                return true;
            }

            command = null;
            return false;
        }
    }

    public void RecordProcessed(DeviceCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        lock (_syncRoot)
        {
            command.Status = CommandStatus.Completed;
            command.ProcessedAtUtc = DateTime.UtcNow;
            command.FailureReason = null;
            _commandHistory.Push(command);
        }
    }

    public bool TryUndo(out DeviceCommand? command)
    {
        lock (_syncRoot)
        {
            if (!_commandHistory.TryPop(out command))
            {
                command = null;
                return false;
            }

            command.Status = CommandStatus.Undone;
            return true;
        }
    }

    public IReadOnlyList<DeviceCommand> GetPendingSnapshot()
    {
        lock (_syncRoot)
        {
            var priorityCommands = _priorityQueue.UnorderedItems
                .OrderBy(item => item.Priority)
                .ThenBy(item => item.Element.RequestedAtUtc)
                .Select(item => item.Element);

            return priorityCommands
                .Concat(_standardQueue)
                .ToArray();
        }
    }

    public IReadOnlyList<DeviceCommand> GetHistorySnapshot()
    {
        lock (_syncRoot)
        {
            return _commandHistory.ToArray();
        }
    }
}