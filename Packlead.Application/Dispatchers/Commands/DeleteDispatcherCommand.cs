using Packlead.Application.Common.Exceptions;
using Packlead.Application.Common.Interfaces;

namespace Packlead.Application.Dispatchers.Commands;

public class DeleteDispatcherCommand
{
    private readonly IDispatcherRepository _repository;
    private readonly IFirebaseUserService _firebaseUserService;

    public DeleteDispatcherCommand(IDispatcherRepository repository, IFirebaseUserService firebaseUserService)
    {
        _repository = repository;
        _firebaseUserService = firebaseUserService;
    }

    public async Task ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var dispatcher = await _repository.GetByIdAsync(id)
            ?? throw new DispatcherNotFoundException();

        await _firebaseUserService.DeleteUserAsync(dispatcher.FirebaseUid, ct);

        await _repository.DeleteAsync(dispatcher.Id);
    }
}