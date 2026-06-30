namespace Packlead.Application.Common.Exceptions;

public class DispatcherNotFoundException : Exception
{
    public DispatcherNotFoundException(Guid id)
        : base($"Dispatcher with id '{id}' was not found.") { }
}

public class DuplicateFirebaseUidException : Exception
{
    public DuplicateFirebaseUidException(string firebaseUid)
        : base($"A dispatcher with FirebaseUid '{firebaseUid}' already exists.") { }
}