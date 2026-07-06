namespace Packlead.Domain.Exceptions;

public class InvalidStateTransitionException : DomainExceptions
{
    public InvalidStateTransitionException(string message) : base(message) { }
}

public class InvalidLocationException : DomainExceptions
{
    public InvalidLocationException(string message) : base(message) { }
}

public class InvalidDispatcherIdException : DomainExceptions
{
    public InvalidDispatcherIdException(string message) : base(message) { }
}