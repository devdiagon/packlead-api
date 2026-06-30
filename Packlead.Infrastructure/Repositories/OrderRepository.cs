using Microsoft.EntityFrameworkCore;
using Packlead.Application.Common.Interfaces;
using Packlead.Domain.Entities;
using Packlead.Domain.Enums;
using Packlead.Infrastructure.Persistence;

namespace Packlead.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(OrderState? state = null, Guid? dispatcherId = null, CancellationToken ct = default)
    {
        var query = _context.Orders.AsQueryable();

        if (state is not null)
            query = query.Where(o => o.State == state);

        if (dispatcherId is not null)
            query = query.Where(o => o.DispatcherId == dispatcherId);

        return await query.ToListAsync(ct);
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        await _context.Orders.AddAsync(order, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null) return;

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync(ct);
    }
}