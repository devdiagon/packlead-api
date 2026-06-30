namespace Packlead.Application.Dispatchers.DTOs;
public sealed class DispatcherResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Vehicle { get; init; } = string.Empty;
    public string LicensePlate { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
}