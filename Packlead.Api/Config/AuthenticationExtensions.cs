using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Packlead.Api.Handlers;
using System.Security.Claims;

namespace Packlead.Api.Config;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddFirebaseAuthAndPolicies(this IServiceCollection services)
    {
        // Authentication scheme (no-op: HttpContext.User puebla FirebaseAuthenticationMiddleware)
        services
            .AddAuthentication("Firebase")
            .AddScheme<AuthenticationSchemeOptions, NoOpAuthenticationHandler>("Firebase", _ => { });

        // Authorization policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireClaim(ClaimTypes.Role, "admin"));

            options.AddPolicy("DispatcherOnly", policy =>
                policy.RequireClaim(ClaimTypes.Role, "dispatcher"));

            options.AddPolicy("AuthenticatedOnly", policy =>
                policy.RequireAuthenticatedUser());
        });

        // Custom 401/403 envelope (en lugar del default Forbid/Challenge)
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, JsonAuthorizationResultHandler>();

        return services;
    }
}