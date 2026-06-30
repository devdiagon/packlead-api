namespace Packlead.Application.Orders.DTOs;

public class OrderResponse
{
    public Guid Id { get; init; }
    public Guid? DispatcherId { get; init; }
    public string ClientName { get; init; } = string.Empty;
    public string ClientPhoneNumber { get; init; } = string.Empty;
    public LocationDto Location { get; init; } = null!;
    public string? Address { get; init; }
    public string State { get; init; } = string.Empty;
    public string Zone { get; init; } = string.Empty;
    public DateTime DeliveryDate { get; init; }
    public DateTime CreatedAt { get; init; }
}
public class LocationDto
{
    public double Lat { get; init; }
    public double Lng { get; init; }
}