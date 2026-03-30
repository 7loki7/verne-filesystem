using System.ComponentModel.DataAnnotations;
using Verne.FileSystem.Domain.Entities;

namespace Verne.FileSystem.Application.DTOs;

public record NodeResponse(
    Guid     Id,
    string   Name,
    NodeType Type,
    Guid?    ParentId,
    DateTime CreatedAt
);

public record CreateNodeRequest(
    [Required]
    [MaxLength(200)]
    string Name,
    Guid  ParentId
);

