using Packlead.Application.Common.Interfaces;
using Packlead.Application.Orders.DTOs;

namespace Packlead.Application.Orders.Queries;

public class GetOrderByIdQuery
{
    private readonly IOrderRepository _repository;

    public GetOrderByIdQuery(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrderResponse?> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _repository.GetByIdAsync(id, ct);
        return order?.ToResponse();
    }
}