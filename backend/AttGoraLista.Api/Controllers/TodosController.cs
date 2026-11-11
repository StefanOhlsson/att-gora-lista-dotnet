using AttGoraLista.Api.Dtos;
using AttGoraLista.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AttGoraLista.Api.Controllers;

[ApiController]
[Route("api/todos")]
public class TodosController(ITodoService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TodoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TodoResponse>>> GetAll(CancellationToken ct) =>
        Ok(await service.GetAllAsync(ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<TodoResponse>> GetById(long id, CancellationToken ct)
    {
        var todo = await service.GetByIdAsync(id, ct);
        return todo is null ? NotFound() : Ok(todo);
    }

    [HttpPost]
    public async Task<ActionResult<TodoResponse>> Create(CreateTodoRequest request, CancellationToken ct)
    {
        var created = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<TodoResponse>> Update(long id, UpdateTodoRequest request, CancellationToken ct)
    {
        var updated = await service.UpdateAsync(id, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPatch("{id:long}/status")]
    public async Task<ActionResult<TodoResponse>> ChangeStatus(long id, ChangeStatusRequest request, CancellationToken ct)
    {
        var updated = await service.ChangeStatusAsync(id, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct) =>
        await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
