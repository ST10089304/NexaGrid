using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using NexaGrid.Shared.Enums;
/* OWASP Foundation (2026) */
namespace NexaGrid.Shared.DTOs;

public class IngestTelemetryRequest
{
    [Required(ErrorMessage = "The sensor identifier is required.")]
    public string SensorIdentifier { get; set; } = string.Empty;

    [Required(ErrorMessage = "The telemetry data type is required.")]
    public TelemetryDataType DataType { get; set; }

    public JsonElement Value { get; set; }

    [StringLength(30)]
    public string Unit { get; set; } = string.Empty;

    public double? MinimumAcceptableValue { get; set; }

    public double? MaximumAcceptableValue { get; set; }

    public bool? ExpectedBooleanValue { get; set; }

    public DateTime? RecordedAtUtc { get; set; }
}
