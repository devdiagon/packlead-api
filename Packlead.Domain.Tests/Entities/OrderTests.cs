using Packlead.Domain.Entities;
using Packlead.Domain.Enums;
using Packlead.Domain.Exceptions;
using Packlead.Domain.ValueObjects;
using Xunit;

namespace Packlead.Domain.Tests.Entities;

public class OrderTests
{
    private static Order CreateValidOrder()
    {
        return new Order(
            clientName: "Jane Doe",
            clientPhoneNumber: "+1-555-0100",
            location: new Location(4.711, -74.0721),
            zone: "Norte",
            deliveryDate: DateTime.UtcNow.AddDays(1));
    }

    [Fact]
    public void New_Order_Starts_In_Pending_State()
    {
        var order = CreateValidOrder();

        Assert.Equal(OrderState.Pending, order.State);
    }

    [Fact]
    public void MarkAsShipped_Throws_When_No_Dispatcher_Assigned()
    {
        var order = CreateValidOrder();

        var ex = Assert.Throws<InvalidStateTransitionException>(() => order.MarkAsShipped());
        Assert.Contains("no dispatcher assigned", ex.Message);
    }

    [Fact]
    public void MarkAsShipped_Succeeds_When_Dispatcher_Assigned()
    {
        var order = CreateValidOrder();
        order.AssignDispatcher(Guid.NewGuid());

        order.MarkAsShipped();

        Assert.Equal(OrderState.Shipped, order.State);
    }

    [Fact]
    public void MarkAsDelivered_Throws_When_Order_Is_Still_Pending()
    {
        var order = CreateValidOrder();

        Assert.Throws<InvalidStateTransitionException>(() => order.MarkAsDelivered());
    }

    [Fact]
    public void Full_Lifecycle_Pending_To_Shipped_To_Delivered_Succeeds()
    {
        var order = CreateValidOrder();
        order.AssignDispatcher(Guid.NewGuid());

        order.MarkAsShipped();
        order.MarkAsDelivered();

        Assert.Equal(OrderState.Delivered, order.State);
    }

    [Theory]
    [InlineData(91, 0)]
    [InlineData(-91, 0)]
    [InlineData(0, 181)]
    [InlineData(0, -181)]
    public void Location_Rejects_Out_Of_Range_Coordinates(double lat, double lng)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Location(lat, lng));
    }
}