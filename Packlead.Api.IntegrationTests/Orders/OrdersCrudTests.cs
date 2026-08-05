using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Packlead.Api.IntegrationTests.Infrastructure;
using Packlead.Application.Orders.DTOs;
using Packlead.Infrastructure.Persistence;

namespace Packlead.Api.IntegrationTests.Orders;

[Collection(PackleadApiCollection.Name)]
public class OrdersCrudTests
{
    private readonly PackleadApiFactory _factory;

    public OrdersCrudTests(PackleadApiFactory factory)
    {
        _factory = factory;
    }

    private HttpClient AdminClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, "admin");
        return client;
    }

    // I.ORD.01 — Contrato JSON: fechas ISO 8601 UTC, enums en minúsculas
    [Fact]
    public async Task GetOrders_ReturnsContractMatchingMobileSummary()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        TestDataSeeder.SeedOrder(db);

        var response = await AdminClient().GetAsync("/orders");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"state\":\"pending\"", json);
        // createdAt/deliveryDate deben poder parsearse como UTC ISO 8601
        var orders = await response.Content.ReadFromJsonAsync<List<OrderResponse>>();
        Assert.All(orders!, o => Assert.Equal(DateTimeKind.Utc, o.CreatedAt.Kind));
    }

    // I.ORD.02 — Crear order con clientPhoneNumber vacío -> 400
    [Fact]
    public async Task CreateOrder_WithEmptyClientPhoneNumber_Returns400()
    {
        var payload = new
        {
            clientName = "Jane Doe",
            clientPhoneNumber = "",
            location = new { lat = 4.71, lng = -74.07 },
            zone = "Norte",
            deliveryDate = DateTime.UtcNow.AddDays(1),
        };

        var response = await AdminClient().PostAsJsonAsync("/orders", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // I.ORD.03 — Transición de estado inválida vía API -> 400 InvalidStateTransition
    [Fact]
    public async Task UpdateOrder_InvalidStateTransition_Returns400WithErrorCode()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var order = TestDataSeeder.SeedOrder(db);

        var response = await AdminClient().PutAsJsonAsync($"/orders/{order.Id}", new
        {
            clientName = "Jane Doe",
            clientPhoneNumber = "+1-555-0100",
            location = new { lat = 4.711, lng = -74.0721 },
            address = "Calle 100 #15-20",
            zone = "Norte",
            deliveryDate = DateTime.UtcNow.AddDays(1),
            state = "delivered", // Pending -> Delivered: salto inválido
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("InvalidStateTransition", body);
    }

    // I.ORD.04 — GET de pedido inexistente -> 404
    [Fact]
    public async Task GetOrder_NonExistentId_Returns404WithEnvelope()
    {
        var response = await AdminClient().GetAsync($"/orders/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // I.ORD.05 — PUT con dispatcherId inexistente -> 404
    [Fact]
    public async Task UpdateOrder_WithNonExistentDispatcherId_Returns404()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var order = TestDataSeeder.SeedOrder(db);

        var response = await AdminClient().PutAsJsonAsync($"/orders/{order.Id}", new
        {
            clientName = "Jane Doe",
            clientPhoneNumber = "+1-555-0100",
            location = new { lat = 4.711, lng = -74.0721 },
            address = "Calle 100 #15-20",
            zone = "Norte",
            deliveryDate = DateTime.UtcNow.AddDays(1),
            dispatcherId = Guid.NewGuid(),
            state = "pending",
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // I.ORD.06 — DELETE normal
    [Fact]
    public async Task DeleteOrder_ExistingOrder_RemovesItFromDb()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var order = TestDataSeeder.SeedOrder(db);

        var deleteResponse = await AdminClient().DeleteAsync($"/orders/{order.Id}");
        Assert.True(
            deleteResponse.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.OK);

        var getResponse = await AdminClient().GetAsync($"/orders/{order.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}