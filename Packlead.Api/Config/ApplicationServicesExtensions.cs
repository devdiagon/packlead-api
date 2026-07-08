using Microsoft.EntityFrameworkCore;
using Packlead.Application.Common.Interfaces;
using Packlead.Application.Dispatchers.Commands;
using Packlead.Application.Dispatchers.Queries;
using Packlead.Application.Orders.Commands;
using Packlead.Application.Orders.Queries;
using Packlead.Infrastructure.Persistence;
using Packlead.Infrastructure.Repositories;

namespace Packlead.Api.Config;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Database context
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IDispatcherRepository, DispatcherRepository>();

        // Commands
        services.AddScoped<CreateOrderCommand>();
        services.AddScoped<UpdateOrderCommand>();
        services.AddScoped<DeleteOrderCommand>();
        services.AddScoped<CreateDispatcherCommand>();
        services.AddScoped<UpdateDispatcherCommand>();
        services.AddScoped<DeleteDispatcherCommand>();

        // Queries
        services.AddScoped<GetAllOrdersQuery>();
        services.AddScoped<GetOrderByIdQuery>();
        services.AddScoped<GetAllDispatchersQuery>();
        services.AddScoped<GetDispatcherByIdQuery>();

        return services;
    }
}