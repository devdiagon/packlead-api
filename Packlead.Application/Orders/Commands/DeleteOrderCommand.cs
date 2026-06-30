using Packlead.Application.Common.Interfaces;

namespace Packlead.Application.Orders.Commands;

public class DeleteOrderCommand
{
    private readonly IOrderRepository _repository;

    public DeleteOrderCommand(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _repository.GetByIdAsync(id, ct);
        if (order is null) return false;

        await _repository.DeleteAsync(id, ct);
        return true;
    }
}