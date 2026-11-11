using AttGoraLista.Api.Dtos;
using AttGoraLista.Api.Models;

namespace AttGoraLista.Api.Mapping;

// Översätter databasentiteter till svarsobjekt (DTO:er) som skickas som JSON till frontend.
// Entiteterna exponeras alltså aldrig direkt – bl.a. skulle navigationsegenskapen
// StatusHistory.TodoItem annars ge en cirkulär referens vid JSON-serialisering.
public static class TodoMapping
{
    public static TodoResponse ToResponse(this TodoItem todo) => new(
        todo.Id, todo.Title, todo.Description, todo.Priority, todo.Status,
        todo.CreatedAt, todo.UpdatedAt,
        // Historiken sorteras med senaste statusändringen först,
        // vilket är ordningen den visas i under "Historik" i frontend.
        todo.History.OrderByDescending(h => h.ChangedAt)
            .Select(h => new StatusHistoryResponse(h.Id, h.FromStatus, h.ToStatus, h.ChangedAt))
            .ToList());
}
