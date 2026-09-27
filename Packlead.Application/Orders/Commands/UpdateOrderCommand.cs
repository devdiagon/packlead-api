using Microsoft.Extensions.Logging;
using Packlead.Application.Common.Interfaces;
using Packlead.Application.Orders.DTOs;
using Packlead.Domain.Enums;
using Packlead.Domain.ValueObjects;

namespace Packlead.Application.Orders.Commands;

public class UpdateOrderCommand
{
    private readonly IOrderRepository _repository;
    private readonly IDispatcherRepository _dispatcherRepository;
    private readonly ILogger<UpdateOrderCommand> _logger;

    public UpdateOrderCommand(
        IOrderRepository repository,
        IDispatcherRepository dispatcherRepository,
        ILogger<UpdateOrderCommand> logger)
    {
        _repository = repository;
        _dispatcherRepository = dispatcherRepository;
        _logger = logger;
    }

    public async Task<OrderResponse> ExecuteAsync(Guid id, UpdateOrderRequest request, CancellationToken ct = default)
    {
        var order = await _repository.GetByIdAsync(id, ct);
        if (order is null)
            throw new OrderNotFoundException();

        // Actualizar datos descriptivos mediante métodos de la entidad
        order.UpdateDetails(
            clientName: request.ClientName,
            clientPhoneNumber: request.ClientPhoneNumber,
            location: new Location(request.Location.Lat, request.Location.Lng),
            address: request.Address,
            zone: request.Zone,
            deliveryDate: request.DeliveryDate);

        if (request.DispatcherId is not null)
        {
            // Solo consultar la DB si efectivamente se está cambiando el dispatcher asignado
            if (order.DispatcherId != request.DispatcherId.Value)
            {
                var dispatcher = await _dispatcherRepository.GetByIdAsync(request.DispatcherId.Value, ct);

                if (dispatcher is null)
                    throw new DispatcherNotFoundException();

                if (dispatcher.State != DispatcherState.Available)
                    throw new DispatcherNotAvailableException("El repartidor no se encuentra disponible.");
            }
            order.AssignDispatcher(request.DispatcherId.Value);
        }

        // Transición de estado — siempre por los métodos de dominio
        if (Enum.TryParse<OrderState>(request.State, ignoreCase: true, out var newState))
        {
            if (newState == OrderState.Shipped) order.MarkAsShipped();
            else if (newState == OrderState.Delivered) order.MarkAsDelivered();
        }

        await _repository.UpdateAsync(order, ct);

        _logger.LogInformation(
            "Order {OrderId} updated, state {State}, dispatcher {DispatcherId}",
            order.Id, order.State, order.DispatcherId);

        return order.ToResponse();
    }
}