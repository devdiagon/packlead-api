using Packlead.Application.Common.Exceptions;
using Packlead.Application.Common.Interfaces;

namespace Packlead.Application.Dispatchers.Commands;

public class DeleteDispatcherCommand
{
    private readonly IDispatcherRepository _repository;

    public DeleteDispatcherCommand(IDispatcherRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(Guid id)
    {
        var dispatcher = await _repository.GetByIdAsync(id)
            ?? throw new DispatcherNotFoundException(id);

        await _repository.DeleteAsync(dispatcher.Id);
    }
}