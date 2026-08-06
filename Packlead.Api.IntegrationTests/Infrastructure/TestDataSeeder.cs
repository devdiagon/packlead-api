using Packlead.Domain.Entities;
using Packlead.Domain.ValueObjects;
using Packlead.Infrastructure.Persistence;

namespace Packlead.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Helpers de seed reutilizables entre los tests de integración.
/// </summary>
internal static class TestDataSeeder
{
    public static Dispatcher SeedDispatcher(
        AppDbContext db,
        string name = "Isaias Merino",
        string? firebaseUid = null)
    {
        var uid = firebaseUid ?? $"firebase-uid-{Guid.NewGuid():N}";
        var email = $"{name.Replace(" ", ".").ToLower()}.{Guid.NewGuid():N}@packlead.com";

        var dispatcher = new Dispatcher(uid, name, email, "Moto", "ABC-123");
        db.Dispatchers.Add(dispatcher);
        db.SaveChanges();
        return dispatcher;
    }

    public static Order SeedOrder(AppDbContext db, Guid? dispatcherId = null, string clientName = "Jane Doe")
    {
        var order = new Order(
            clientName: clientName,
            clientPhoneNumber: "+1-555-0100",
            location: new Location(4.711, -74.0721),
            address: "Calle 100 #15-20",
            zone: "Norte",
            deliveryDate: DateTime.UtcNow.AddDays(1));

        if (dispatcherId is { } id)
        {
            order.AssignDispatcher(id);
        }

        db.Orders.Add(order);
        db.SaveChanges();
        return order;
    }
}