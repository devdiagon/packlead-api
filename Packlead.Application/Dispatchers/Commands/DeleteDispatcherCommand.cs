using Microsoft.Extensions.Logging;
using Packlead.Application.Common.Interfaces;

namespace Packlead.Application.Dispatchers.Commands;

public class DeleteDispatcherCommand
{
    private readonly IDispatcherRepository _repository;
    private readonly IFirebaseUserService _firebaseUserService;
    private readonly ILogger<DeleteDispatcherCommand> _logger;

    public DeleteDispatcherCommand(
        IDispatcherRepository repository,
        IFirebaseUserService firebaseUserService,
        ILogger<DeleteDispatcherCommand> logger)
    {
        _repository = repository;
        _firebaseUserService = firebaseUserService;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var dispatcher = await _repository.GetByIdAsync(id)
            ?? throw new DispatcherNotFoundException();

        await _firebaseUserService.DeleteUserAsync(dispatcher.FirebaseUid, ct);

        await _repository.DeleteAsync(dispatcher.Id);

        _logger.LogInformation("Dispatcher {DispatcherId} deleted", dispatcher.Id);
    }
}