using Microsoft.AspNetCore.Mvc;
using NexaGrid.API.Services;
using NexaGrid.Shared.DTOs;

namespace NexaGrid.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService
        _attachmentService;

    public AttachmentsController(
        IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    [HttpPost("sensor/{sensorId:int}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10_485_760)]
    [ProducesResponseType(
        typeof(ApiResponse<SensorAttachmentResponse>),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<
        ApiResponse<SensorAttachmentResponse>>> Upload(
        int sensorId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        try
        {
            SensorAttachmentResponse attachment =
                await _attachmentService.UploadAsync(
                    sensorId,
                    file,
                    cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<SensorAttachmentResponse>.Ok(
                    attachment,
                    "Attachment uploaded successfully."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(
                ApiResponse<SensorAttachmentResponse>.Failure(
                    exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                ApiResponse<SensorAttachmentResponse>.Failure(
                    exception.Message));
        }
    }

    [HttpGet("sensor/{sensorId:int}")]
    public async Task<ActionResult<
        ApiResponse<IReadOnlyList<SensorAttachmentResponse>>>>
        GetBySensor(
            int sensorId,
            CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<SensorAttachmentResponse>
                attachments =
                    await _attachmentService
                        .GetBySensorIdAsync(
                            sensorId,
                            cancellationToken);

            return Ok(
                ApiResponse<
                    IReadOnlyList<SensorAttachmentResponse>>.Ok(
                    attachments,
                    $"{attachments.Count} attachment(s) retrieved."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(
                ApiResponse<
                    IReadOnlyList<SensorAttachmentResponse>>.Failure(
                    exception.Message));
        }
    }

    [HttpGet("{attachmentId:int}/download")]
    public async Task<IActionResult> Download(
        int attachmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            AttachmentDownloadResult result =
                await _attachmentService.DownloadAsync(
                    attachmentId,
                    cancellationToken);

            return File(
                result.Stream,
                result.ContentType,
                result.FileName,
                enableRangeProcessing: true);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new
            {
                success = false,
                message = exception.Message
            });
        }
        catch (FileNotFoundException exception)
        {
            return NotFound(new
            {
                success = false,
                message = exception.Message
            });
        }
    }
}