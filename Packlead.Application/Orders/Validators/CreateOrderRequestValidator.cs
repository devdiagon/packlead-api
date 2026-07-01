using FluentValidation;
using Packlead.Application.Orders.DTOs;

namespace Packlead.Application.Orders.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.ClientName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ClientPhoneNumber)
            .NotEmpty()
            .Matches(@"^\+?[0-9\-\s]{7,20}$").WithMessage("Invalid phone number format.");
        RuleFor(x => x.Location.Lat).InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.");
        RuleFor(x => x.Location.Lng).InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.");
        RuleFor(x => x.Zone).NotEmpty();
        RuleFor(x => x.DeliveryDate).GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Delivery date cannot be in the past.");
    }
}