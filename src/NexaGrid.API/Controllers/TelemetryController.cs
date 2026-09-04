using Microsoft.AspNetCore.Mvc;
using NexaGrid.API.Services;
using NexaGrid.Shared.DTOs;

namespace NexaGrid.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelemetryController : ControllerBase
{
    private readonly ITelemetryService _telemetryService;

    public TelemetryController(
        ITelemetryService telemetryService)
    {
        _telemetryService = telemetryService;
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(ApiResponse<TelemetryResponse>),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<
        ApiResponse<TelemetryResponse>>> Ingest(
        [FromBody] IngestTelemetryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            TelemetryResponse telemetry =
                await _telemetryService.IngestAsync(
                    request,
                    cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<TelemetryResponse>.Ok(
                    telemetry,
                    telemetry.IsAnomaly
                        ? "Telemetry accepted. An anomaly was detected."
                        : "Telemetry accepted successfully."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(
                ApiResponse<TelemetryResponse>.Failure(
                    exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                ApiResponse<TelemetryResponse>.Failure(
                    exception.Message));
        }
    }

    [HttpGet("sensor/{sensorIdentifier}")]
    [ProducesResponseType(
        typeof(ApiResponse<IReadOnlyList<TelemetryResponse>>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<
        ApiResponse<IReadOnlyList<TelemetryResponse>>>> GetRecent(
        string sensorIdentifier,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IReadOnlyList<TelemetryResponse> readings =
                await _telemetryService.GetRecentAsync(
                    sensorIdentifier,
                    limit,
                    cancellationToken);

            return Ok(
                ApiResponse<IReadOnlyList<TelemetryResponse>>.Ok(
                    readings,
                    $"{readings.Count} telemetry reading(s) retrieved."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                ApiResponse<IReadOnlyList<TelemetryResponse>>.Failure(
                    exception.Message));
        }
    }

    [HttpPost("seed/{sensorIdentifier}")]
    [ProducesResponseType(
        typeof(ApiResponse<TelemetrySeedResponse>),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<
        ApiResponse<TelemetrySeedResponse>>> Seed(
        string sensorIdentifier,
        [FromQuery] int count = 1_000,
        CancellationToken cancellationToken = default)
    {
        try
        {
            TelemetrySeedResponse result =
                await _telemetryService.SeedAsync(
                    sensorIdentifier,
                    count,
                    cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<TelemetrySeedResponse>.Ok(
                    result,
                    $"{result.CreatedReadingCount} mock readings generated successfully."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(
                ApiResponse<TelemetrySeedResponse>.Failure(
                    exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                ApiResponse<TelemetrySeedResponse>.Failure(
                    exception.Message));
        }
    }
}