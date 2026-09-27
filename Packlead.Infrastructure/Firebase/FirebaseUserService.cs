using FirebaseAdmin.Auth;
using Microsoft.Extensions.Logging;
using Packlead.Application.Common.Exceptions;
using Packlead.Application.Common.Interfaces;
using System.Security.Cryptography;

namespace Packlead.Infrastructure.Firebase;

public sealed class FirebaseUserService : IFirebaseUserService
{
    private const string RoleClaimKey = "role";
    private const string DispatcherRoleValue = "dispatcher";

    private readonly ILogger<FirebaseUserService> _logger;

    public FirebaseUserService(ILogger<FirebaseUserService> logger)
    {
        _logger = logger;
    }

    public async Task<string> CreateDispatcherUserAsync(string email, CancellationToken ct)
    {
        var tempPassword = GenerateSecureTempPassword();

        UserRecord userRecord;
        try
        {
            userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(new UserRecordArgs
            {
                Email = email,
                Password = tempPassword,
                EmailVerified = false
            });
        }
        catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.EmailAlreadyExists)
        {
            throw new DuplicateEmailException(email);
        }
        catch (FirebaseAuthException ex)
        {
            throw new FirebaseUserCreationException(email, ex);
        }

        try
        {
            await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(
                userRecord.Uid,
                new Dictionary<string, object> { { RoleClaimKey, DispatcherRoleValue } });
        }
        catch (FirebaseAuthException ex)
        {
            // El usuario ya se creó pero el claim falló.
            await TryDeleteOrphanUserAsync(userRecord.Uid);
            throw new FirebaseUserCreationException(email, ex);
        }

        return userRecord.Uid;
    }

    public async Task<string> GeneratePasswordResetLinkAsync(string email, CancellationToken ct)
    {
        try
        {
            return await FirebaseAuth.DefaultInstance.GeneratePasswordResetLinkAsync(email);
        }
        catch (FirebaseAuthException ex)
        {
            throw new FirebaseUserCreationException(email, ex);
        }
    }

    public async Task DeleteUserAsync(string firebaseUid, CancellationToken ct)
    {
        try
        {
            await FirebaseAuth.DefaultInstance.DeleteUserAsync(firebaseUid);
        }
        catch (FirebaseAuthException ex)
        {
            throw new FirebaseUserDeletionException(firebaseUid, ex);
        }
    }

    private async Task TryDeleteOrphanUserAsync(string uid)
    {
        try
        {
            await FirebaseAuth.DefaultInstance.DeleteUserAsync(uid);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to delete orphaned Firebase user {FirebaseUid} after a custom-claims failure — user has no role claim",
                uid);
        }
    }

    private static string GenerateSecureTempPassword()
    {
        // Contraseña descartable: entra por el link de reset (GeneratePasswordResetLinkAsync).
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(24)) + "aA1!";
    }
}