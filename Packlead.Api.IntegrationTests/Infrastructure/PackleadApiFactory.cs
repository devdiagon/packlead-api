using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Packlead.Application.Common.Interfaces;
using Packlead.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Packlead.Api.IntegrationTests.Infrastructure;

/// <summary>
/// WebApplicationFactory compartida por todos los tests de Packlead.Api.IntegrationTests.
///
/// - Levanta un contenedor PostgreSQL real vía Testcontainers.
/// - Reemplaza el AppDbContext apuntando al contenedor.
/// - Registra TestAuthHandler como esquema de auth por defecto (fake de Firebase).
/// </summary>
public class PackleadApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17.10")
        .WithDatabase("packlead_test_db")
        .WithUsername("packlead_test")
        .WithPassword("packlead_test")
        .Build();

    public Mock<IFirebaseUserService> FirebaseUserServiceMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 1. Reemplazar el DbContext por uno apuntando al contenedor de test.
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));

            // 2. Reemplazar el esquema de auth real (Firebase) por TestAuthHandler.
            services
                .AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });

            // 3. Reemplazar IFirebaseUserService por el mock único y persistente del factory.
            var firebaseUserServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IFirebaseUserService));
            if (firebaseUserServiceDescriptor is not null)
            {
                services.Remove(firebaseUserServiceDescriptor);
            }

            services.AddScoped(_ => FirebaseUserServiceMock.Object);
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}