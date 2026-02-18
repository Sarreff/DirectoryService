using DirectoryService.Domain.Exceptions;
using DirectoryService.Presentation.Envelopes;
using DirectoryService.Shared;

namespace DirectoryService.Web.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        (int code, Error error) = exception switch
        {
            ValidationException ex => (StatusCodes.Status400BadRequest, ex.Error),

            NotFoundException ex => (StatusCodes.Status404NotFound, ex.Error),

            ConflictException ex => (StatusCodes.Status409Conflict, ex.Error),

            FailureException ex => (StatusCodes.Status500InternalServerError, ex.Error),

            _ => (StatusCodes.Status500InternalServerError, Error.Failure("server.internal", exception.Message)),
        };

        var envelope = Envelope.Error(error.ToErrors());
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = code;

        await httpContext.Response.WriteAsJsonAsync(envelope);
    }
}

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionMiddleware>();
    }
}