using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Packlead.Api.IntegrationTests.Infrastructure;
using Packlead.Application.Orders.DTOs;
using Packlead.Infrastructure.Persistence;

namespace Packlead.Api.IntegrationTests.Orders;

public class DispatcherIdFilterTests : IClassFixture<PackleadApiFactory>
{
    private readonly PackleadApiFactory _factory;

    public DispatcherIdFilterTests(PackleadApiFactory factory)
    {
        _factory = factory;
    }

    // I.AUTHZ.07 — Dispatcher A consulta con dispatcherId de B -> debe ignorarse el query param
    [Fact]
    public async Task GetOrders_DispatcherQueriesAnotherDispatcherId_ReturnsOnlyOwnOrders()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var dispatcherA = TestDataSeeder.SeedDispatcher(db, "Dispatcher A");
        var dispatcherB = TestDataSeeder.SeedDispatcher(db, "Dispatcher B");
        TestDataSeeder.SeedOrder(db, dispatcherA.Id, "Order for A");
        TestDataSeeder.SeedOrder(db, dispatcherB.Id, "Order for B");

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, "dispatcher");
        client.DefaultRequestHeaders.Add(TestAuthHandler.DispatcherIdHeader, dispatcherA.Id.ToString());

        var response = await client.GetAsync($"/orders?dispatcherId={dispatcherB.Id}");
        response.EnsureSuccessStatusCode();

        var orders = await response.Content.ReadFromJsonAsync<List<OrderResponse>>();

        Assert.NotNull(orders);
        Assert.All(orders!, o => Assert.Equal(dispatcherA.Id, o.DispatcherId));
        Assert.DoesNotContain(orders!, o => o.DispatcherId == dispatcherB.Id);
    }

    // I.AUTHZ.08 — Dispatcher sin query param -> solo sus propias órdenes
    [Fact]
    public async Task GetOrders_DispatcherWithoutQueryParam_ReturnsOnlyOwnOrders()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var dispatcherA = TestDataSeeder.SeedDispatcher(db, "Dispatcher A2");
        var dispatcherB = TestDataSeeder.SeedDispatcher(db, "Dispatcher B2");
        TestDataSeeder.SeedOrder(db, dispatcherA.Id);
        TestDataSeeder.SeedOrder(db, dispatcherB.Id);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, "dispatcher");
        client.DefaultRequestHeaders.Add(TestAuthHandler.DispatcherIdHeader, dispatcherA.Id.ToString());

        var response = await client.GetAsync("/orders");
        response.EnsureSuccessStatusCode();

        var orders = await response.Content.ReadFromJsonAsync<List<OrderResponse>>();

        Assert.All(orders!, o => Assert.Equal(dispatcherA.Id, o.DispatcherId));
    }

    // I.AUTHZ.09 — Admin con filtro explícito -> se respeta tal cual
    [Fact]
    public async Task GetOrders_AdminWithDispatcherIdFilter_AppliesFilterAsIs()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var dispatcherA = TestDataSeeder.SeedDispatcher(db, "Dispatcher A3");
        var dispatcherB = TestDataSeeder.SeedDispatcher(db, "Dispatcher B3");
        TestDataSeeder.SeedOrder(db, dispatcherA.Id);
        TestDataSeeder.SeedOrder(db, dispatcherB.Id);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, "admin");

        var response = await client.GetAsync($"/orders?dispatcherId={dispatcherA.Id}");
        response.EnsureSuccessStatusCode();

        var orders = await response.Content.ReadFromJsonAsync<List<OrderResponse>>();

        Assert.All(orders!, o => Assert.Equal(dispatcherA.Id, o.DispatcherId));
    }

    // I.AUTHZ.10 — Admin sin filtros -> todas las órdenes
    [Fact]
    public async Task GetOrders_AdminWithoutFilters_ReturnsAllOrders()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var dispatcherA = TestDataSeeder.SeedDispatcher(db, "Dispatcher A4");
        var dispatcherB = TestDataSeeder.SeedDispatcher(db, "Dispatcher B4");
        TestDataSeeder.SeedOrder(db, dispatcherA.Id);
        TestDataSeeder.SeedOrder(db, dispatcherB.Id);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, "admin");

        var response = await client.GetAsync("/orders");
        response.EnsureSuccessStatusCode();

        var orders = await response.Content.ReadFromJsonAsync<List<OrderResponse>>();

        Assert.True(orders!.Count >= 2);
    }
}