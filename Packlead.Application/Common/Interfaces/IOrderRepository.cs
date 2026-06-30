using Packlead.Domain.Entities;
using Packlead.Domain.Enums;

namespace Packlead.Application.Common.Interfaces;

public interface IOrderRepository
{
    Task<IReadOnlyList<Order>> GetAllAsync(OrderState? state = null, Guid? dispatcherId = null, CancellationToken ct = default);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}