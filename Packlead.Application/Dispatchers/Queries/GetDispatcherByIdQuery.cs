using Packlead.Application.Common.Exceptions;
using Packlead.Application.Common.Interfaces;
using Packlead.Application.Dispatchers.DTOs;

namespace Packlead.Application.Dispatchers.Queries;

public class GetDispatcherByIdQuery
{
    private readonly IDispatcherRepository _repository;

    public GetDispatcherByIdQuery(IDispatcherRepository repository)
    {
        _repository = repository;
    }

    public async Task<DispatcherResponse> ExecuteAsync(Guid id)
    {
        var dispatcher = await _repository.GetByIdAsync(id)
            ?? throw new DispatcherNotFoundException(id);

        return dispatcher.ToResponse();
    }
}