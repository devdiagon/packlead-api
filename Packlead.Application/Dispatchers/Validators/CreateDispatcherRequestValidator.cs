using FluentValidation;
using Packlead.Application.Dispatchers.DTOs;

namespace Packlead.Application.Dispatchers.Validators;

public class CreateDispatcherRequestValidator : AbstractValidator<CreateDispatcherRequest>
{
    public CreateDispatcherRequestValidator()
    {
        RuleFor(x => x.FirebaseUid)
            .NotEmpty()
            .When(x => x.FirebaseUid is not null)
            .WithMessage("Si se envía FirebaseUid, no puede estar vacío.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Vehicle).NotEmpty();
        RuleFor(x => x.LicensePlate).NotEmpty().MaximumLength(15);
    }
}