namespace Verne.FileSystem.Domain.Entities;

public class Node
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public NodeType Type { get; private set; }
    public Guid? ParentId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Node() { }

    public static Node Create(string name, NodeType type, Guid? parentId = null)
    {
        return new Node
        {
            Id        = Guid.NewGuid(),
            Name      = name.Trim(),
            Type      = type,
            ParentId  = parentId,
            CreatedAt = DateTime.UtcNow
        };
    }
}