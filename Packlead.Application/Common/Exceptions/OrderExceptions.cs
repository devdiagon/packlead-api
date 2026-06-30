using Packlead.Application.Common.Exceptions;

public class OrderNotFoundException : AppException
{
    public override int StatusCode => 404;
    public override string ErrorCode => "NotFound";

    public OrderNotFoundException(Guid id)
        : base($"Order with id '{id}' was not found.") { }
}

public class InvalidStateTransitionException : AppException
{
    public override int StatusCode => 400;
    public override string ErrorCode => "InvalidStateTransition";

    public InvalidStateTransitionException(string message) : base(message) { }
}

public class DispatcherNotAvailableException : AppException
{
    public override int StatusCode => 409;
    public override string ErrorCode => "DispatcherNotAvailable";

    public DispatcherNotAvailableException(string message) : base(message) { }
}