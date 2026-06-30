namespace Packlead.Application.Dispatchers.DTOs;
public sealed class CreateDispatcherRequest
{
    public string FirebaseUid { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Vehicle { get; init; } = string.Empty;
    public string LicensePlate { get; init; } = string.Empty;
}