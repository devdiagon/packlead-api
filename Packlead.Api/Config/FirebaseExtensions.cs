using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace Packlead.Api.Config;

public static class FirebaseExtensions
{
    public static IServiceCollection AddFirebaseAdmin(
        this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Firebase configuration
        var serviceAccountPath = configuration["Firebase:ServiceAccountPath"]
            ?? throw new InvalidOperationException(
                "Falta la clave 'Firebase:ServiceAccountPath' en la configuración.");

        services.AddSingleton(_ =>
        {
            var credential = environment.IsDevelopment()
                ? LoadServiceAccountCredential(serviceAccountPath)
                : GoogleCredential.GetApplicationDefault();

            return FirebaseApp.Create(new AppOptions
            {
                Credential = credential,
                ProjectId = configuration["Firebase:ProjectId"]
            });
        });

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