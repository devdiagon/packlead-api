namespace Packlead.Domain.Exceptions;

public abstract class DomainExceptions : Exception
{
    protected DomainExceptions(string message) : base(message) { }
}