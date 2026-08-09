using Moq;
using Packlead.Application.Common.Interfaces;
using Packlead.Domain.Entities;
using Packlead.Domain.Enums;

namespace Packlead.Api.IntegrationTests.Auth;

public class FirebaseAuthenticationMiddlewareTests
{
    private readonly Mock<IDispatcherRepository> _dispatcherRepository = new();

    private static Dispatcher ExistingDispatcher(string firebaseUid, DispatcherState state)
    {
        var dispatcher = new Dispatcher(firebaseUid, "Carlos Rivera", "carlos@packlead.com", "Moto", "ABC-123");
        dispatcher.SetState(state);
        return dispatcher;
    }

    // U.FAM.01 — Dispatcher inactivo -> DispatcherInactiveException (401), no debe autenticar
    [Fact]
    public async Task BuildClaimsAsync_DispatcherIsInactive_ThrowsDispatcherInactiveException()
    {
        var dispatcher = ExistingDispatcher("firebase-uid-1", DispatcherState.Inactive);
        _dispatcherRepository
            .Setup(r => r.GetByFirebaseUidAsync(dispatcher.FirebaseUid, default))
            .ReturnsAsync(dispatcher);

        await Assert.ThrowsAsync<DispatcherInactiveException>(
            () => FirebaseAuthenticationMiddleware.BuildClaimsAsync(
                dispatcher.FirebaseUid, "dispatcher", _dispatcherRepository.Object));
    }

    // U.FAM.02 — Dispatcher activo -> se resuelven los claims normalmente
    [Fact]
    public async Task BuildClaimsAsync_DispatcherIsAvailable_ReturnsClaimsWithDispatcherId()
    {
        var dispatcher = ExistingDispatcher("firebase-uid-2", DispatcherState.Available);
        _dispatcherRepository
            .Setup(r => r.GetByFirebaseUidAsync(dispatcher.FirebaseUid, default))
            .ReturnsAsync(dispatcher);

        var claims = await FirebaseAuthenticationMiddleware.BuildClaimsAsync(
            dispatcher.FirebaseUid, "dispatcher", _dispatcherRepository.Object);

        Assert.Contains(claims, c => c.Type == "dispatcherId" && c.Value == dispatcher.Id.ToString());
    }

    // U.FAM.03 — Dispatcher inexistente sigue lanzando DispatcherRecordMissingException
    [Fact]
    public async Task BuildClaimsAsync_DispatcherNotFound_ThrowsDispatcherRecordMissingException()
    {
        _dispatcherRepository
            .Setup(r => r.GetByFirebaseUidAsync("unknown-uid", default))
            .ReturnsAsync((Dispatcher?)null);

        await Assert.ThrowsAsync<DispatcherRecordMissingException>(
            () => FirebaseAuthenticationMiddleware.BuildClaimsAsync(
                "unknown-uid", "dispatcher", _dispatcherRepository.Object));
    }

    // U.FAM.04 — Role admin no consulta el repositorio de dispatchers
    [Fact]
    public async Task BuildClaimsAsync_AdminRole_DoesNotQueryDispatcherRepository()
    {
        var claims = await FirebaseAuthenticationMiddleware.BuildClaimsAsync(
            "admin-uid", "admin", _dispatcherRepository.Object);

        Assert.DoesNotContain(claims, c => c.Type == "dispatcherId");
        _dispatcherRepository.Verify(
            r => r.GetByFirebaseUidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
