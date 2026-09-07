namespace NexaGrid.Shared.Models;
/*Stack Overflow Community (2018) Best practices for structuring Models, Structs, and Generics in C# projects*/
public class Device
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string MacAddress { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public DateTime RegisteredAtUtc { get; set; } = DateTime.UtcNow;

    public List<Sensor> Sensors { get; set; } = [];
}