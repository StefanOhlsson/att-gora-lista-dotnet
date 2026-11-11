using AttGoraLista.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AttGoraLista.Api.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext db)
    {
        // Seedning är idempotent: data läggs bara in när tabellen är tom.
        if (await db.Todos.AnyAsync()) return;

        var now = DateTime.UtcNow;
        var todos = new[]
        {
            New("Planera sprint", "Gå igenom backlog och prioriteringar.", TodoPriority.HOG, TodoStatus.STARTAD, 1),
            New("Förbered demo", "Visa .NET-implementationen för teamet.", TodoPriority.KRITISK, TodoStatus.REGISTRERAD, 2),
            New("Skriv tester", "Komplettera service- och API-tester.", TodoPriority.HOG, TodoStatus.REGISTRERAD, 3),
            New("Uppdatera README", "Dokumentera start, API och arkitektur.", TodoPriority.NORMAL, TodoStatus.AVSLUTAD, 4),
            New("Granska loggning", "Kontrollera loggnivåer och felutskrifter.", TodoPriority.NORMAL, TodoStatus.REGISTRERAD, 5),
            New("Kontrollera CORS", "Begränsa origins inför produktion.", TodoPriority.HOG, TodoStatus.REGISTRERAD, 6),
            New("Skapa migrering", "Ersätt EnsureCreated i nästa iteration.", TodoPriority.NORMAL, TodoStatus.REGISTRERAD, 7),
            New("Verifiera Docker", "Bygg hela lösningen utan lokala beroenden.", TodoPriority.HOG, TodoStatus.STARTAD, 8),
            New("Rensa backlog", "Ta bort inaktuella uppgifter.", TodoPriority.LAG, TodoStatus.REGISTRERAD, 9),
            New("Planera release", "Definiera versionsnummer och release notes.", TodoPriority.NORMAL, TodoStatus.REGISTRERAD, 10)
        };

        db.Todos.AddRange(todos);
        await db.SaveChangesAsync();

        // Lägg till historik för de exempel som redan har bytt status.
        foreach (var todo in todos.Where(t => t.Status != TodoStatus.REGISTRERAD))
        {
            todo.History.Add(new StatusHistory
            {
                FromStatus = TodoStatus.REGISTRERAD,
                ToStatus = todo.Status,
                ChangedAt = todo.UpdatedAt
            });
        }
        await db.SaveChangesAsync();

        TodoItem New(string title, string description, TodoPriority priority, TodoStatus status, int daysAgo) =>
            new() { Title = title, Description = description, Priority = priority, Status = status,
                CreatedAt = now.AddDays(-daysAgo), UpdatedAt = now.AddDays(-daysAgo).AddHours(2) };
    }
}
