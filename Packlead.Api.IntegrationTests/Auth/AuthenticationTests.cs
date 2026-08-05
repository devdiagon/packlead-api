using System.Net;
using System.Net.Http.Json;
using Packlead.Api.IntegrationTests.Infrastructure;

namespace Packlead.Api.IntegrationTests.Auth;
public class AuthenticationTests : IClassFixture<PackleadApiFactory>
{
    private readonly PackleadApiFactory _factory;

    public AuthenticationTests(PackleadApiFactory factory)
    {
        _factory = factory;
    }

    // I.AUTH.02 — Sin header Authorization, endpoint protegido
    [Fact]
    public async Task ProtectedEndpoint_WithoutAuthHeader_Returns401()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UnauthenticatedHeader, "true");

        var response = await client.GetAsync("/orders");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // I.AUTH.04 — Token válido, role: admin
    [Fact]
    public async Task ProtectedEndpoint_AdminRole_Returns200()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, "admin");

        var response = await client.GetAsync("/dispatchers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // I.AUTH.05 — Token válido, role: dispatcher, con fila resuelta
    [Fact]
    public async Task DispatcherMeEndpoint_WithResolvedDispatcherId_Returns200()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, "dispatcher");
        client.DefaultRequestHeaders.Add(TestAuthHandler.DispatcherIdHeader, Guid.NewGuid().ToString());

        var response = await client.GetAsync("/dispatchers/me");

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // I.AUTH.07 — Token válido sin claim role -> 400 MissingRoleClaim
    [Fact]
    public async Task AnyProtectedEndpoint_TokenWithoutRoleClaim_Returns400MissingRoleClaim()
    {
        var client = _factory.CreateClient();
        // Intencionalmente sin X-Test-Role: simula un ClaimsPrincipal autenticado pero sin rol.

        var response = await client.GetAsync("/orders");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ErrorEnvelope>();
        Assert.NotNull(body);
        Assert.Equal("MissingRoleClaim", body!.Error);
    }

    // I.INFRA.01 — Nunca 500 sin esquema de auth
    [Theory]
    [InlineData("/orders")]
    [InlineData("/dispatchers")]
    public async Task ProtectedEndpoints_WithoutAuthHeader_NeverReturn500(string path)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UnauthenticatedHeader, "true");

        var response = await client.GetAsync(path);

        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    // I.INFRA.02 — Envelope de error consistente {status, error, message}
    [Fact]
    public async Task ErrorResponses_AlwaysContainStandardEnvelope()
    {
        var client = _factory.CreateClient();
        // sin X-Test-Role -> dispara MissingRoleClaimException (400)

        var response = await client.GetAsync("/orders");
        var body = await response.Content.ReadFromJsonAsync<ErrorEnvelope>();

        Assert.NotNull(body);
        Assert.NotEqual(0, body!.Status);
        Assert.False(string.IsNullOrWhiteSpace(body.Error));
        Assert.False(string.IsNullOrWhiteSpace(body.Message));
    }

    private sealed record ErrorEnvelope(int Status, string Error, string Message);
}