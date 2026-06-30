using Microsoft.AspNetCore.Mvc;
using Packlead.Application.Dispatchers.Commands;
using Packlead.Application.Dispatchers.DTOs;
using Packlead.Application.Dispatchers.Queries;

namespace Packlead.Api.Controllers;

[ApiController]
[Route("dispatchers")]
public class DispatchersController : ControllerBase
{
    private readonly GetAllDispatchersQuery _getAll;
    private readonly GetDispatcherByIdQuery _getById;
    private readonly CreateDispatcherCommand _create;
    private readonly UpdateDispatcherCommand _update;
    private readonly DeleteDispatcherCommand _delete;

    public DispatchersController(
        GetAllDispatchersQuery getAll,
        GetDispatcherByIdQuery getById,
        CreateDispatcherCommand create,
        UpdateDispatcherCommand update,
        DeleteDispatcherCommand delete)
    {
        _getAll = getAll;
        _getById = getById;
        _create = create;
        _update = update;
        _delete = delete;
    }

    [HttpGet]
    public async Task<ActionResult<List<DispatcherResponse>>> GetAll()
        => Ok(await _getAll.ExecuteAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DispatcherResponse>> GetById(Guid id)
        => Ok(await _getById.ExecuteAsync(id));

    [HttpPost]
    public async Task<ActionResult<DispatcherResponse>> Create(CreateDispatcherRequest request)
    {
        var result = await _create.ExecuteAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DispatcherResponse>> Update(Guid id, UpdateDispatcherRequest request)
        => Ok(await _update.ExecuteAsync(id, request));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _delete.ExecuteAsync(id);
        return NoContent();
    }
}