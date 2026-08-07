using Packlead.Application.Dispatchers.DTOs;
using Packlead.Domain.Entities;

namespace Packlead.Application.Dispatchers;

public static class DispatcherMappingExtensions
{
    public static DispatcherResponse ToResponse(this Dispatcher dispatcher) => new()
    {
        Id = dispatcher.Id,
        FirebaseUid = dispatcher.FirebaseUid,
        Name = dispatcher.Name,
        Email = dispatcher.Email,
        Vehicle = dispatcher.Vehicle,
        LicensePlate = dispatcher.LicensePlate,
        State = dispatcher.State.ToString().ToLowerInvariant()
    };
}