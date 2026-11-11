namespace AttGoraLista.Api.Models;

public class TodoItem
{
    public long Id { get; set; }
    public required string Title { get; set; }
    public string Description { get; set; } = string.Empty;
    public TodoPriority Priority { get; set; } = TodoPriority.NORMAL;
    public TodoStatus Status { get; set; } = TodoStatus.REGISTRERAD;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigationsegenskapen låter EF Core läsa historiken tillsammans med uppgiften.
    public List<StatusHistory> History { get; set; } = [];
}
