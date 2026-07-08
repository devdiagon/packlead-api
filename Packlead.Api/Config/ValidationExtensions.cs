using FluentValidation;
using Packlead.Api.Filters;
using Packlead.Application.Orders.Validators;

namespace Packlead.Api.Config;

public static class ValidationExtensions
{
    public static IServiceCollection AddApiValidation(this IServiceCollection services)
    {
        // Validators
        services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

        // Controllers with Validation Filter
        services.AddControllers(options =>
        {
            options.Filters.Add<ValidationFilter>();
        });

        return services;
    }
}