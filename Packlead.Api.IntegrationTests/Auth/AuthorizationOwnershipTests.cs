using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Packlead.Api.IntegrationTests.Infrastructure;
using Packlead.Infrastructure.Persistence;

namespace Packlead.Api.IntegrationTests.Auth;

[Collection(PackleadApiCollection.Name)]
public class AuthorizationOwnershipTests
{
    private readonly PackleadApiFactory _factory;

    public AuthorizationOwnershipTests(PackleadApiFactory factory)
    {
        _factory = factory;
    }

    // I.AUTHZ.01 — Dispatcher intenta crear un pedido
    [Fact]
    public async Task Dispatcher_CreateOrder_IfAdminOnly_Returns403()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, "dispatcher");
        client.DefaultRequestHeaders.Add(TestAuthHandler.DispatcherIdHeader, Guid.NewGuid().ToString());

        var response = await client.PostAsJsonAsync("/orders", new
        {
            clientName = "X",
            clientPhoneNumber = "+1-555-0000",
            location = new { lat = 0, lng = 0 },
            zone = "Norte",
            deliveryDate = DateTime.UtcNow.AddDays(1),
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // I.AUTHZ.02 — Dispatcher intenta eliminar dispatcher
    [Fact]
    public async Task Dispatcher_DeleteDispatcher_Returns403()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var target = TestDataSeeder.SeedDispatcher(db);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, "dispatcher");
        client.DefaultRequestHeaders.Add(TestAuthHandler.DispatcherIdHeader, Guid.NewGuid().ToString());

        var response = await client.DeleteAsync($"/dispatchers/{target.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // I.AUTHZ.03 — Dispatcher A modifica pedido de dispatcher B
    [Fact]
    public async Task Dispatcher_UpdatesOrderOfAnotherDispatcher_Returns403()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dispatcherA = TestDataSeeder.SeedDispatcher(db, "A");
        var dispatcherB = TestDataSeeder.SeedDispatcher(db, "B");
        var order = TestDataSeeder.SeedOrder(db, dispatcherB.Id);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, "dispatcher");
        client.DefaultRequestHeaders.Add(TestAuthHandler.DispatcherIdHeader, dispatcherA.Id.ToString());

        var response = await client.PutAsJsonAsync($"/orders/{order.Id}", new
        {
            clientName = "Jane Doe",
            clientPhoneNumber = "+1-555-0100",
            location = new { lat = 4.711, lng = -74.0721 },
            address = "Calle 100 #15-20",
            zone = "Norte",
            deliveryDate = DateTime.UtcNow.AddDays(1),
            state = "shipped",
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // I.AUTHZ.04 — Dispatcher modifica campos no permitidos de su propio pedido
    [Fact]
    public async Task Dispatcher_UpdatesOwnOrder_OnlyStateIsApplied()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dispatcher = TestDataSeeder.SeedDispatcher(db);
        var order = TestDataSeeder.SeedOrder(db, dispatcher.Id, clientName: "Original Name");

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, "dispatcher");
        client.DefaultRequestHeaders.Add(TestAuthHandler.DispatcherIdHeader, dispatcher.Id.ToString());

        var response = await client.PutAsJsonAsync($"/orders/{order.Id}", new
        {
            clientName = "Attempted Overwrite",
            clientPhoneNumber = "+1-555-0100",
            location = new { lat = 4.711, lng = -74.0721 },
            address = "Calle 100 #15-20",
            zone = "Norte",
            deliveryDate = DateTime.UtcNow.AddDays(1),
            state = "shipped",
        });

        response.EnsureSuccessStatusCode();
        var raw = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"clientName\":\"Original Name\"", raw);
        Assert.Contains("\"state\":\"shipped\"", raw);
    }

    // I.AUTHZ.05 — GET /dispatchers/me como dispatcher
    [Fact]
    public async Task GetDispatcherMe_AsDispatcher_ReturnsOwnRecord()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dispatcher = TestDataSeeder.SeedDispatcher(db);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, "dispatcher");
        client.DefaultRequestHeaders.Add(TestAuthHandler.DispatcherIdHeader, dispatcher.Id.ToString());

        var response = await client.GetAsync("/dispatchers/me");

        response.EnsureSuccessStatusCode();
        var raw = await response.Content.ReadAsStringAsync();
        Assert.Contains(dispatcher.Id.ToString(), raw);
    }

    // I.AUTHZ.06 — GET /dispatchers/me como admin
    [Fact]
    public async Task GetDispatcherMe_AsAdmin_DocumentsActualBehavior()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, "admin");

        var response = await client.GetAsync("/dispatchers/me");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}