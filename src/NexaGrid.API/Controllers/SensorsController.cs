using Microsoft.AspNetCore.Mvc;
using NexaGrid.API.Services;
using NexaGrid.Shared.DTOs;

namespace NexaGrid.API.Controllers;
/*Halvorsen, H. P. (2020)*/
[ApiController]
[Route("api/[controller]")]
public class SensorsController : ControllerBase
{
    private readonly ISensorService _sensorService;
/*Halvorsen, H. P. (2020)*/
    public SensorsController(ISensorService sensorService)
    {
        _sensorService = sensorService;
    }
/*Halvorsen, H. P. (2020)*/
    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<IReadOnlyList<SensorResponse>>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<
        ApiResponse<IReadOnlyList<SensorResponse>>>> GetAll(
        CancellationToken cancellationToken)
    {
        IReadOnlyList<SensorResponse> sensors =
            await _sensorService.GetAllAsync(cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<SensorResponse>>.Ok(
            sensors,
            $"{sensors.Count} sensor(s) retrieved."));
    }
/*Issa, G. (2021)*/
    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<SensorResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SensorResponse>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        SensorResponse? sensor =
            await _sensorService.GetByIdAsync(
                id,
                cancellationToken);

        if (sensor is null)
        {
            return NotFound(
                ApiResponse<SensorResponse>.Failure(
                    $"Sensor with ID {id} was not found."));
        }
/*Issa, G. (2021)*/
        return Ok(
            ApiResponse<SensorResponse>.Ok(
                sensor,
                "Sensor retrieved successfully."));
    }
/*Issa, G. (2021)*/
    [HttpPost]
    [ProducesResponseType(
        typeof(ApiResponse<SensorResponse>),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<SensorResponse>>> Register(
        [FromBody] RegisterSensorRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            SensorResponse sensor =
                await _sensorService.RegisterAsync(
                    request,
                    cancellationToken);
/*Issa, G. (2021)*/
            return CreatedAtAction(
                nameof(GetById),
                new { id = sensor.Id },
                ApiResponse<SensorResponse>.Ok(
                    sensor,
                    "Sensor registered successfully."));
        }
        catch (InvalidOperationException exception) /*Issa, G. (2021)*/
        {
            return Conflict(
                ApiResponse<SensorResponse>.Failure(
                    exception.Message));
        }
    }
}