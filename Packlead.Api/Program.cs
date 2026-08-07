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

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    app.Services.GetRequiredService<FirebaseAdmin.FirebaseApp>();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<FirebaseAuthenticationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseApiOpenApiInDevelopment();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();