using Microsoft.AspNetCore.Mvc;
using Verne.FileSystem.Application.DTOs;
using Verne.FileSystem.Application.Interfaces;

namespace Verne.FileSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NodesController(INodeService nodeService) : ControllerBase
{
    private readonly INodeService _nodeService = nodeService;

    [HttpGet("root")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetRootId(CancellationToken ct)
    {
        var result = _nodeService.GetRootId(ct);
        return Ok(result);
    }

    [HttpPost("folders")]
    [ProducesResponseType(typeof(NodeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateFolder([FromBody] CreateNodeRequest request, CancellationToken ct)
    {
        var result = await _nodeService.CreateFolderAsync(request, ct);
        return CreatedAtAction(nameof(GetChildren), new { parentId = result.Id }, result);
    }

    [HttpPost("files")]
    [ProducesResponseType(typeof(NodeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateFile(
        [FromBody] CreateNodeRequest request, CancellationToken ct)
    {
        var result = await _nodeService.CreateFileAsync(request, ct);
        return CreatedAtAction(nameof(GetChildren), new { parentId = result.ParentId }, result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNode(Guid id, CancellationToken ct)
    {
        await _nodeService.DeleteNodeAsync(id, ct);
        return NoContent();
    }

    [HttpGet("{parentId:guid}/children")]
    [ProducesResponseType(typeof(IEnumerable<NodeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChildren(Guid parentId, CancellationToken ct)
    {
        var result = await _nodeService.GetChildrenAsync(parentId, ct);
        return Ok(result);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<NodeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string name,
        [FromQuery] Guid? parentId,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Query parameter 'name' is required.");

        var result = await _nodeService.SearchByExactNameAsync(name, parentId, ct);
        return Ok(result);
    }

    [HttpGet("autocomplete")]
    [ProducesResponseType(typeof(IEnumerable<NodeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Autocomplete([FromQuery] string prefix, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return BadRequest("Query parameter 'prefix' is required.");

        var result = await _nodeService.AutocompleteAsync(prefix, ct);
        return Ok(result);
    }
}
