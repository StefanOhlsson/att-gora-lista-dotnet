namespace AttGoraLista.Api.Models;

public class StatusHistory
{
    public long Id { get; set; }
    public long TodoItemId { get; set; }
    public TodoStatus FromStatus { get; set; }
    public TodoStatus ToStatus { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public TodoItem? TodoItem { get; set; }
}
