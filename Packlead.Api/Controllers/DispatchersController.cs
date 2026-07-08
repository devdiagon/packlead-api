using Microsoft.AspNetCore.Authorization;
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

    [Authorize(Policy = "AdminOnly")]
    [HttpGet]
    public async Task<ActionResult<List<DispatcherResponse>>> GetAll()
        => Ok(await _getAll.ExecuteAsync());

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DispatcherResponse>> GetById(Guid id)
        => Ok(await _getById.ExecuteAsync(id));

    [Authorize(Policy = "DispatcherOnly")]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var dispatcherIdClaim = User.FindFirst("dispatcherId")?.Value;
        if (!Guid.TryParse(dispatcherIdClaim, out var dispatcherId))
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "InconsistentState",
                detail: "Ocurrió un error inesperado procesando el token.");
        }
        return Ok(await _getById.ExecuteAsync(dispatcherId));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<ActionResult<DispatcherResponse>> Create(CreateDispatcherRequest request)
    {
        var result = await _create.ExecuteAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DispatcherResponse>> Update(Guid id, UpdateDispatcherRequest request)
        => Ok(await _update.ExecuteAsync(id, request));

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _delete.ExecuteAsync(id);
        return NoContent();
    }
}