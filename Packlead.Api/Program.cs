using Microsoft.AspNetCore.HttpOverrides;
using Packlead.Api.Config;
using Packlead.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddApiValidation();
builder.Services.AddFirebaseAuthAndPolicies();
builder.Services.AddApiOpenApi();

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddFirebaseAdmin(builder.Configuration, builder.Environment);
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsEnvironment("Testing"))
{
    app.Services.GetRequiredService<FirebaseAdmin.FirebaseApp>();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<FirebaseAuthenticationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseApiOpenApiInDevelopment();

app.MapControllers();

app.Run();