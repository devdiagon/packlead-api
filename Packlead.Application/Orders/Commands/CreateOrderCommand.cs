using Packlead.Application.Common.Interfaces;
using Packlead.Application.Orders.DTOs;
using Packlead.Domain.Entities;
using Packlead.Domain.ValueObjects;

namespace Packlead.Application.Orders.Commands;

public class CreateOrderCommand
{
    private readonly IOrderRepository _repository;

    public CreateOrderCommand(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrderResponse> ExecuteAsync(CreateOrderRequest request, CancellationToken ct = default)
    {
        var order = new Order(
            clientName: request.ClientName,
            clientPhoneNumber: request.ClientPhoneNumber,
            location: new Location(request.Location.Lat, request.Location.Lng),
            zone: request.Zone,
            deliveryDate: request.DeliveryDate,
            address: request.Address,
            dispatcherId: request.DispatcherId);

        await _repository.AddAsync(order, ct);
        return order.ToResponse();
    }
}