using AttGoraLista.Api.Dtos;

namespace AttGoraLista.Api.Services;

public interface ITodoService
{
    Task<IReadOnlyList<TodoResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<TodoResponse?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<TodoResponse> CreateAsync(CreateTodoRequest request, CancellationToken cancellationToken);
    Task<TodoResponse?> UpdateAsync(long id, UpdateTodoRequest request, CancellationToken cancellationToken);
    Task<TodoResponse?> ChangeStatusAsync(long id, ChangeStatusRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken);
}
