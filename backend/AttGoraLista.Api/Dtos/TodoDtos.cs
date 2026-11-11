using System.ComponentModel.DataAnnotations;
using AttGoraLista.Api.Models;

namespace AttGoraLista.Api.Dtos;

public record CreateTodoRequest(
    [Required, StringLength(200, MinimumLength = 1)] string Title,
    [StringLength(2000)] string? Description,
    TodoPriority Priority = TodoPriority.NORMAL,
    TodoStatus Status = TodoStatus.REGISTRERAD);

public record UpdateTodoRequest(
    [Required, StringLength(200, MinimumLength = 1)] string Title,
    [StringLength(2000)] string? Description,
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
