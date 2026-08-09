using Packlead.Application.Common.Exceptions;

public class DispatcherNotFoundException : AppException
{
    public override int StatusCode => 404;
    public override string ErrorCode => "NotFound";

    public DispatcherNotFoundException()
        : base($"Dispatcher was not found.") { }
}

public class DuplicateFirebaseUidException : AppException
{
    public override int StatusCode => 409;
    public override string ErrorCode => "DuplicateFirebaseUid";

    public DuplicateFirebaseUidException(string firebaseUid)
        : base($"A dispatcher with FirebaseUid '{firebaseUid}' already exists.") { }
}

public class DispatcherRecordMissingException : AppException
{
    public override int StatusCode => 404;
    public override string ErrorCode => "DispatcherRecordMissing";

    public DispatcherRecordMissingException()
        : base("No dispatcher record found for this account. Contact an administrator.") { }
}

public class DispatcherInactiveException : AppException
{
    public override int StatusCode => 401;
    public override string ErrorCode => "DispatcherInactive";

    public DispatcherInactiveException()
        : base("This dispatcher account is inactive.") { }
}