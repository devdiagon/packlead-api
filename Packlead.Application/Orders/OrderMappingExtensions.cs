using Packlead.Application.Orders.DTOs;
using Packlead.Domain.Entities;

namespace Packlead.Application.Orders;

public static class OrderMappingExtensions
{
    public static OrderResponse ToResponse(this Order order) => new()
    {
        Id = order.Id,
        DispatcherId = order.DispatcherId,
        ClientName = order.ClientName,
        ClientPhoneNumber = order.ClientPhoneNumber,
        Location = new LocationDto
        {
            Lat = order.Location.Lat,
            Lng = order.Location.Lng
        },
        Address = order.Address,
        State = order.State.ToString().ToLowerInvariant(), // i.e: Pending --> "pending"
        Zone = order.Zone,
        DeliveryDate = order.DeliveryDate,
        CreatedAt = order.CreatedAt
    };
}