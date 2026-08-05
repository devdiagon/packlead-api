using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Packlead.Api.IntegrationTests.Infrastructure;
/// <summary>
/// Se controla vía headers HTTP en la request:
///   X-Test-Role:          "admin" | "dispatcher" | (ausente = sin rol / MissingRoleClaim)
///   X-Test-DispatcherId:  GUID interno del dispatcher
///   X-Test-Unauthenticated: "true" -> simula ausencia total de header Authorization
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestScheme";

    public const string RoleHeader = "X-Test-Role";
    public const string DispatcherIdHeader = "X-Test-DispatcherId";
    public const string UnauthenticatedHeader = "X-Test-Unauthenticated";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers.ContainsKey(UnauthenticatedHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-firebase-uid"),
        };

        if (Request.Headers.TryGetValue(RoleHeader, out var role) && !string.IsNullOrWhiteSpace(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role!));
        }

        if (Request.Headers.TryGetValue(DispatcherIdHeader, out var dispatcherId) &&
            !string.IsNullOrWhiteSpace(dispatcherId))
        {
            claims.Add(new Claim("dispatcherId", dispatcherId!));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
