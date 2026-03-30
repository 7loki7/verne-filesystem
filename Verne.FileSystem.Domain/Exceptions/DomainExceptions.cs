namespace Verne.FileSystem.Domain.Exceptions;

public class NodeNotFoundException : Exception
{
    public NodeNotFoundException(Guid id)
        : base($"Node with id '{id}' was not found.") { }
}

public class DuplicateNodeNameException : Exception
{
    public DuplicateNodeNameException(string name, Guid? parentId)
        : base($"A node named '{name}' already exists in parent '{parentId?.ToString() ?? "root"}'.") { }
}

public class InvalidOperationOnNodeException : Exception
{
    public InvalidOperationOnNodeException(string message) : base(message) { }
}