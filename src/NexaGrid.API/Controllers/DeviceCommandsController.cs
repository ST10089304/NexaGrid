using Microsoft.AspNetCore.Mvc;
using NexaGrid.API.Services;
using NexaGrid.Shared.Commands;
using NexaGrid.Shared.DTOs;
using NexaGrid.Shared.DTOs.CommandStream;
using NexaGrid.Shared.DTOs.Predictive;
using NexaGrid.Shared.Enums;

namespace NexaGrid.API.Controllers;
// ASP.NET Core controller routing, model binding and typed action results follow
// Microsoft guidance for controller-based web APIs (Microsoft, 2026a; Microsoft, 2026b).
/*Halvorsen, H. P. (2020)*/
[ApiController]
[Route("api/device-commands")]
public sealed class DeviceCommandsController : ControllerBase
{
    private readonly ICommandQueueService _commandQueueService;
    private readonly IPredictiveActionService _predictiveActionService;

    public DeviceCommandsController(
        ICommandQueueService commandQueueService,
        IPredictiveActionService predictiveActionService)
    {
        _commandQueueService = commandQueueService;
        _predictiveActionService = predictiveActionService;
    }
/*Halvorsen, H. P. (2020)*/
    [HttpPost]
    [ProducesResponseType(
        typeof(ApiResponse<DeviceCommandResponse>),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ApiResponse<DeviceCommandResponse>),
        StatusCodes.Status400BadRequest)]
    public ActionResult<ApiResponse<DeviceCommandResponse>> Issue(
        [FromBody] IssueDeviceCommandRequest request,
        [FromQuery] string operatorIdentifier)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(operatorIdentifier))
            {
                throw new ArgumentException(
                    "The operator identifier is required.",
                    nameof(operatorIdentifier));
            }

            var command = new DeviceCommand
            {
                TargetIdentifier = request.TargetIdentifier.Trim(),
                CommandType = request.CommandType,
                Priority = request.Priority,
                Argument = string.IsNullOrWhiteSpace(request.Argument)
                    ? null
                    : request.Argument.Trim(),
                RequestedAtUtc = DateTime.UtcNow
            };

            _commandQueueService.Enqueue(command);

            _predictiveActionService.RecordActivity(
                new RecordUserActivityRequest
                {
                    OperatorIdentifier = operatorIdentifier.Trim(),
                    ActivityType = UserActivityType.Command,
                    ActivityValue = request.CommandType.ToString(),
                    TargetIdentifier = command.TargetIdentifier,
                    OccurredAtUtc = command.RequestedAtUtc
                });

            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<DeviceCommandResponse>.Ok(
                    ToResponse(command),
                    "Device command queued successfully."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                ApiResponse<DeviceCommandResponse>.Failure(
                    exception.Message));
        }
    }
/*Halvorsen, H. P. (2020)*/
    [HttpGet("pending")]
    [ProducesResponseType(
        typeof(ApiResponse<
            IReadOnlyList<DeviceCommandResponse>>),
        StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<
        IReadOnlyList<DeviceCommandResponse>>> GetPending()
    {
        IReadOnlyList<DeviceCommandResponse> commands =
            _commandQueueService
                .GetPendingSnapshot()
                .Select(ToResponse)
                .ToArray();

        return Ok(
            ApiResponse<IReadOnlyList<DeviceCommandResponse>>.Ok(
                commands,
                $"{commands.Count} pending device command(s) retrieved."));
    }
/*Halvorsen, H. P. (2020)*/
    [HttpPost("process-next")]
    [ProducesResponseType(
        typeof(ApiResponse<DeviceCommandResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<DeviceCommandResponse>),
        StatusCodes.Status404NotFound)]
    public ActionResult<ApiResponse<DeviceCommandResponse>> ProcessNext()
    {
        if (!_commandQueueService.TryDequeue(
                out DeviceCommand? command) ||
            command is null)
        {
            return NotFound(
                ApiResponse<DeviceCommandResponse>.Failure(
                    "There are no pending device commands to process."));
        }

        _commandQueueService.RecordProcessed(command);

        return Ok(
            ApiResponse<DeviceCommandResponse>.Ok(
                ToResponse(command),
                "Device command processed successfully."));
    }
/*Halvorsen, H. P. (2020)*/
    [HttpPost("undo-last")]
    [ProducesResponseType(
        typeof(ApiResponse<DeviceCommandResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<DeviceCommandResponse>),
        StatusCodes.Status404NotFound)]
    public ActionResult<ApiResponse<DeviceCommandResponse>> UndoLast()
    {
        if (!_commandQueueService.TryUndo(
                out DeviceCommand? command) ||
            command is null)
        {
            return NotFound(
                ApiResponse<DeviceCommandResponse>.Failure(
                    "There are no processed device commands to undo."));
        }

        return Ok(
            ApiResponse<DeviceCommandResponse>.Ok(
                ToResponse(command),
                "The most recently processed command was undone."));
    }
/*Halvorsen, H. P. (2020)*/
    [HttpGet("history")]
    [ProducesResponseType(
        typeof(ApiResponse<
            IReadOnlyList<DeviceCommandResponse>>),
        StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<
        IReadOnlyList<DeviceCommandResponse>>> GetHistory()
    {
        IReadOnlyList<DeviceCommandResponse> commands =
            _commandQueueService
                .GetHistorySnapshot()
                .Select(ToResponse)
                .ToArray();

        return Ok(
            ApiResponse<IReadOnlyList<DeviceCommandResponse>>.Ok(
                commands,
                $"{commands.Count} processed device command(s) retrieved."));
    }
/*Halvorsen, H. P. (2020)*/
    private static DeviceCommandResponse ToResponse(
        DeviceCommand command)
    {
        return new DeviceCommandResponse
        {
            Id = command.Id,
            TargetIdentifier = command.TargetIdentifier,
            CommandType = command.CommandType,
            Priority = command.Priority,
            Argument = command.Argument,
            Status = command.Status,
            RequestedAtUtc = command.RequestedAtUtc,
            ProcessedAtUtc = command.ProcessedAtUtc,
            FailureReason = command.FailureReason
        };
    }
}