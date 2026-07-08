using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace Packlead.Api.Config;

public static class OpenApiExtensions
{
    public static IServiceCollection AddApiOpenApi(this IServiceCollection services)
    {
        // OpenApi with scalar
        services.AddOpenApi(options =>
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

        return services;
    }

    public static WebApplication UseApiOpenApiInDevelopment(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
            app.MapGet("/", () => Results.Redirect("/scalar"));
        }

        return app;
    }
}