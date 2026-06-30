using Microsoft.AspNetCore.Mvc;
using Packlead.Application.Orders.Commands;
using Packlead.Application.Orders.DTOs;
using Packlead.Application.Orders.Queries;

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
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? state,
        [FromQuery] Guid? dispatcherId,
        CancellationToken ct)
    {
        var orders = await _getAll.ExecuteAsync(state, dispatcherId, ct);
        return Ok(orders);
    }

    // GET /orders/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var order = await _getById.ExecuteAsync(id, ct);
        if (order is null) return NotFound();
        return Ok(order);
    }

    // POST /orders
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var order = await _create.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    // PUT /orders/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderRequest request, CancellationToken ct)
    {
        var order = await _update.ExecuteAsync(id, request, ct);
        if (order is null) return NotFound();
        return Ok(order);
    }

    // DELETE /orders/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _delete.ExecuteAsync(id, ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}