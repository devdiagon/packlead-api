using Microsoft.EntityFrameworkCore;
using Packlead.Api.Middleware;
using Packlead.Application.Common.Interfaces;
using Packlead.Application.Dispatchers.Commands;
using Packlead.Application.Dispatchers.Queries;
using Packlead.Application.Orders.Commands;
using Packlead.Application.Orders.Queries;
using Packlead.Infrastructure.Persistence;
using Packlead.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IDispatcherRepository, DispatcherRepository>();

// Commands
builder.Services.AddScoped<CreateOrderCommand>();
builder.Services.AddScoped<UpdateOrderCommand>();
builder.Services.AddScoped<DeleteOrderCommand>();
builder.Services.AddScoped<CreateDispatcherCommand>();
builder.Services.AddScoped<UpdateDispatcherCommand>();
builder.Services.AddScoped<DeleteDispatcherCommand>();

// Queries
builder.Services.AddScoped<GetAllOrdersQuery>();
builder.Services.AddScoped<GetOrderByIdQuery>();
builder.Services.AddScoped<GetAllDispatchersQuery>();
builder.Services.AddScoped<GetDispatcherByIdQuery>();


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
