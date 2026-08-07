using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Packlead.Application.Common.Interfaces;
using Packlead.Infrastructure.Firebase;

namespace Packlead.Api.Config;

public static class FirebaseExtensions
{
    public static IServiceCollection AddFirebaseAdmin(
        this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddSingleton(_ =>
        {
            var credential = environment.IsDevelopment()
                ? LoadServiceAccountCredential(
                    configuration["Firebase:ServiceAccountPath"]
                        ?? throw new InvalidOperationException(
                            "Falta la clave 'Firebase:ServiceAccountPath' en la configuración."))
                : GoogleCredential.GetApplicationDefault();

            return FirebaseApp.Create(new AppOptions
            {
                Credential = credential,
                ProjectId = configuration["Firebase:ProjectId"]
            });
        });

        services.AddScoped<IFirebaseUserService, FirebaseUserService>();

        return services;
    }

    // Load the service account credential from the specified path
    private static GoogleCredential LoadServiceAccountCredential(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        var serviceAccountCredential = ServiceAccountCredential.FromServiceAccountData(stream);
        return GoogleCredential.FromServiceAccountCredential(serviceAccountCredential);
    }
}