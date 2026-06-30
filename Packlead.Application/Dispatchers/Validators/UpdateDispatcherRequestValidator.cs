using FluentValidation;
using Packlead.Application.Dispatchers.DTOs;

namespace Packlead.Application.Dispatchers.Validators;

public class UpdateDispatcherRequestValidator : AbstractValidator<UpdateDispatcherRequest>
{
    public UpdateDispatcherRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Vehicle).NotEmpty();
        RuleFor(x => x.LicensePlate).NotEmpty().MaximumLength(15);
        RuleFor(x => x.State)
            .Must(s => s.Equals("available", StringComparison.OrdinalIgnoreCase)
                    || s.Equals("inactive", StringComparison.OrdinalIgnoreCase))
            .WithMessage("State must be 'available' or 'inactive'.");
    }
}