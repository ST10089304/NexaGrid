using Microsoft.AspNetCore.Mvc;
using NexaGrid.API.Services;
using NexaGrid.Shared.DTOs;
using NexaGrid.Shared.Models;

namespace NexaGrid.API.Controllers;

/*
 * Exposes the Part 2 Dictionary, SortedDictionary, and HashSet operational
 * structures through controlled API endpoints (Microsoft, 2026).
 */
[ApiController]
[Route("api/operational-state")]
public sealed class OperationalStateController : ControllerBase
{
    private readonly IOperationalLookupService _operationalLookupService;

    public OperationalStateController(
        IOperationalLookupService operationalLookupService)
    {
        _operationalLookupService = operationalLookupService;
    }

    [HttpGet("summary")]
    public ActionResult<ApiResponse<object>> GetSummary()
    {
        var summary = new
        {
            cachedDeviceCount =
                _operationalLookupService.CachedDeviceCount,

            telemetryTimelineCount =
                _operationalLookupService.TimelineCount,

            disconnectedNodeCount =
                _operationalLookupService.DisconnectedNodeCount
        };

        return Ok(
            ApiResponse<object>.Ok(
                summary,
                "Operational structure summary retrieved."));
    }

    [HttpGet("devices/{macAddress}")]
    public ActionResult<ApiResponse<Device>> GetDevice(
        string macAddress)
    {
        if (!_operationalLookupService.TryGetDevice(
                macAddress,
                out Device? device)
            || device is null)
        {
            return NotFound(
                ApiResponse<Device>.Failure(
                    $"No cached device was found for MAC address '{macAddress}'."));
        }

        return Ok(
            ApiResponse<Device>.Ok(
                device,
                "Cached device retrieved successfully."));
    }

    [HttpGet("telemetry")]
    public ActionResult<ApiResponse<object>> GetTimeline(
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null)
    {
        if (fromUtc.HasValue
            && toUtc.HasValue
            && fromUtc.Value > toUtc.Value)
        {
            return BadRequest(
                ApiResponse<object>.Failure(
                    "The fromUtc value cannot be later than toUtc."));
        }

        IReadOnlyList<TelemetryRecord> records =
            _operationalLookupService.GetTelemetryTimeline(
                fromUtc?.ToUniversalTime(),
                toUtc?.ToUniversalTime());

        /*
         * Project entities into an API-safe shape so Entity Framework
         * navigation properties do not create JSON reference cycles
         * (Microsoft, 2026).
         */
        var timeline = records
            .Select(record => new
            {
                record.Id,
                record.SensorId,
                SensorIdentifier =
                    record.Sensor?.UniqueIdentifier ?? string.Empty,
                record.DataType,
                record.Value,
                record.Unit,
                record.IsAnomaly,
                record.RecordedAtUtc
            })
            .ToArray();

        return Ok(
            ApiResponse<object>.Ok(
                timeline,
                $"{timeline.Length} operational telemetry record(s) retrieved."));
    }

    [HttpGet("disconnected")]
    public ActionResult<
        ApiResponse<IReadOnlyList<string>>> GetDisconnectedNodes()
    {
        IReadOnlyList<string> identifiers =
            _operationalLookupService.GetDisconnectedNodes();

        return Ok(
            ApiResponse<IReadOnlyList<string>>.Ok(
                identifiers,
                $"{identifiers.Count} disconnected node(s) retrieved."));
    }

    [HttpPost("nodes/{nodeIdentifier}/disconnect")]
    public ActionResult<ApiResponse<object>> MarkDisconnected(
        string nodeIdentifier)
    {
        try
        {
            bool changed =
                _operationalLookupService.MarkDisconnected(
                    nodeIdentifier);

            return Ok(
                ApiResponse<object>.Ok(
                    new
                    {
                        nodeIdentifier =
                            nodeIdentifier.Trim().ToUpperInvariant(),

                        disconnected = true,
                        stateChanged = changed
                    },
                    changed
                        ? "Node marked as disconnected."
                        : "Node was already marked as disconnected."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                ApiResponse<object>.Failure(
                    exception.Message));
        }
    }

    [HttpPost("nodes/{nodeIdentifier}/connect")]
    public ActionResult<ApiResponse<object>> MarkConnected(
        string nodeIdentifier)
    {
        try
        {
            bool changed =
                _operationalLookupService.MarkConnected(
                    nodeIdentifier);

            return Ok(
                ApiResponse<object>.Ok(
                    new
                    {
                        nodeIdentifier =
                            nodeIdentifier.Trim().ToUpperInvariant(),

                        disconnected = false,
                        stateChanged = changed
                    },
                    changed
                        ? "Node marked as connected."
                        : "Node was not marked as disconnected."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                ApiResponse<object>.Failure(
                    exception.Message));
        }
    }
}