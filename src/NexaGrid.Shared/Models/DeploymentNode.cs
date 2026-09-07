namespace NexaGrid.Shared.Models;
/*Stack Overflow Community (2018) Best practices for structuring Models, Structs, and Generics in C# projects*/
public class DeploymentNode
{
    public string Name { get; set; } = string.Empty;

    public string NodeType { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public List<DeploymentNode> Children { get; set; } = [];
}