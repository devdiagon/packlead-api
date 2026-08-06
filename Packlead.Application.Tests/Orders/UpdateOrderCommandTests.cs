using Moq;
using Packlead.Application.Common.Interfaces;
using Packlead.Application.Orders.Commands;
using Packlead.Application.Orders.DTOs;
using Packlead.Domain.Entities;
using Packlead.Domain.Enums;
using Packlead.Domain.ValueObjects;

namespace Packlead.Application.Tests.Orders;

public class UpdateOrderCommandTests
{
    private readonly Mock<IOrderRepository> _orderRepository = new();
    private readonly Mock<IDispatcherRepository> _dispatcherRepository = new();

    private UpdateOrderCommand CreateSut() =>
        new(_orderRepository.Object, _dispatcherRepository.Object);

    private static Order PendingOrder() => new(
        clientName: "Jane Doe",
        clientPhoneNumber: "+1-555-0100",
        location: new Location(4.711, -74.0721),
        address: "Calle 100 #15-20",
        zone: "Norte",
        deliveryDate: DateTime.UtcNow.AddDays(1));

    private static Dispatcher DispatcherWithState(DispatcherState state)
    {
        var dispatcher = new Dispatcher("uid-1", "Pedro", "pedro@packlead.com", "Moto", "ABC-123");
        if (state == DispatcherState.Inactive)
        {
            dispatcher.SetState(DispatcherState.Inactive);
        }
        return dispatcher;
    }

    private static UpdateOrderRequest FullValidRequest() => new()
    {
        ClientName = "Jane Doe",
        ClientPhoneNumber = "+1-555-0100",
        Location = new LocationDto { Lat = 4.711, Lng = -74.0721 },
        Address = "Calle 100 #15-20",
        Zone = "Norte",
        DeliveryDate = DateTime.UtcNow.AddDays(1),
        State = "pending",
        DispatcherId = null,
    };

    // A.UOC.01 — Asignar dispatcher inactivo
    [Fact]
    public async Task ExecuteAsync_AssignInactiveDispatcher_ThrowsDispatcherNotAvailable()
    {
        var orderId = Guid.NewGuid();
        var dispatcherId = Guid.NewGuid();
        var order = PendingOrder();
        var inactiveDispatcher = DispatcherWithState(DispatcherState.Inactive);

        _orderRepository.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _dispatcherRepository.Setup(r => r.GetByIdAsync(dispatcherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveDispatcher);

        var request = FullValidRequest() with { DispatcherId = dispatcherId };
        var sut = CreateSut();

        await Assert.ThrowsAsync<DispatcherNotAvailableException>(
            () => sut.ExecuteAsync(orderId, request, CancellationToken.None));
    }

    // A.UOC.02 — Asignar dispatcher inexistente
    [Fact]
    public async Task ExecuteAsync_AssignNonExistentDispatcher_ThrowsDispatcherNotFound()
    {
        var orderId = Guid.NewGuid();
        var dispatcherId = Guid.NewGuid();
        var order = PendingOrder();

        _orderRepository.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _dispatcherRepository.Setup(r => r.GetByIdAsync(dispatcherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Dispatcher?)null);

        var request = FullValidRequest() with { DispatcherId = dispatcherId };
        var sut = CreateSut();

        await Assert.ThrowsAsync<DispatcherNotFoundException>(
            () => sut.ExecuteAsync(orderId, request, CancellationToken.None));
    }

    // A.UOC.03 — Asignar dispatcher disponible
    [Fact]
    public async Task ExecuteAsync_AssignAvailableDispatcher_SetsDispatcherOnOrder()
    {
        var orderId = Guid.NewGuid();
        var dispatcherId = Guid.NewGuid();
        var order = PendingOrder();
        var availableDispatcher = DispatcherWithState(DispatcherState.Available);

        _orderRepository.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _dispatcherRepository.Setup(r => r.GetByIdAsync(dispatcherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(availableDispatcher);
        _orderRepository.Setup(r => r.UpdateAsync(order, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = FullValidRequest() with { DispatcherId = dispatcherId };
        var sut = CreateSut();

        await sut.ExecuteAsync(orderId, request, CancellationToken.None);

        Assert.Equal(dispatcherId, order.DispatcherId);
    }

    // A.UOC.04 — Transición de estado inválida propagada desde el dominio
    [Fact]
    public async Task ExecuteAsync_InvalidStateTransition_PropagatesDomainException()
    {
        var orderId = Guid.NewGuid();
        var order = PendingOrder(); // sin dispatcher asignado

        _orderRepository.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var request = FullValidRequest() with { State = "delivered" };
        var sut = CreateSut();

        await Assert.ThrowsAsync<Domain.Exceptions.InvalidStateTransitionException>(
            () => sut.ExecuteAsync(orderId, request, CancellationToken.None));
    }

    // A.UOC.05 — Pedido inexistente
    [Fact]
    public async Task ExecuteAsync_OrderDoesNotExist_ThrowsOrderNotFound()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var request = FullValidRequest();
        var sut = CreateSut();

        await Assert.ThrowsAsync<OrderNotFoundException>(
            () => sut.ExecuteAsync(orderId, request, CancellationToken.None));
    }
}
