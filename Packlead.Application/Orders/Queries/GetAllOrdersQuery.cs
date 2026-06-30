using Packlead.Application.Common.Interfaces;
using Packlead.Application.Orders.DTOs;
using Packlead.Domain.Enums;

namespace Packlead.Application.Orders.Queries;

public class GetAllOrdersQuery
{
    private readonly IOrderRepository _repository;

    public GetAllOrdersQuery(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<OrderResponse>> ExecuteAsync(
        string? state = null,
        Guid? dispatcherId = null,
        CancellationToken ct = default)
    {
        // Convertir el string del query param al enum, si viene
        OrderState? parsedState = null;
        if (state is not null && Enum.TryParse<OrderState>(state, ignoreCase: true, out var s))
            parsedState = s;

        var orders = await _repository.GetAllAsync(parsedState, dispatcherId, ct);
        return orders.Select(o => o.ToResponse()).ToList();
    }
}