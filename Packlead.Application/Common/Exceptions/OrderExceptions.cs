using Packlead.Application.Common.Exceptions;

public class OrderNotFoundException : AppException
{
    public override int StatusCode => 404;
    public override string ErrorCode => "NotFound";

    public OrderNotFoundException()
        : base($"Order was not found.") { }
}

public class DispatcherNotAvailableException : AppException
{
    public override int StatusCode => 409;
    public override string ErrorCode => "DispatcherNotAvailable";

    public DispatcherNotAvailableException(string message) : base(message) { }
}