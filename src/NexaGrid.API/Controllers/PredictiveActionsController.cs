using Microsoft.AspNetCore.Mvc;
using NexaGrid.API.Services;
using NexaGrid.Shared.DTOs;
using NexaGrid.Shared.DTOs.Predictive;

namespace NexaGrid.API.Controllers;

[ApiController]
[Route("api/predictive-actions")]
public sealed class PredictiveActionsController : ControllerBase
{
    private readonly IPredictiveActionService _predictiveActionService;

    public PredictiveActionsController(
        IPredictiveActionService predictiveActionService)
    {
        _predictiveActionService = predictiveActionService;
    }

    [HttpPost("activity")]
    [ProducesResponseType(
        typeof(ApiResponse<int>),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ApiResponse<int>),
        StatusCodes.Status400BadRequest)]
    public ActionResult<ApiResponse<int>> RecordActivity(
        [FromBody] RecordUserActivityRequest request)
    {
        try
        {
            _predictiveActionService.RecordActivity(request);

            var activityCount =
                _predictiveActionService.GetRecordedActivityCount(
                    request.OperatorIdentifier);

            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<int>.Ok(
                    activityCount,
                    "User activity recorded successfully."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                ApiResponse<int>.Failure(
                    exception.Message));
        }
    }

    [HttpGet("suggestions")]
    [ProducesResponseType(
        typeof(ApiResponse<
            IReadOnlyList<PredictiveSuggestionResponse>>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<
            IReadOnlyList<PredictiveSuggestionResponse>>),
        StatusCodes.Status400BadRequest)]
    public ActionResult<ApiResponse<
        IReadOnlyList<PredictiveSuggestionResponse>>> GetSuggestions(
        [FromQuery] string operatorIdentifier,
        [FromQuery] int activeAlertCount = 0)
    {
        try
        {
            if (activeAlertCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(activeAlertCount),
                    "The active alert count cannot be negative.");
            }

            IReadOnlyList<PredictiveSuggestionResponse> suggestions =
                _predictiveActionService.GenerateSuggestions(
                    operatorIdentifier,
                    activeAlertCount);

            return Ok(
                ApiResponse<
                    IReadOnlyList<PredictiveSuggestionResponse>>.Ok(
                    suggestions,
                    $"{suggestions.Count} predictive suggestion(s) generated."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                ApiResponse<
                    IReadOnlyList<PredictiveSuggestionResponse>>.Failure(
                    exception.Message));
        }
    }
}