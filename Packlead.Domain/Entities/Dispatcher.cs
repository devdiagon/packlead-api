using Packlead.Domain.Enums;

namespace Packlead.Domain.Entities;

public class Dispatcher
{
    public Guid Id { get; private set; }
    public string FirebaseUid { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Vehicle { get; private set; } = string.Empty;
    public string LicensePlate { get; private set; } = string.Empty;
    public DispatcherState State { get; private set; }

    private Dispatcher() { }

    public Dispatcher(string firebaseUid, string name, string email, string vehicle, string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(firebaseUid))
            throw new ArgumentException("FirebaseUid is required.", nameof(firebaseUid));

        Id = Guid.NewGuid();
        FirebaseUid = firebaseUid;
        Name = name;
        Email = email;
        Vehicle = vehicle;
        LicensePlate = licensePlate;
        State = DispatcherState.Available;
    }

    public void SetState(DispatcherState newState) => State = newState;

    public void UpdateDetails(string name, string email, string vehicle, string licensePlate)
    {
        Name = name;
        Email = email;
        Vehicle = vehicle;
        LicensePlate = licensePlate;
    }
}