namespace Packlead.Application.Common.Exceptions;

public abstract class AppException : Exception
{
    public abstract int StatusCode { get; }
    public abstract string ErrorCode { get; }

    protected AppException(string message) : base(message) { }
}