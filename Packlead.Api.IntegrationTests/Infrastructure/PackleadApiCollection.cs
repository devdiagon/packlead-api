namespace Packlead.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Colección xUnit compartida por TODAS las clases de Packlead.Api.IntegrationTests.
///
/// Garantiza una única instancia de PackleadApiFactory (un solo host, FirebaseApp, Postgres) 
/// para toda la ejecución. Evita que xUnit paralelize automáticamente las tests.
/// </summary>

[CollectionDefinition(Name)]
public class PackleadApiCollection : ICollectionFixture<PackleadApiFactory>
{
    public const string Name = "Packlead API integration tests";
}
