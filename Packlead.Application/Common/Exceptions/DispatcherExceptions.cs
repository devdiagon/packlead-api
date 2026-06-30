using Packlead.Application.Common.Exceptions;

public class DispatcherNotFoundException : AppException
{
    public override int StatusCode => 404;
    public override string ErrorCode => "NotFound";

    public DispatcherNotFoundException(Guid id)
        : base($"Dispatcher with id '{id}' was not found.") { }
}

public class DuplicateFirebaseUidException : AppException
{
    public override int StatusCode => 409;
    public override string ErrorCode => "DuplicateFirebaseUid";

    public DuplicateFirebaseUidException(string firebaseUid)
        : base($"A dispatcher with FirebaseUid '{firebaseUid}' already exists.") { }
}