namespace Packlead.Application.Dispatchers.DTOs;

public record class CreateDispatcherResponse(
    Guid Id,
    string Name,
    string Email,
    string Vehicle,
    string LicensePlate,
    string State,
    string? PasswordResetLink
)
{
    public static CreateDispatcherResponse FromDispatcherResponse(DispatcherResponse response, string? passwordResetLink) =>
        new(response.Id, response.Name, response.Email, response.Vehicle, response.LicensePlate, response.State, passwordResetLink);
}