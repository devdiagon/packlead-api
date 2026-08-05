using Packlead.Domain.Entities;
using Packlead.Domain.Enums;
using Packlead.Domain.Exceptions;
using Packlead.Domain.ValueObjects;

namespace Packlead.Domain.Tests.Entities;
public class OrderTests
{
    private static Order NewPendingOrder(Guid? dispatcherId = null)
    {
        var order = new Order(
            clientName: "Jane Doe",
            clientPhoneNumber: "+1-555-0100",
            location: new Location(4.711, -74.0721),
            address: "Calle 100 #15-20",
            zone: "Norte",
            deliveryDate: DateTime.UtcNow.AddDays(1));

        if (dispatcherId is { } id)
        {
            order.AssignDispatcher(id);
        }

        return order;
    }

    // D.ORD.01 — Marcar como shipped con dispatcher asignado
    [Fact]
    public void MarkAsShipped_WithDispatcherAssigned_TransitionsToShipped()
    {
        var order = NewPendingOrder(Guid.NewGuid());

        order.MarkAsShipped();

        Assert.Equal(OrderState.Shipped, order.State);
    }

    // D.ORD.02 — Marcar como shipped sin dispatcher asignado
    [Fact]
    public void MarkAsShipped_WithoutDispatcher_ThrowsInvalidStateTransition()
    {
        var order = NewPendingOrder();

        Assert.Throws<InvalidStateTransitionException>(() => order.MarkAsShipped());
    }

    // D.ORD.03 — Marcar como delivered desde shipped
    [Fact]
    public void MarkAsDelivered_FromShipped_TransitionsToDelivered()
    {
        var order = NewPendingOrder(Guid.NewGuid());
        order.MarkAsShipped();

        order.MarkAsDelivered();

        Assert.Equal(OrderState.Delivered, order.State);
    }

    // D.ORD.04 — Marcar como delivered desde pending (salto inválido)
    [Fact]
    public void MarkAsDelivered_FromPending_ThrowsInvalidStateTransition()
    {
        var order = NewPendingOrder(Guid.NewGuid());

        Assert.Throws<InvalidStateTransitionException>(() => order.MarkAsDelivered());
    }

    // D.ORD.05 — Asignar dispatcher con GUID vacío
    [Fact]
    public void AssignDispatcher_WithEmptyGuid_ThrowsDomainException_NotGeneric500()
    {
        var order = NewPendingOrder();

        // Debe ser una excepción de dominio tipada, no un ArgumentException/500 genérico.
        Assert.Throws<InvalidDispatcherIdException>(() => order.AssignDispatcher(Guid.Empty));
    }

    // D.ORD.06 — Asignar dispatcher con GUID válido
    [Fact]
    public void AssignDispatcher_WithValidGuid_SetsDispatcherId()
    {
        var order = NewPendingOrder();
        var dispatcherId = Guid.NewGuid();

        order.AssignDispatcher(dispatcherId);

        Assert.Equal(dispatcherId, order.DispatcherId);
    }
}