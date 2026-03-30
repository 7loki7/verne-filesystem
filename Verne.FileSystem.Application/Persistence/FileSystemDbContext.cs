using Microsoft.EntityFrameworkCore;
using Verne.FileSystem.Domain.Entities;

namespace Verne.FileSystem.Application.Persistence;

public class FileSystemDbContext : DbContext
{
    public FileSystemDbContext(DbContextOptions<FileSystemDbContext> options)
        : base(options) { }

    public DbSet<Node> Nodes => Set<Node>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Node>(entity =>
        {
            entity.HasKey(n => n.Id);

            entity.Property(n => n.Name)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(n => n.Type)
                  .IsRequired();

            entity.HasIndex(n => new { n.Name, n.ParentId })
                  .IsUnique();
        });
    }
}

