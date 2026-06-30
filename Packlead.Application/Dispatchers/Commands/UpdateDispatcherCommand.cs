using Packlead.Application.Common.Exceptions;
using Packlead.Application.Common.Interfaces;
using Packlead.Application.Dispatchers.DTOs;
using Packlead.Domain.Enums;

namespace Packlead.Application.Dispatchers.Commands;

public class UpdateDispatcherCommand
{
    private readonly IDispatcherRepository _repository;

    public UpdateDispatcherCommand(IDispatcherRepository repository)
    {
        _repository = repository;
    }

    public async Task<DispatcherResponse> ExecuteAsync(Guid id, UpdateDispatcherRequest request)
    {
        var dispatcher = await _repository.GetByIdAsync(id)
            ?? throw new DispatcherNotFoundException(id);

        dispatcher.UpdateDetails(
            name: request.Name,
            email: request.Email,
            vehicle: request.Vehicle,
            licensePlate: request.LicensePlate
        );

        var requestedState = Enum.Parse<DispatcherState>(request.State, ignoreCase: true);
        if (requestedState == DispatcherState.Available)
            dispatcher.SetState(DispatcherState.Available);
        else
            dispatcher.SetState(DispatcherState.Inactive);

        await _repository.UpdateAsync(dispatcher);

        return dispatcher.ToResponse();
    }
}