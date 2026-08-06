using Moq;
using Packlead.Application.Common.Exceptions;
using Packlead.Application.Common.Interfaces;
using Packlead.Application.Dispatchers.Commands;
using Packlead.Application.Dispatchers.DTOs;
using Packlead.Domain.Entities;

namespace Packlead.Application.Tests.Dispatchers;

public class CreateDispatcherCommandTests
{
    private readonly Mock<IDispatcherRepository> _dispatcherRepository = new();
    private readonly Mock<IFirebaseUserService> _firebaseUserService = new();

    private CreateDispatcherCommand CreateSut() =>
        new(_dispatcherRepository.Object, _firebaseUserService.Object);

    private static CreateDispatcherRequest MigrationRequest(string firebaseUid = "existing-uid") => new()
    {
        FirebaseUid = firebaseUid,
        Name = "Carlos Rivera",
        Email = "carlos@packlead.com",
        Vehicle = "Moto",
        LicensePlate = "ABC-123",
    };

    private static CreateDispatcherRequest AutomaticRequest() => new()
    {
        FirebaseUid = null,
        Name = "Ana Torres",
        Email = "ana@packlead.com",
        Vehicle = "Auto",
        LicensePlate = "XYZ-987",
    };

    // A.CDC.01 — Modo migración: FirebaseUid presente
    [Fact]
    public async Task ExecuteAsync_MigrationMode_NeverCallsFirebaseUserService()
    {
        var request = MigrationRequest();
        _dispatcherRepository
            .Setup(r => r.CreateAsync(It.IsAny<Dispatcher>(), It.IsAny<CancellationToken>()))
            .Returns((Dispatcher d, CancellationToken _) => Task.FromResult(d));

        var sut = CreateSut();

        var response = await sut.ExecuteAsync(request, CancellationToken.None);

        Assert.Null(response.PasswordResetLink);
        _firebaseUserService.Verify(
            s => s.CreateDispatcherUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _firebaseUserService.Verify(
            s => s.GeneratePasswordResetLinkAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // A.CDC.02 — Modo automático, email nuevo
    [Fact]
    public async Task ExecuteAsync_AutomaticMode_NewEmail_CreatesUserAndReturnsResetLink()
    {
        var request = AutomaticRequest();
        var newUid = "new-firebase-uid";
        const string resetLink = "https://packlead-project.firebaseapp.com/__/auth/action?...";

        var sequence = new MockSequence();
        _firebaseUserService
            .InSequence(sequence)
            .Setup(s => s.CreateDispatcherUserAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newUid);
        _firebaseUserService
            .InSequence(sequence)
            .Setup(s => s.GeneratePasswordResetLinkAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resetLink);

        _dispatcherRepository
            .Setup(r => r.CreateAsync(It.IsAny<Dispatcher>(), It.IsAny<CancellationToken>()))
            .Returns((Dispatcher d, CancellationToken _) => Task.FromResult(d));

        var sut = CreateSut();

        var response = await sut.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(resetLink, response.PasswordResetLink);
        _firebaseUserService.Verify(
            s => s.CreateDispatcherUserAsync(request.Email, It.IsAny<CancellationToken>()), Times.Once);
        _firebaseUserService.Verify(
            s => s.GeneratePasswordResetLinkAsync(request.Email, It.IsAny<CancellationToken>()), Times.Once);
    }

    // A.CDC.03 — Modo automático, email duplicado
    [Fact]
    public async Task ExecuteAsync_AutomaticMode_DuplicateEmail_ThrowsWithoutPersisting()
    {
        var request = AutomaticRequest();
        _firebaseUserService
            .Setup(s => s.CreateDispatcherUserAsync(request.Email, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateEmailException(request.Email));

        var sut = CreateSut();

        await Assert.ThrowsAsync<DuplicateEmailException>(
            () => sut.ExecuteAsync(request, CancellationToken.None));

        _dispatcherRepository.Verify(
            r => r.CreateAsync(It.IsAny<Dispatcher>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // A.CDC.04 — Rollback: falla persistencia SQL tras crear usuario en Firebase
    [Fact]
    public async Task ExecuteAsync_PersistenceFailsAfterFirebaseUserCreated_RollsBackAndRethrowsOriginal()
    {
        var request = AutomaticRequest();
        var newUid = "orphan-candidate-uid";
        var originalException = new InvalidOperationException("DB connection lost");

        _firebaseUserService
            .Setup(s => s.CreateDispatcherUserAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newUid);
        _firebaseUserService
            .Setup(s => s.GeneratePasswordResetLinkAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://reset.link");
        _dispatcherRepository
            .Setup(r => r.CreateAsync(It.IsAny<Dispatcher>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(originalException);

        var sut = CreateSut();

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.ExecuteAsync(request, CancellationToken.None));

        Assert.Same(originalException, thrown);
        _firebaseUserService.Verify(
            s => s.DeleteUserAsync(newUid, It.IsAny<CancellationToken>()), Times.Once);
    }

    // A.CDC.05 — Rollback que también falla: se relanza la excepción ORIGINAL, no la de rollback
    [Fact]
    public async Task ExecuteAsync_RollbackAlsoFails_StillRethrowsOriginalPersistenceException()
    {
        var request = AutomaticRequest();
        var newUid = "orphan-uid";
        var originalException = new InvalidOperationException("DB connection lost");
        var rollbackException = new Exception("Firebase Admin SDK unreachable");

        _firebaseUserService
            .Setup(s => s.CreateDispatcherUserAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newUid);
        _firebaseUserService
            .Setup(s => s.GeneratePasswordResetLinkAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://reset.link");
        _dispatcherRepository
            .Setup(r => r.CreateAsync(It.IsAny<Dispatcher>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(originalException);
        _firebaseUserService
            .Setup(s => s.DeleteUserAsync(newUid, It.IsAny<CancellationToken>()))
            .ThrowsAsync(rollbackException);

        var sut = CreateSut();

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.ExecuteAsync(request, CancellationToken.None));

        Assert.Same(originalException, thrown);
    }
}