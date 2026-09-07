using System.ComponentModel.DataAnnotations;
using NexaGrid.Shared.Enums;

namespace NexaGrid.Shared.DTOs;
/* OWASP Foundation (2026) */
public class RegisterSensorRequest
{
    [Required(ErrorMessage = "The device name is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string DeviceName { get; set; } = string.Empty;

    [Required(ErrorMessage = "The MAC address is required.")]
    [RegularExpression(
        @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$",
        ErrorMessage = "Enter a valid MAC address, for example AA:BB:CC:DD:EE:FF.")]
    public string MacAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "The manufacturer is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string Manufacturer { get; set; } = string.Empty;

    [Required(ErrorMessage = "The sensor name is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string SensorName { get; set; } = string.Empty;

    [Required(ErrorMessage = "The sensor identifier is required.")]
    [StringLength(100, MinimumLength = 3)]
    [RegularExpression(
        @"^[A-Za-z0-9_-]+$",
        ErrorMessage = "The sensor identifier can only contain letters, numbers, hyphens and underscores.")]
    public string UniqueIdentifier { get; set; } = string.Empty;

    [Required(ErrorMessage = "The deployment location is required.")]
    [StringLength(150, MinimumLength = 2)]
    public string DeploymentLocation { get; set; } = string.Empty;

    [Required(ErrorMessage = "The sensor category is required.")]
    [EnumDataType(
        typeof(SensorCategory),
        ErrorMessage = "Select a valid sensor category.")]
    public SensorCategory Category { get; set; }
}