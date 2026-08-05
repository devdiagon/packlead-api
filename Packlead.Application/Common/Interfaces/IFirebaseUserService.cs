namespace Packlead.Application.Common.Interfaces;

public interface IFirebaseUserService
{
    Task<string> CreateDispatcherUserAsync(string email, CancellationToken ct);
    Task<string> GeneratePasswordResetLinkAsync(string email, CancellationToken ct);
    Task DeleteUserAsync(string firebaseUid, CancellationToken ct);
}