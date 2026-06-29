using Packlead.Domain.Enums;
using Packlead.Domain.ValueObjects;

namespace Packlead.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public Guid? DispatcherId { get; private set; }
    public string ClientName { get; private set; } = string.Empty;
    public string ClientPhoneNumber { get; private set; } = string.Empty;
    public Location Location { get; private set; } = null!;
    public string? Address { get; private set; }
    public OrderState State { get; private set; }
    public string Zone { get; private set; } = string.Empty;
    public DateTime DeliveryDate { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Order() { }

    public Order(
        string clientName,
        string clientPhoneNumber,
        Location location,
        string zone,
        DateTime deliveryDate,
        string? address = null,
        Guid? dispatcherId = null)
    {
        Id = Guid.NewGuid();
        ClientName = clientName;
        ClientPhoneNumber = clientPhoneNumber;
        Location = location;
        Address = address;
        Zone = zone;
        DeliveryDate = deliveryDate;
        DispatcherId = dispatcherId;
        State = OrderState.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void AssignDispatcher(Guid dispatcherId)
    {
        DispatcherId = dispatcherId;
    }

    // Update state
    public void MarkAsShipped()
    {
        if (State != OrderState.Pending)
            throw new InvalidOperationException($"Cannot ship an order in state '{State}'. Must be 'Pending'.");

        if (DispatcherId is null)
            throw new InvalidOperationException("Cannot ship an order with no dispatcher assigned.");

        State = OrderState.Shipped;
    }

    public void MarkAsDelivered()
    {
        if (State != OrderState.Shipped)
            throw new InvalidOperationException($"Cannot deliver an order in state '{State}'. Must be 'Shipped'.");

        State = OrderState.Delivered;
    }
}