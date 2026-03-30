# Verne FileSystem API

This is a Verne Recruitment Task exericse : a large-scale browser-based filesystem API.  
Built with **.NET 8**, , and **EF Core InMemory**.

---
## Key design decisions

This solution is imagined like a real filesystem: one seeded root(C:\ equivalent), everything else lives inside it, and GetRootId is how the client bootstraps itself on first load to know where to start.
Some implementation decisions were influenced by the limitations of the EF Core InMemory provider, for string comparisons and query behaviour.
The code is written to remain simple for this exercise but it is structured similar as it would be in a real backend implementation. 

## Future Improvements

Given more time, these would be the first areas to make this project better:

Automated Testing

Search Optimization

Hierarchical Data Handling

Validation and Constraints

Docker Support

Real Database Integration




## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (v17+) or Rider

---

## Running the project

### Visual Studio (F5)

1. Open `Verne.FileSystem.sln`
2. Ensure `Verne.FileSystem.API` is set as the startup project
3. Press **F5** — the browser will open at `https://localhost:7050/swagger`

---

## API Endpoints

### Get Roor id


### Create a folder
```
POST /api/nodes/folders
Content-Type: application/json

{
  "name": "Documents",
  "parentId": <root-guid>      
}
```

### Create a file
```
POST /api/nodes/files
Content-Type: application/json

{
  "name": "report.pdf",
  "parentId": "<folder-guid>"
}
```

### List children of a folder
```
GET /api/nodes/{parentId}/children
```

### Search by exact name
```
# Global search
GET /api/nodes/search?name=report.pdf

# Within a specific folder
GET /api/nodes/search?name=report.pdf&parentId=<folder-guid>
```

### Autocomplete (top 10 starts-with)
```
GET /api/nodes/autocomplete?prefix=rep
```

### Delete a node (cascades to all children)
```
DELETE /api/nodes/{id}
```

---

## Architecture

```
Verne.FileSystem.sln
├── Verne.FileSystem.API          # Controllers, middleware, Program.cs
├── Verne.FileSystem.Application  # Services, interfaces, DTOs, DbContext
└── Verne.FileSystem.Domain       # Node entity, NodeType enum, exceptions
```


## Error responses

All errors return `{ "error": "..." }` with the appropriate HTTP status:

| Status | Scenario |
|---|---|
| `400 Bad Request` | Invalid operation (e.g. adding child to a File node) |
| `404 Not Found` | Node with given ID does not exist |
| `409 Conflict` | A node with that name already exists in the same parent |
| `204 No Content` | Successful delete |
| `201 Created` | Successful creation |
