namespace Packlead.Application.Common.Exceptions;

public sealed class DuplicateEmailException : AppException
{
    public override int StatusCode => 409;
    public override string ErrorCode => "DuplicateEmail";

    public DuplicateEmailException(string email)
        : base($"Ya existe una cuenta de Firebase con el email '{email}'.") { }
}
