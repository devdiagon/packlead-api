using FirebaseAdmin;
using FluentValidation;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Packlead.Api.Filters;
using Packlead.Api.Handlers;
using Packlead.Api.Middleware;
using Packlead.Application.Common.Interfaces;
using Packlead.Application.Dispatchers.Commands;
using Packlead.Application.Dispatchers.Queries;
using Packlead.Application.Orders.Commands;
using Packlead.Application.Orders.Queries;
using Packlead.Application.Orders.Validators;
using Packlead.Infrastructure.Persistence;
using Packlead.Infrastructure.Repositories;
using Scalar.AspNetCore;
using System.Security.Claims;

static GoogleCredential LoadServiceAccountCredential(string path)
{
    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
    var serviceAccountCredential = ServiceAccountCredential.FromServiceAccountData(stream);
    return GoogleCredential.FromServiceAccountCredential(serviceAccountCredential);
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IDispatcherRepository, DispatcherRepository>();

// Commands
builder.Services.AddScoped<CreateOrderCommand>();
builder.Services.AddScoped<UpdateOrderCommand>();
builder.Services.AddScoped<DeleteOrderCommand>();
builder.Services.AddScoped<CreateDispatcherCommand>();
builder.Services.AddScoped<UpdateDispatcherCommand>();
builder.Services.AddScoped<DeleteDispatcherCommand>();

// Queries
builder.Services.AddScoped<GetAllOrdersQuery>();
builder.Services.AddScoped<GetOrderByIdQuery>();
builder.Services.AddScoped<GetAllDispatchersQuery>();
builder.Services.AddScoped<GetDispatcherByIdQuery>();

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

// Controllers
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

// Authentication scheme (no-op: HttpContext.User puebla FirebaseAuthenticationMiddleware)
builder.Services
    .AddAuthentication("Firebase")
    .AddScheme<AuthenticationSchemeOptions, NoOpAuthenticationHandler>("Firebase", _ => { });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim(ClaimTypes.Role, "admin"));

    options.AddPolicy("DispatcherOnly", policy =>
        policy.RequireClaim(ClaimTypes.Role, "dispatcher"));

    options.AddPolicy("AuthenticatedOnly", policy =>
        policy.RequireAuthenticatedUser());
});

// Custom 401/403 envelope (en lugar del default Forbid/Challenge)
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, JsonAuthorizationResultHandler>();

// OpenApi with scalar
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((doc, ctx, ct) =>
    {
        doc.Components ??= new();
        doc.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        doc.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };

        return Task.CompletedTask;
    });
});

// Firebase configuration
var serviceAccountPath = builder.Configuration["Firebase:ServiceAccountPath"]
    ?? throw new InvalidOperationException(
        "Falta la clave 'Firebase:ServiceAccountPath' en la configuración.");

builder.Services.AddSingleton(sp =>
{
    var credential = builder.Environment.IsDevelopment()
        ? LoadServiceAccountCredential(serviceAccountPath)
        : GoogleCredential.GetApplicationDefault();

    return FirebaseApp.Create(new AppOptions { Credential = credential, ProjectId = builder.Configuration["Firebase:ProjectId"] });
});

var app = builder.Build();
app.Services.GetRequiredService<FirebaseApp>();

// Middlewares
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<FirebaseAuthenticationMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/", () => Results.Redirect("/scalar"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
