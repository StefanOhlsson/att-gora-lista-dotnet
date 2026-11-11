using AttGoraLista.Api.Data;
using AttGoraLista.Api.Dtos;
using AttGoraLista.Api.Mapping;
using AttGoraLista.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AttGoraLista.Api.Services;

public class TodoService(AppDbContext db) : ITodoService
{
    public async Task<IReadOnlyList<TodoResponse>> GetAllAsync(CancellationToken ct) =>
        await db.Todos.AsNoTracking().Include(x => x.History)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.ToResponse()).ToListAsync(ct);

    public async Task<TodoResponse?> GetByIdAsync(long id, CancellationToken ct)
    {
        var todo = await db.Todos.AsNoTracking().Include(x => x.History)
            .SingleOrDefaultAsync(x => x.Id == id, ct);
        return todo?.ToResponse();
    }

    public async Task<TodoResponse> CreateAsync(CreateTodoRequest request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var todo = new TodoItem
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Priority = request.Priority,
            Status = request.Status,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Todos.Add(todo);
        await db.SaveChangesAsync(ct);
        return todo.ToResponse();
    }

    public async Task<TodoResponse?> UpdateAsync(long id, UpdateTodoRequest request, CancellationToken ct)
    {
        var todo = await db.Todos.Include(x => x.History).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (todo is null) return null;

        // Om PUT även ändrar status loggas ändringen på samma sätt som PATCH.
        if (todo.Status != request.Status)
            AddHistory(todo, todo.Status, request.Status);

        todo.Title = request.Title.Trim();
        todo.Description = request.Description?.Trim() ?? string.Empty;
        todo.Priority = request.Priority;
        todo.Status = request.Status;
        todo.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return todo.ToResponse();
    }

    public async Task<TodoResponse?> ChangeStatusAsync(long id, ChangeStatusRequest request, CancellationToken ct)
    {
        var todo = await db.Todos.Include(x => x.History).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (todo is null) return null;

        // Samma status är ett tillåtet, idempotent anrop och skapar ingen falsk historikpost.
        if (todo.Status != request.Status)
        {
            AddHistory(todo, todo.Status, request.Status);
            todo.Status = request.Status;
            todo.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return todo.ToResponse();
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct)
    {
        var todo = await db.Todos.FindAsync([id], ct);
        if (todo is null) return false;
        db.Todos.Remove(todo);
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static void AddHistory(TodoItem todo, TodoStatus from, TodoStatus to) =>
        todo.History.Add(new StatusHistory { FromStatus = from, ToStatus = to, ChangedAt = DateTime.UtcNow });
}
