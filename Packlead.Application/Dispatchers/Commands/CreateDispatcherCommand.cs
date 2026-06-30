using Packlead.Application.Common.Exceptions;
using Packlead.Application.Common.Interfaces;
using Packlead.Application.Dispatchers.DTOs;
using Packlead.Domain.Entities;

namespace Packlead.Application.Dispatchers.Commands;

public class CreateDispatcherCommand
{
    private readonly IDispatcherRepository _repository;

    public CreateDispatcherCommand(IDispatcherRepository repository)
    {
        _repository = repository;
    }

    public async Task<DispatcherResponse> ExecuteAsync(CreateDispatcherRequest request)
    {
        if (await _repository.ExistsByFirebaseUidAsync(request.FirebaseUid))
            throw new DuplicateFirebaseUidException(request.FirebaseUid);

        var dispatcher = new Dispatcher(
            firebaseUid: request.FirebaseUid,
            name: request.Name,
            email: request.Email,
            vehicle: request.Vehicle,
            licensePlate: request.LicensePlate
        );

        await _repository.CreateAsync(dispatcher);

        return dispatcher.ToResponse();
    }
}