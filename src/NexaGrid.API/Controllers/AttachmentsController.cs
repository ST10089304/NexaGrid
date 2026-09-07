using Microsoft.AspNetCore.Mvc;
using NexaGrid.API.Services;
using NexaGrid.Shared.DTOs;
/*Microsoft (2026) */
/*Kip, R. (2021) Upload Files To Folder Using C#*/
namespace NexaGrid.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttachmentsController : ControllerBase
{/*Microsoft (2026) */
    private readonly IAttachmentService
        _attachmentService;

    public AttachmentsController(
        IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    [HttpPost("sensor/{sensorId:int}")]/*Microsoft (2026) */
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10_485_760)]/*Kip, R. (2021) Upload Files To Folder Using C#*/
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
    {/*Microsoft (2026) */
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
        catch (KeyNotFoundException exception)/*Microsoft (2026) */
        {
            return NotFound(
                ApiResponse<SensorAttachmentResponse>.Failure(
                    exception.Message));
        }
        catch (ArgumentException exception)/*Microsoft (2026) */
        {
            return BadRequest(
                ApiResponse<SensorAttachmentResponse>.Failure(
                    exception.Message));
        }
    }

    [HttpGet("sensor/{sensorId:int}")]
    public async Task<ActionResult<
        ApiResponse<IReadOnlyList<SensorAttachmentResponse>>>>
        GetBySensor(/*Microsoft (2026) */
            int sensorId,
            CancellationToken cancellationToken)
    {
        try
        {/*Microsoft (2026) */
        /*Kip, R. (2021) Upload Files To Folder Using C#*/
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
        catch (KeyNotFoundException exception)/*Microsoft (2026) */
        {
            return NotFound(
                ApiResponse<
                    IReadOnlyList<SensorAttachmentResponse>>.Failure(
                    exception.Message));
        }
    }

    [HttpGet("{attachmentId:int}/download")]/*Microsoft (2026) */
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

            return File(/*Microsoft (2026) */
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
        catch (FileNotFoundException exception)/*Microsoft (2026) */
        {
            return NotFound(new
            {
                success = false,
                message = exception.Message
            });
        }
    }
}