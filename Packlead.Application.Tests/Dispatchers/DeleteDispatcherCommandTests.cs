using Moq;
using Packlead.Application.Common.Exceptions;
using Packlead.Application.Common.Interfaces;
using Packlead.Application.Dispatchers.Commands;
using Packlead.Domain.Entities;

namespace Packlead.Application.Tests.Dispatchers;

public class DeleteDispatcherCommandTests
{
    private readonly Mock<IDispatcherRepository> _dispatcherRepository = new();
    private readonly Mock<IFirebaseUserService> _firebaseUserService = new();

    private DeleteDispatcherCommand CreateSut() =>
        new(_dispatcherRepository.Object, _firebaseUserService.Object);

    private static Dispatcher ExistingDispatcher(string firebaseUid = "existing-uid") =>
        new(firebaseUid, "Carlos Rivera", "carlos@packlead.com", "Moto", "ABC-123");

    // A.DDC.01 — Dispatcher inexistente: no debe tocar Firebase ni la DB
    [Fact]
    public async Task ExecuteAsync_DispatcherNotFound_ThrowsWithoutCallingFirebaseOrRepository()
    {
        var id = Guid.NewGuid();
        _dispatcherRepository
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Dispatcher?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<DispatcherNotFoundException>(
            () => sut.ExecuteAsync(id, CancellationToken.None));

        _firebaseUserService.Verify(
            s => s.DeleteUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _dispatcherRepository.Verify(
            r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    // A.DDC.02 — Firebase confirma el borrado: se elimina el registro en DB
    [Fact]
    public async Task ExecuteAsync_FirebaseDeletionSucceeds_DeletesDispatcherInRepository()
    {
        var dispatcher = ExistingDispatcher();
        _dispatcherRepository
            .Setup(r => r.GetByIdAsync(dispatcher.Id))
            .ReturnsAsync(dispatcher);
        _firebaseUserService
            .Setup(s => s.DeleteUserAsync(dispatcher.FirebaseUid, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        await sut.ExecuteAsync(dispatcher.Id, CancellationToken.None);

        _firebaseUserService.Verify(
            s => s.DeleteUserAsync(dispatcher.FirebaseUid, It.IsAny<CancellationToken>()), Times.Once);
        _dispatcherRepository.Verify(
            r => r.DeleteAsync(dispatcher.Id), Times.Once);
    }

    // A.DDC.03 — Firebase falla: NO se debe borrar nada en la DB
    [Fact]
    public async Task ExecuteAsync_FirebaseDeletionFails_ThrowsAndNeverDeletesInRepository()
    {
        var dispatcher = ExistingDispatcher();
        var firebaseException = new FirebaseUserDeletionException(
            dispatcher.FirebaseUid, new InvalidOperationException("Firebase unreachable"));

        _dispatcherRepository
            .Setup(r => r.GetByIdAsync(dispatcher.Id))
            .ReturnsAsync(dispatcher);
        _firebaseUserService
            .Setup(s => s.DeleteUserAsync(dispatcher.FirebaseUid, It.IsAny<CancellationToken>()))
            .ThrowsAsync(firebaseException);

        var sut = CreateSut();

        var thrown = await Assert.ThrowsAsync<FirebaseUserDeletionException>(
            () => sut.ExecuteAsync(dispatcher.Id, CancellationToken.None));

        Assert.Same(firebaseException, thrown);
        _dispatcherRepository.Verify(
            r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}
