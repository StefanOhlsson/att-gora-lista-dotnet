using Xunit;
using AttGoraLista.Api.Data;
using AttGoraLista.Api.Dtos;
using AttGoraLista.Api.Models;
using AttGoraLista.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace AttGoraLista.Tests;

public class TodoServiceTests
{
    private static AppDbContext CreateDb() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task CreateAsync_SavesTodo()
    {
        await using var db = CreateDb();
        var service = new TodoService(db);
        var result = await service.CreateAsync(new CreateTodoRequest("Test", "Beskrivning", TodoPriority.HOG), default);
        Assert.True(result.Id > 0);
        Assert.Equal("Test", result.Title);
        Assert.Single(db.Todos);
    }

    [Fact]
    public async Task ChangeStatusAsync_CreatesHistory()
    {
        await using var db = CreateDb();
        var todo = new TodoItem { Title = "Test" };
        db.Todos.Add(todo);
        await db.SaveChangesAsync();
        var result = await new TodoService(db).ChangeStatusAsync(todo.Id, new ChangeStatusRequest(TodoStatus.STARTAD), default);
        Assert.NotNull(result);
        Assert.Equal(TodoStatus.STARTAD, result.Status);
        Assert.Single(result.History);
        Assert.Equal(TodoStatus.REGISTRERAD, result.History[0].FromStatus);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenMissing()
    {
        await using var db = CreateDb();
        var result = await new TodoService(db).GetByIdAsync(999, default);
        Assert.Null(result);
    }
}
