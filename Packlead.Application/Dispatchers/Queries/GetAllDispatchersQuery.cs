using Packlead.Application.Common.Interfaces;
using Packlead.Application.Dispatchers.DTOs;

namespace Packlead.Application.Dispatchers.Queries;

public class GetAllDispatchersQuery
{
    private readonly IDispatcherRepository _repository;

    public GetAllDispatchersQuery(IDispatcherRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<DispatcherResponse>> ExecuteAsync()
    {
        var dispatchers = await _repository.GetAllAsync();
        return dispatchers.Select(d => d.ToResponse()).ToList();
    }
}