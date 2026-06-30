using Packlead.Application.Common.Interfaces;
using Packlead.Application.Orders.DTOs;
using Packlead.Domain.Enums;
using Packlead.Domain.ValueObjects;

namespace Packlead.Application.Orders.Commands;

public class UpdateOrderCommand
{
    private readonly IOrderRepository _repository;

    public UpdateOrderCommand(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrderResponse?> ExecuteAsync(Guid id, UpdateOrderRequest request, CancellationToken ct = default)
    {
        var order = await _repository.GetByIdAsync(id, ct);
        if (order is null) return null;

        // Actualizar datos descriptivos mediante métodos de la entidad
        order.UpdateDetails(
            clientName: request.ClientName,
            clientPhoneNumber: request.ClientPhoneNumber,
            location: new Location(request.Location.Lat, request.Location.Lng),
            address: request.Address,
            zone: request.Zone,
            deliveryDate: request.DeliveryDate);

        if (request.DispatcherId is not null)
            order.AssignDispatcher(request.DispatcherId.Value);

        // Transición de estado — siempre por los métodos de dominio
        if (Enum.TryParse<OrderState>(request.State, ignoreCase: true, out var newState))
        {
            if (newState == OrderState.Shipped) order.MarkAsShipped();
            else if (newState == OrderState.Delivered) order.MarkAsDelivered();
        }

        await _repository.UpdateAsync(order, ct);
        return order.ToResponse();
    }
}