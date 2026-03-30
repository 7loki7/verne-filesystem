using Verne.FileSystem.Application.DTOs;

namespace Verne.FileSystem.Application.Interfaces;

public interface INodeService
{
    Guid GetRootId(CancellationToken ct = default);
    Task<NodeResponse> CreateFolderAsync(CreateNodeRequest request, CancellationToken ct = default);
    Task<NodeResponse> CreateFileAsync(CreateNodeRequest request, CancellationToken ct = default);
    Task DeleteNodeAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<NodeResponse>> GetChildrenAsync(Guid parentId, CancellationToken ct = default);
    Task<IEnumerable<NodeResponse>> SearchByExactNameAsync(string name, Guid? parentId, CancellationToken ct = default);
    Task<IEnumerable<NodeResponse>> AutocompleteAsync(string prefix, CancellationToken ct = default);
}


