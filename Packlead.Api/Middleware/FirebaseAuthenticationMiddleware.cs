using FirebaseAdmin.Auth;
using Packlead.Application.Common.Exceptions;
using Packlead.Application.Common.Interfaces;
using System.Security.Claims;

public class FirebaseAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public FirebaseAuthenticationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IDispatcherRepository dispatcherRepository)
    {
        var header = context.Request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrEmpty(header) || !header.StartsWith("Bearer "))
        {
            await _next(context);
            return;
        }

        var token = header["Bearer ".Length..];
        FirebaseToken decoded;

        try
        {
            decoded = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
        }
        catch (FirebaseAuthException)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                status = 401,
                error = "Unauthorized",
                message = "Token de Firebase inválido o expirado."
            });
            return;
        }

        var role = decoded.Claims.TryGetValue("role", out var r) ? r?.ToString() : "none";
        role ??= "none";

        if (string.Equals(role, "none", StringComparison.OrdinalIgnoreCase))
        {
            throw new MissingRoleClaimException();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, decoded.Uid),
            new(ClaimTypes.Role, role)
        };

        if (string.Equals(role, "dispatcher", StringComparison.OrdinalIgnoreCase))
        {
            var dispatcher = await dispatcherRepository.GetByFirebaseUidAsync(decoded.Uid);

            if (dispatcher is null)
                throw new DispatcherRecordMissingException();

            claims.Add(new Claim("dispatcherId", dispatcher.Id.ToString()));
        }

        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Firebase"));
        await _next(context);
    }
}