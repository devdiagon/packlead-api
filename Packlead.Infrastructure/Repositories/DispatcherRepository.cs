using Microsoft.EntityFrameworkCore;
using Packlead.Application.Common.Interfaces;
using Packlead.Domain.Entities;
using Packlead.Infrastructure.Persistence;

namespace Packlead.Infrastructure.Repositories;

public class DispatcherRepository : IDispatcherRepository
{
    private readonly AppDbContext _context;

    public DispatcherRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Dispatcher>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Dispatchers.ToListAsync(ct);
    }

    public async Task<Dispatcher?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Dispatchers.FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<Dispatcher?> GetByFirebaseUidAsync(string firebaseUid, CancellationToken ct = default)
    {
        return await _context.Dispatchers.FirstOrDefaultAsync(d => d.FirebaseUid == firebaseUid, ct);
    }

    public async Task<bool> ExistsByFirebaseUidAsync(string firebaseUid)
    {
        return await _context.Dispatchers
            .AnyAsync(d => d.FirebaseUid == firebaseUid);
    }

    public async Task CreateAsync(Dispatcher dispatcher, CancellationToken ct = default)
    {
        await _context.Dispatchers.AddAsync(dispatcher, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Dispatcher dispatcher, CancellationToken ct = default)
    {
        _context.Dispatchers.Update(dispatcher);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var dispatcher = await _context.Dispatchers.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (dispatcher is null) return;

        _context.Dispatchers.Remove(dispatcher);
        await _context.SaveChangesAsync(ct);
    }
}