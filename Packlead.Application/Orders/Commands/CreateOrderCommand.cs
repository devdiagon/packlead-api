using Microsoft.Extensions.Logging;
using Packlead.Application.Common.Interfaces;
using Packlead.Application.Orders.DTOs;
using Packlead.Domain.Entities;
using Packlead.Domain.ValueObjects;

namespace Packlead.Application.Orders.Commands;

public class CreateOrderCommand
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<CreateOrderCommand> _logger;

    public CreateOrderCommand(IOrderRepository repository, ILogger<CreateOrderCommand> logger)
    {
        _repository = repository;
        _logger = logger;
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

        _logger.LogInformation(
            "Order {OrderId} created for zone {Zone}, dispatcher {DispatcherId}",
            order.Id, order.Zone, order.DispatcherId);

        return order.ToResponse();
    }
}