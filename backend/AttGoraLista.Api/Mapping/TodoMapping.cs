using AttGoraLista.Api.Dtos;
using AttGoraLista.Api.Models;

namespace AttGoraLista.Api.Mapping;

public static class TodoMapping
{
    public static TodoResponse ToResponse(this TodoItem todo) => new(
        todo.Id, todo.Title, todo.Description, todo.Priority, todo.Status,
        todo.CreatedAt, todo.UpdatedAt,
        todo.History.OrderByDescending(h => h.ChangedAt)
            .Select(h => new StatusHistoryResponse(h.Id, h.FromStatus, h.ToStatus, h.ChangedAt))
            .ToList());
}
