using AttGoraLista.Api.Dtos;
using AttGoraLista.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AttGoraLista.Api.Controllers;

// Controllern är API:ets "ytterdörr": den tar emot HTTP-anrop från frontend,
// lämnar över jobbet till ITodoService och översätter resultatet till rätt HTTP-svar
// (200 OK, 201 Created, 204 No Content eller 404 Not Found).
// [ApiController] gör att ogiltiga request-objekt (t.ex. tom eller för lång rubrik)
// automatiskt besvaras med 400 Bad Request innan någon av metoderna nedan körs.
[ApiController]
[Route("api/todos")]
public class TodosController(ITodoService service) : ControllerBase
{
    // GET /api/todos
    // Hämtar alla uppgifter inklusive statushistorik. Anropas av frontend vid
    // sidladdning och efter varje ändring för att visa en uppdaterad lista.
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TodoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TodoResponse>>> GetAll(CancellationToken ct) =>
        Ok(await service.GetAllAsync(ct));

    // GET /api/todos/{id}
    // Hämtar en enskild uppgift. Finns den inte svarar vi 404.
    [HttpGet("{id:long}")]
    public async Task<ActionResult<TodoResponse>> GetById(long id, CancellationToken ct)
    {
        var todo = await service.GetByIdAsync(id, ct);
        return todo is null ? NotFound() : Ok(todo);
    }

    // POST /api/todos
    // Användaren har fyllt i formuläret "Ny uppgift" och klickat på "Lägg till".
    // Body: { title, description, priority, status }.
    [HttpPost]
    public async Task<ActionResult<TodoResponse>> Create(CreateTodoRequest request, CancellationToken ct)
    {
        var created = await service.CreateAsync(request, ct);
        // 201 Created + en Location-header som pekar på GET /api/todos/{id}
        // för den nya uppgiften. Den skapade uppgiften skickas med i svaret.
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT /api/todos/{id}
    // Ersätter alla redigerbara fält (rubrik, beskrivning, prioritet, status) på en
    // befintlig uppgift. Används inte av nuvarande frontend men finns i API:et.
    [HttpPut("{id:long}")]
    public async Task<ActionResult<TodoResponse>> Update(long id, UpdateTodoRequest request, CancellationToken ct)
    {
        var updated = await service.UpdateAsync(id, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    // PATCH /api/todos/{id}/status
    // Användaren har valt en ny status i rullistan på ett uppgiftskort.
    // Body: { status }. Endast statusen ändras, och bytet loggas i historiken.
    [HttpPatch("{id:long}/status")]
    public async Task<ActionResult<TodoResponse>> ChangeStatus(long id, ChangeStatusRequest request, CancellationToken ct)
    {
        var updated = await service.ChangeStatusAsync(id, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    // DELETE /api/todos/{id}
    // Användaren har klickat på "Ta bort" och bekräftat. Svarar 204 No Content
    // (lyckat, inget innehåll att skicka tillbaka) eller 404 om uppgiften saknas.
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct) =>
        await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
