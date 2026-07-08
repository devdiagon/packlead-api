using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Packlead.Application.Orders.Commands;
using Packlead.Application.Orders.DTOs;
using Packlead.Application.Orders.Queries;
using Packlead.Domain.Entities;
using System.Security.Claims;

namespace Packlead.Api.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly GetAllOrdersQuery _getAll;
    private readonly GetOrderByIdQuery _getById;
    private readonly CreateOrderCommand _create;
    private readonly UpdateOrderCommand _update;
    private readonly DeleteOrderCommand _delete;

    public OrdersController(
        GetAllOrdersQuery getAll,
        GetOrderByIdQuery getById,
        CreateOrderCommand create,
        UpdateOrderCommand update,
        DeleteOrderCommand delete)
    {
        _getAll = getAll;
        _getById = getById;
        _create = create;
        _update = update;
        _delete = delete;
    }

    // GET /orders?state=pending&dispatcherId=xxx
    [Authorize(Policy = "AuthenticatedOnly")]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? state,
        [FromQuery] Guid? dispatcherId,
        CancellationToken ct)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (role == "dispatcher")
        {
            var ownId = User.FindFirst("dispatcherId")?.Value;
            dispatcherId = Guid.Parse(ownId!);
        }

        var orders = await _getAll.ExecuteAsync(state, dispatcherId, ct);
        return Ok(orders);
    }

    // GET /orders/{id}
    [Authorize(Policy = "AuthenticatedOnly")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var order = await _getById.ExecuteAsync(id, ct);
        if (order is null) return NotFound();
        return Ok(order);
    }

    // POST /orders
    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var order = await _create.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    // PUT /orders/{id}
    [Authorize(Policy = "AuthenticatedOnly")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderRequest request, CancellationToken ct)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.Equals(role, "dispatcher", StringComparison.OrdinalIgnoreCase))
        {
            var ownDispatcherId = Guid.Parse(User.FindFirst("dispatcherId")!.Value);

            Console.WriteLine(ownDispatcherId);

            var existing = await _getById.ExecuteAsync(id);
            if (existing is null) return NotFound();
            if (existing.DispatcherId != ownDispatcherId)
                return Forbid();

            // Restricción de campos: el dispatcher solo puede tocar `state`,
            // no reasignar cliente, dirección, dispatcherId, etc.
            var stateOnlyRequest = request with
            {
                ClientName = existing.ClientName,
                ClientPhoneNumber = existing.ClientPhoneNumber,
                Address = existing.Address,
                Zone = existing.Zone,
                DeliveryDate = existing.DeliveryDate,
                Location = existing.Location,
                DispatcherId = existing.DispatcherId
                // State = request.State  (esto sí se respeta del request original)
            };

            return Ok(await _update.ExecuteAsync(id, stateOnlyRequest));
        }

        // admin: update completo, sin restricciones
        var order = await _update.ExecuteAsync(id, request, ct);
        if (order is null) return NotFound();
        return Ok(order);
    }

    // DELETE /orders/{id}
    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _delete.ExecuteAsync(id, ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}