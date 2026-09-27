using Microsoft.Extensions.Logging;
using Packlead.Application.Common.Interfaces;

namespace Packlead.Application.Orders.Commands;

public class DeleteOrderCommand
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<DeleteOrderCommand> _logger;

    public DeleteOrderCommand(IOrderRepository repository, ILogger<DeleteOrderCommand> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _repository.GetByIdAsync(id, ct);
        if (order is null) return false;

        await _repository.DeleteAsync(id, ct);

        _logger.LogInformation("Order {OrderId} deleted", id);

        return true;
    }
}