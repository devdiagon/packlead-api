namespace Packlead.Domain.Enums;

// Usado solo para tipar HttpContext.User en la capa Api.
// No mapea a ninguna tabla — el rol Admin vive únicamente en Firebase
public enum UserRole
{
    None,
    Admin,
    Dispatcher
}