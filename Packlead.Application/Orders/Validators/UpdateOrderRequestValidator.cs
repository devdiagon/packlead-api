using FluentValidation;
using Packlead.Application.Orders.DTOs;

namespace Packlead.Application.Orders.Validators;

public class UpdateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public UpdateOrderRequestValidator()
    {
        RuleFor(x => x.ClientName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ClientPhoneNumber)
            .NotEmpty()
            .Matches(@"^\+?[0-9\-\s]{7,20}$").WithMessage("Invalid phone number format.");
        RuleFor(x => x.Location.Lat).InclusiveBetween(-90, 90);
        RuleFor(x => x.Location.Lng).InclusiveBetween(-180, 180);
        RuleFor(x => x.Zone).NotEmpty();
    }
}