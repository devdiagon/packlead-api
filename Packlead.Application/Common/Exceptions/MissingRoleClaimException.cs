namespace Packlead.Application.Common.Exceptions;

public class MissingRoleClaimException : AppException
{
    public override int StatusCode => 400;
    public override string ErrorCode => "MissingRoleClaim";

    public MissingRoleClaimException()
        : base("No se ha podido identificar este usuario.") { }
}