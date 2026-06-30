using Packlead.Application.Dispatchers.DTOs;
using Packlead.Domain.Entities;

public static class DispatcherMappingExtensions
{
    public static DispatcherResponse ToResponse(this Dispatcher dispatcher) => new()
    {
        Id = dispatcher.Id,
        Name = dispatcher.Name,
        Email = dispatcher.Email,
        Vehicle = dispatcher.Vehicle,
        LicensePlate = dispatcher.LicensePlate,
        State = dispatcher.State.ToString().ToLowerInvariant()
    };
}