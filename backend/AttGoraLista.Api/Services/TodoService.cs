using AttGoraLista.Api.Data;
using AttGoraLista.Api.Dtos;
using AttGoraLista.Api.Mapping;
using AttGoraLista.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AttGoraLista.Api.Services;

// Servicen innehåller affärslogiken: här bestäms vad som faktiskt händer med
// datat när användaren skapar, ändrar eller tar bort en uppgift.
// All databasåtkomst sker via EF Core (AppDbContext). Ändringar på objekt som
// EF Core spårar skrivs till PostgreSQL först när SaveChangesAsync anropas.
public class TodoService(AppDbContext db) : ITodoService
{
    // Hämtar alla uppgifter, nyaste först, tillsammans med deras statushistorik.
    // AsNoTracking: vi ska bara läsa, så EF Core behöver inte hålla koll på
    // ändringar – det gör frågan snabbare.
    // Include(History): hämtar historikraderna i samma fråga (en JOIN).
    public async Task<IReadOnlyList<TodoResponse>> GetAllAsync(CancellationToken ct) =>
        await db.Todos.AsNoTracking().Include(x => x.History)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.ToResponse()).ToListAsync(ct);

    // Hämtar en uppgift via id. Returnerar null om den inte finns,
    // vilket controllern översätter till 404 Not Found.
    public async Task<TodoResponse?> GetByIdAsync(long id, CancellationToken ct)
    {
        var todo = await db.Todos.AsNoTracking().Include(x => x.History)
            .SingleOrDefaultAsync(x => x.Id == id, ct);
        return todo?.ToResponse();
    }

    // LÄGG TILL NY UPPGIFT (POST)
    public async Task<TodoResponse> CreateAsync(CreateTodoRequest request, CancellationToken ct)
    {
        // Samma tidsstämpel för skapad/uppdaterad så att de är exakt lika från start.
        var now = DateTime.UtcNow;

        // Bygg entiteten från förfrågan. Texten trimmas så att inledande/avslutande
        // mellanslag inte sparas. Saknas beskrivning sparas en tom sträng i stället för null.
        // Ingen historikpost skapas här – historiken loggar bara statusbyten,
        // och en ny uppgift har ännu inte bytt status.
        var todo = new TodoItem
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Priority = request.Priority,
            Status = request.Status,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Add markerar objektet som nytt; SaveChangesAsync gör INSERT i databasen.
        // Efter sparningen har databasen tilldelat todo.Id ett värde.
        db.Todos.Add(todo);
        await db.SaveChangesAsync(ct);

        // Gör om entiteten till ett svarsobjekt (DTO) som skickas till frontend.
        return todo.ToResponse();
    }

    // UPPDATERA HELA UPPGIFTEN (PUT)
    public async Task<TodoResponse?> UpdateAsync(long id, UpdateTodoRequest request, CancellationToken ct)
    {
        // Hämta uppgiften MED spårning (ingen AsNoTracking) så att EF Core märker
        // ändringarna nedan. Historiken inkluderas eftersom en post kan läggas till
        // och eftersom svaret ska innehålla hela historiken.
        var todo = await db.Todos.Include(x => x.History).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (todo is null) return null; // -> 404 i controllern

        // Om PUT även ändrar status loggas ändringen på samma sätt som PATCH.
        // Historiken måste skapas INNAN todo.Status skrivs över, annars vet vi
        // inte längre vilken status uppgiften hade tidigare.
        if (todo.Status != request.Status)
            AddHistory(todo, todo.Status, request.Status);

        // Skriv över alla redigerbara fält och stämpla när ändringen gjordes.
        todo.Title = request.Title.Trim();
        todo.Description = request.Description?.Trim() ?? string.Empty;
        todo.Priority = request.Priority;
        todo.Status = request.Status;
        todo.UpdatedAt = DateTime.UtcNow;

        // EF Core genererar UPDATE för uppgiften och ev. INSERT för den nya
        // historikposten, i en och samma transaktion.
        await db.SaveChangesAsync(ct);
        return todo.ToResponse();
    }

    // ÄNDRA STATUS (PATCH) – körs när användaren väljer ny status i rullistan.
    public async Task<TodoResponse?> ChangeStatusAsync(long id, ChangeStatusRequest request, CancellationToken ct)
    {
        // Hämta uppgiften med spårning och befintlig historik.
        var todo = await db.Todos.Include(x => x.History).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (todo is null) return null; // -> 404 i controllern

        // Samma status är ett tillåtet, idempotent anrop och skapar ingen falsk historikpost.
        if (todo.Status != request.Status)
        {
            // 1. Logga övergången gammal -> ny status (innan statusen skrivs över).
            AddHistory(todo, todo.Status, request.Status);
            // 2. Sätt den nya statusen och uppdatera tidsstämpeln.
            todo.Status = request.Status;
            todo.UpdatedAt = DateTime.UtcNow;
            // 3. Spara: UPDATE på uppgiften + INSERT i status_history, i samma transaktion.
            await db.SaveChangesAsync(ct);
        }

        // Svaret innehåller aktuell status och hela historiken (nyaste först).
        return todo.ToResponse();
    }

    // TA BORT UPPGIFT (DELETE)
    public async Task<bool> DeleteAsync(long id, CancellationToken ct)
    {
        // FindAsync letar via primärnyckeln. Historiken behöver inte laddas:
        // relationen är konfigurerad med Cascade (se AppDbContext), så databasen
        // tar automatiskt bort uppgiftens historikrader när uppgiften raderas.
        var todo = await db.Todos.FindAsync([id], ct);
        if (todo is null) return false; // -> 404 i controllern

        // Remove markerar objektet för borttagning; SaveChangesAsync gör DELETE.
        db.Todos.Remove(todo);
        await db.SaveChangesAsync(ct);
        return true; // -> 204 No Content i controllern
    }

    // Hjälpmetod som lägger till en historikpost "från -> till" på uppgiften.
    // Posten läggs i uppgiftens History-lista; EF Core ser den som ny och sätter
    // den främmande nyckeln TodoItemId automatiskt när SaveChangesAsync körs.
    private static void AddHistory(TodoItem todo, TodoStatus from, TodoStatus to) =>
        todo.History.Add(new StatusHistory { FromStatus = from, ToStatus = to, ChangedAt = DateTime.UtcNow });
}
