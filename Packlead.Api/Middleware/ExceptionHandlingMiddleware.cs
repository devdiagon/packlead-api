using System.Text.Json;
using Packlead.Application.Common.Exceptions;
using Packlead.Domain.Exceptions;

namespace Packlead.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            await WriteErrorResponse(context, ex.StatusCode, ex.ErrorCode, ex.Message);
        }
        catch (DomainExceptions ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                status = 400,
                error = ex.GetType().Name.Replace("Exception", string.Empty),
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteErrorResponse(context, 500, "InternalServerError", "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, int statusCode, string error, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var payload = JsonSerializer.Serialize(new
        {
            status = statusCode,
            error,
            message
        });

        await context.Response.WriteAsync(payload);
    }
}