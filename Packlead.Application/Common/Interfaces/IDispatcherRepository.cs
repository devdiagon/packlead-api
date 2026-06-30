using Packlead.Domain.Entities;

namespace Packlead.Application.Common.Interfaces;

public interface IDispatcherRepository
{
    Task<IReadOnlyList<Dispatcher>> GetAllAsync(CancellationToken ct = default);
    Task<Dispatcher?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Dispatcher?> GetByFirebaseUidAsync(string firebaseUid, CancellationToken ct = default);
    Task AddAsync(Dispatcher dispatcher, CancellationToken ct = default);
    Task UpdateAsync(Dispatcher dispatcher, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}