using Microsoft.EntityFrameworkCore;
using Verne.FileSystem.API.Middleware;
using Verne.FileSystem.Application.Interfaces;
using Verne.FileSystem.Application.Persistence;
using Verne.FileSystem.Application.Services;
using Verne.FileSystem.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Verne FileSystem API", Version = "v1" });
});

builder.Services.AddDbContext<FileSystemDbContext>(opt =>
    opt.UseInMemoryDatabase("FileSystemDb"));

builder.Services.AddScoped<INodeService, NodeService>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FileSystemDbContext>();

    if (!db.Nodes.Any())
    {
        db.Nodes.Add(Node.Create("root", NodeType.Folder, parentId: null));
        db.SaveChanges();
    }
}
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
