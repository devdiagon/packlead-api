namespace Packlead.Application.Common.Exceptions;

public sealed class FirebaseUserDeletionException : AppException
{
    public override int StatusCode => 502;
    public override string ErrorCode => "FirebaseUserDeletionFailed";

    public FirebaseUserDeletionException(string firebaseUid, Exception inner)
        : base($"No se pudo eliminar el usuario de Firebase '{firebaseUid}'.", inner) { }
}
