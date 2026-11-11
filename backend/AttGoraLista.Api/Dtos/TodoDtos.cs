using System.ComponentModel.DataAnnotations;
using AttGoraLista.Api.Models;

namespace AttGoraLista.Api.Dtos;

public record CreateTodoRequest(
    [property: Required, StringLength(200, MinimumLength = 1)] string Title,
    [property: StringLength(2000)] string? Description,
    TodoPriority Priority = TodoPriority.NORMAL,
    TodoStatus Status = TodoStatus.REGISTRERAD);

public record UpdateTodoRequest(
    [property: Required, StringLength(200, MinimumLength = 1)] string Title,
    [property: StringLength(2000)] string? Description,
    TodoPriority Priority,
    TodoStatus Status);

public record ChangeStatusRequest(TodoStatus Status);

public record StatusHistoryResponse(long Id, TodoStatus FromStatus, TodoStatus ToStatus, DateTime ChangedAt);

public record TodoResponse(
    long Id,
    string Title,
    string Description,
    TodoPriority Priority,
    TodoStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<StatusHistoryResponse> History);
