using Microsoft.Extensions.Logging;
using Packlead.Application.Common.Interfaces;
using Packlead.Application.Dispatchers.DTOs;
using Packlead.Domain.Enums;

namespace Packlead.Application.Dispatchers.Commands;

public class UpdateDispatcherCommand
{
    private readonly IDispatcherRepository _repository;
    private readonly ILogger<UpdateDispatcherCommand> _logger;

    public UpdateDispatcherCommand(IDispatcherRepository repository, ILogger<UpdateDispatcherCommand> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<DispatcherResponse> ExecuteAsync(Guid id, UpdateDispatcherRequest request)
    {
        var dispatcher = await _repository.GetByIdAsync(id)
            ?? throw new DispatcherNotFoundException();

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

        _logger.LogInformation(
            "Dispatcher {DispatcherId} updated, state {State}",
            dispatcher.Id, dispatcher.State);

        return dispatcher.ToResponse();
    }
}