using Microsoft.Extensions.DependencyInjection;
using Moq;
using Packlead.Api.IntegrationTests.Infrastructure;
using Packlead.Application.Common.Exceptions;
using Packlead.Application.Common.Interfaces;
using Packlead.Infrastructure.Persistence;
using System.Net;
using System.Net.Http.Json;

namespace Packlead.Api.IntegrationTests.Dispatchers;

[Collection(PackleadApiCollection.Name)]
public class DispatchersCrudTests
{
    private readonly PackleadApiFactory _factory;

    public DispatchersCrudTests(PackleadApiFactory factory)
    {
        _factory = factory;
    }

    private HttpClient AdminClient(Mock<IFirebaseUserService>? firebaseMock = null)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, "admin");
        return client;
    }

    // I.DIS.01 — Alta automática, email nuevo
    [Fact]
    public async Task CreateDispatcher_AutomaticMode_NewEmail_Returns201WithResetLink()
    {
        _factory.FirebaseUserServiceMock.Reset();
        _factory.FirebaseUserServiceMock
            .Setup(s => s.CreateDispatcherUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("new-uid");
        _factory.FirebaseUserServiceMock
            .Setup(s => s.GeneratePasswordResetLinkAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://reset.link");

        var payload = new
        {
            name = "Ana Torres",
            email = "ana.torres@packlead.com",
            vehicle = "Auto",
            licensePlate = "XYZ-987",
        };

        var response = await AdminClient().PostAsJsonAsync("/dispatchers", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("passwordResetLink", body);
        Assert.DoesNotContain("null,\"passwordResetLink\"", body);
    }

    // I.DIS.02 — Alta automática, email duplicado
    [Fact]
    public async Task CreateDispatcher_AutomaticMode_DuplicateEmail_Returns409()
    {
        _factory.FirebaseUserServiceMock.Reset();
        _factory.FirebaseUserServiceMock
            .Setup(s => s.CreateDispatcherUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateEmailException("dup@packlead.com"));

        var payload = new
        {
            name = "Dup User",
            email = "dup@packlead.com",
            vehicle = "Auto",
            licensePlate = "DUP-000",
        };

        var response = await AdminClient().PostAsJsonAsync("/dispatchers", payload);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // I.DIS.03 — Modo migración
    [Fact]
    public async Task CreateDispatcher_MigrationMode_Returns201WithNullResetLink()
    {
        // MockBehavior.Strict en un mock nuevo local, ya que en modo migración
        // IFirebaseUserService no debería invocarse en absoluto.
        _factory.FirebaseUserServiceMock.Reset();

        var payload = new
        {
            firebaseUid = "already-existing-uid",
            name = "Legacy Dispatcher",
            email = "legacy@packlead.com",
            vehicle = "Moto",
            licensePlate = "LEG-001",
        };

        var response = await AdminClient().PostAsJsonAsync("/dispatchers", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"passwordResetLink\":null", body);
        _factory.FirebaseUserServiceMock.VerifyNoOtherCalls();
    }

    // I.DIS.04 — firebaseUid string vacío -> 400 desde validator, sin tocar Firebase
    [Fact]
    public async Task CreateDispatcher_EmptyFirebaseUidString_Returns400WithoutCallingFirebase()
    {
        _factory.FirebaseUserServiceMock.Reset();

        var payload = new
        {
            firebaseUid = "",
            name = "Bad Request",
            email = "bad@packlead.com",
            vehicle = "Moto",
            licensePlate = "BAD-000",
        };

        var response = await AdminClient().PostAsJsonAsync("/dispatchers", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _factory.FirebaseUserServiceMock.VerifyNoOtherCalls();
    }

    // I.DIS.05 — Respuesta de lectura no expone el link de reseteo de contraseña
    [Fact]
    public async Task GetDispatcher_RawJson_DoesNotExposeResetLink()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dispatcher = TestDataSeeder.SeedDispatcher(db);

        var response = await AdminClient().GetAsync($"/dispatchers/{dispatcher.Id}");
        var raw = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("passwordResetLink", raw, StringComparison.OrdinalIgnoreCase);
    }

    // I.DIS.06 — Cambio de estado
    [Fact]
    public async Task UpdateDispatcher_ChangesState()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dispatcher = TestDataSeeder.SeedDispatcher(db);

        var response = await AdminClient().PutAsJsonAsync($"/dispatchers/{dispatcher.Id}", new
        {
            name = dispatcher.Name,
            email = dispatcher.Email,
            vehicle = dispatcher.Vehicle,
            licensePlate = dispatcher.LicensePlate,
            state = "inactive",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // I.DIS.07 — state inválido -> 400 legible, no 500
    [Fact]
    public async Task UpdateDispatcher_InvalidState_Returns400NotServerError()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dispatcher = TestDataSeeder.SeedDispatcher(db);

        var response = await AdminClient().PutAsJsonAsync(
            $"/dispatchers/{dispatcher.Id}", new { state = "foo" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // I.DIS.08 — Delete con orders asociadas -> SetNull
    [Fact]
    public async Task DeleteDispatcher_WithAssociatedOrders_SetsOrdersDispatcherIdToNull()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dispatcher = TestDataSeeder.SeedDispatcher(db);
        var order = TestDataSeeder.SeedOrder(db, dispatcher.Id);

        var deleteResponse = await AdminClient().DeleteAsync($"/dispatchers/{dispatcher.Id}");
        Assert.True(
            deleteResponse.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.OK);

        var getOrderResponse = await AdminClient().GetAsync($"/orders/{order.Id}");
        var raw = await getOrderResponse.Content.ReadAsStringAsync();
        Assert.Contains("\"dispatcherId\":null", raw);
    }
}