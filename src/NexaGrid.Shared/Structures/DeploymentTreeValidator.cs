using NexaGrid.Shared.Models;

namespace NexaGrid.Shared.Structures;
/*Stack Overflow Community (2018) Best practices for structuring Models, Structs, and Generics in C# projects*/
public static class DeploymentTreeValidator
{
    public static bool Validate(DeploymentNode? node)
    {
        // Recursion base case
        if (node is null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(node.Name) ||
            string.IsNullOrWhiteSpace(node.NodeType) ||
            !node.IsActive)
        {
            return false;
        }

        foreach (DeploymentNode child in node.Children)
        {
            if (!Validate(child))
            {
                return false;
            }
        }

        return true;
    }

/*Stack Overflow Community (2018) Best practices for structuring Models, Structs, and Generics in C# projects*/
    public static DeploymentNode? FindNode(
        DeploymentNode? currentNode,
        string nodeName)
    {
        // Recursion base case
        if (currentNode is null)
        {
            return null;
        }

        if (currentNode.Name.Equals(
                nodeName,
                StringComparison.OrdinalIgnoreCase))
        {
            return currentNode;
        }

        foreach (DeploymentNode child in currentNode.Children)
        {
            DeploymentNode? result = FindNode(child, nodeName);

            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }
}