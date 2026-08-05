using Packlead.Application.Common.Interfaces;
using Packlead.Application.Dispatchers.DTOs;
using Packlead.Domain.Entities;

namespace Packlead.Application.Dispatchers.Commands;

public class CreateDispatcherCommand
{
    private readonly IDispatcherRepository _repository;
    private readonly IFirebaseUserService _firebaseUserService;

    public CreateDispatcherCommand(IDispatcherRepository repository, IFirebaseUserService firebaseUserService)
    {
        _repository = repository;
        _firebaseUserService = firebaseUserService;
    }

    public async Task<CreateDispatcherResponse> ExecuteAsync(CreateDispatcherRequest request, CancellationToken ct)
    {
        var isMigration = request.FirebaseUid is not null;
        string firebaseUid;
        string? passwordResetLink = null;

        if (isMigration)
        {
            if (await _repository.ExistsByFirebaseUidAsync(request.FirebaseUid!))
                throw new DuplicateFirebaseUidException(request.FirebaseUid!);

            firebaseUid = request.FirebaseUid!;
        }
        else
        {
            firebaseUid = await _firebaseUserService.CreateDispatcherUserAsync(request.Email, ct);
            passwordResetLink = await _firebaseUserService.GeneratePasswordResetLinkAsync(request.Email, ct);
        }

        var dispatcher = new Dispatcher(
            firebaseUid: firebaseUid,
            name: request.Name,
            email: request.Email,
            vehicle: request.Vehicle,
            licensePlate: request.LicensePlate
        );

        try
        {
            await _repository.CreateAsync(dispatcher);
        }
        catch (Exception persistenceException) when (!isMigration)
        {
            try
            {
                await _firebaseUserService.DeleteUserAsync(firebaseUid, ct);
            }
            catch
            {
                // fallo del rollback
            }

            throw;
        }

        var baseResponse = dispatcher.ToResponse();
        return CreateDispatcherResponse.FromDispatcherResponse(baseResponse, passwordResetLink);
    }
}