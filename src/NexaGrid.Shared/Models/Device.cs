namespace NexaGrid.Shared.Models;

public class Device
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string MacAddress { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public DateTime RegisteredAtUtc { get; set; } = DateTime.UtcNow;

    public List<Sensor> Sensors { get; set; } = [];
}