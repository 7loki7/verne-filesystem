using Microsoft.EntityFrameworkCore;
using Verne.FileSystem.Application.DTOs;
using Verne.FileSystem.Application.Interfaces;
using Verne.FileSystem.Application.Persistence;
using Verne.FileSystem.Domain.Entities;
using Verne.FileSystem.Domain.Exceptions;

namespace Verne.FileSystem.Application.Services;

public class NodeService(FileSystemDbContext db) : INodeService
{
    private readonly FileSystemDbContext _db = db;

    public Guid GetRootId(CancellationToken ct = default)
    {
        var root = _db.Nodes.FirstOrDefault(n => n.ParentId == null) ?? throw new NodeNotFoundException(Guid.Empty);
        return root.Id;
    }

    public async Task<NodeResponse> CreateFolderAsync(CreateNodeRequest request, CancellationToken ct = default)
    {
        await EnsureParentIsFolderAsync(request.ParentId, ct);
        await EnsureNameIsUniqueInParentAsync(request.Name, request.ParentId, ct);

        var node = Node.Create(request.Name, NodeType.Folder, request.ParentId);
        _db.Nodes.Add(node);
        await _db.SaveChangesAsync(ct);

        return node.ToResponse();
    }

    public async Task<NodeResponse> CreateFileAsync(CreateNodeRequest request, CancellationToken ct = default)
    {
        await EnsureParentIsFolderAsync(request.ParentId, ct);
        await EnsureNameIsUniqueInParentAsync(request.Name, request.ParentId, ct);

        var node = Node.Create(request.Name, NodeType.File, request.ParentId);
        _db.Nodes.Add(node);
        await _db.SaveChangesAsync(ct);

        return node.ToResponse();
    }

    public async Task DeleteNodeAsync(Guid id, CancellationToken ct = default)
    {
        var node = await _db.Nodes.FindAsync([id], ct) ?? throw new NodeNotFoundException(id);

        if (node.ParentId == null) throw new InvalidOperationOnNodeException("Root folder cannot be deleted.");

        var descendants = await CollectDescendantsAsync(id, ct);

        _db.Nodes.RemoveRange(descendants);
        _db.Nodes.Remove(node);

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<NodeResponse>> GetChildrenAsync(Guid parentId, CancellationToken ct = default)
    {
        if (!await _db.Nodes.AnyAsync(n => n.Id == parentId, ct)) throw new NodeNotFoundException(parentId);

        return await _db.Nodes
            .Where(n => n.ParentId == parentId)
            .OrderBy(n => n.Type)
            .ThenBy(n => n.Name)
            .Select(n => n.ToResponse())
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<NodeResponse>> SearchByExactNameAsync(
        string name, Guid? parentId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name)) return [];

        if (parentId.HasValue && !await _db.Nodes.AnyAsync(n => n.Id == parentId, ct))
            throw new NodeNotFoundException(parentId.Value);

        var normalizedName = name.Trim().ToLower();

        var query = _db.Nodes.Where(n => n.Name.ToLower() == normalizedName);

        if (parentId.HasValue) query = query.Where(n => n.ParentId == parentId);

        return await query
            .Select(n => new NodeResponse(n.Id, n.Name, n.Type, n.ParentId, n.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<NodeResponse>> AutocompleteAsync(string startString, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(startString)) return [];

        var normalizedName = startString.Trim().ToLower();

        return await _db.Nodes
            .Where(n => n.Name.ToLower().StartsWith(normalizedName))
            .OrderBy(n => n.Name)
            .Take(10)
            .Select(n => n.ToResponse()).
            ToListAsync(ct);
    }

    private async Task EnsureParentIsFolderAsync(Guid parentId, CancellationToken ct)
    {
        var parent = await _db.Nodes.FindAsync([parentId], ct) ?? throw new NodeNotFoundException(parentId);

        if (parent.Type != NodeType.Folder)
            throw new InvalidOperationOnNodeException($"Node '{parentId}' is not a folder and cannot contain children.");
    }

    private async Task EnsureNameIsUniqueInParentAsync(string name, Guid parentId, CancellationToken ct)
    {
        var normalizedName = name.Trim().ToLower();

        var exists = await _db.Nodes.AnyAsync( n => n.ParentId == parentId && n.Name.ToLower() == normalizedName, ct);

        if (exists) throw new DuplicateNodeNameException(name, parentId);
    }

    private async Task<List<Node>> CollectDescendantsAsync(Guid parentId, CancellationToken ct)
    {
        var allNodes = await _db.Nodes.ToListAsync(ct);

        var result = new List<Node>();
        var queue = new Queue<Guid>();
        queue.Enqueue(parentId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            var children = allNodes.Where(n => n.ParentId == currentId).ToList();

            foreach (var child in children)
            {
                result.Add(child);
                if (child.Type == NodeType.Folder)
                    queue.Enqueue(child.Id);
            }
        }

        return result;
    }
}

internal static class NodeMappingExtensions
{
    internal static NodeResponse ToResponse(this Node node) =>
        new(node.Id, node.Name, node.Type, node.ParentId, node.CreatedAt);
}
