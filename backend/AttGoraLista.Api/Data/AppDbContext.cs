using AttGoraLista.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AttGoraLista.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> Todos => Set<TodoItem>();
    public DbSet<StatusHistory> StatusHistory => Set<StatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var todo = modelBuilder.Entity<TodoItem>();
        todo.ToTable("todos");
        todo.HasKey(x => x.Id);
        todo.Property(x => x.Title).HasMaxLength(200).IsRequired();
        todo.Property(x => x.Description).HasMaxLength(2000);
        todo.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);
        todo.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        todo.HasMany(x => x.History).WithOne(x => x.TodoItem)
            .HasForeignKey(x => x.TodoItemId).OnDelete(DeleteBehavior.Cascade);

        var history = modelBuilder.Entity<StatusHistory>();
        history.ToTable("status_history");
        history.Property(x => x.FromStatus).HasConversion<string>().HasMaxLength(20);
        history.Property(x => x.ToStatus).HasConversion<string>().HasMaxLength(20);
        history.HasIndex(x => new { x.TodoItemId, x.ChangedAt });
    }
}
