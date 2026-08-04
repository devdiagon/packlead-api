namespace Packlead.Application.Common.Exceptions;

public sealed class FirebaseUserCreationException : AppException
{
    public override int StatusCode => 502;
    public override string ErrorCode => "FirebaseUserCreationFailed";

    public FirebaseUserCreationException(string email, Exception inner)
        : base($"No se pudo crear el usuario de Firebase para '{email}'.", inner) { }
}