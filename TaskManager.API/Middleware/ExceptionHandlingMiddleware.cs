namespace TaskManager.API.Middleware;

using System.Net;
using System.Text.Json;
using FluentValidation;

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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => HandleValidationException(validationEx),
            UnauthorizedAccessException unauthorizedEx => HandleUnauthorizedException(unauthorizedEx, context),
            InvalidOperationException invalidOpEx => HandleInvalidOperationException(invalidOpEx, context),
            KeyNotFoundException notFoundEx => HandleNotFoundException(notFoundEx, context),
            FileNotFoundException fileNotFoundEx => HandleFileNotFoundException(fileNotFoundEx, context),
            _ => HandleGenericException(exception, context)
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var jsonResponse = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(jsonResponse);
    }

    private (HttpStatusCode, object) HandleValidationException(ValidationException validationException)
    {
        _logger.LogWarning("Validation failed: {Errors}",
            string.Join(", ", validationException.Errors.Select(e => e.ErrorMessage)));

        var errors = validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return (HttpStatusCode.BadRequest, new { detail = "Validation failed", errors });
    }

    private (HttpStatusCode, object) HandleUnauthorizedException(UnauthorizedAccessException exception, HttpContext context)
    {
        _logger.LogWarning("Unauthorized access: {Message} | Path: {Path} | IP: {IP}",
            exception.Message,
            context.Request.Path,
            context.Connection.RemoteIpAddress);

        return (HttpStatusCode.Unauthorized, new { detail = exception.Message });
    }

    private (HttpStatusCode, object) HandleInvalidOperationException(InvalidOperationException exception, HttpContext context)
    {
        _logger.LogWarning("Invalid operation: {Message} | Path: {Path}",
            exception.Message,
            context.Request.Path);

        return (HttpStatusCode.BadRequest, new { detail = exception.Message });
    }

    private (HttpStatusCode, object) HandleNotFoundException(KeyNotFoundException exception, HttpContext context)
    {
        _logger.LogWarning("Resource not found: {Message} | Path: {Path}",
            exception.Message,
            context.Request.Path);

        return (HttpStatusCode.NotFound, new { detail = exception.Message });
    }

    private (HttpStatusCode, object) HandleFileNotFoundException(FileNotFoundException exception, HttpContext context)
    {
        _logger.LogWarning("File not found: {Message} | Path: {Path}",
            exception.Message,
            context.Request.Path);

        return (HttpStatusCode.NotFound, new { detail = exception.Message });
    }

    private (HttpStatusCode, object) HandleGenericException(Exception exception, HttpContext context)
    {
        _logger.LogError(exception,
            "Unhandled exception | Path: {Path} | Method: {Method} | IP: {IP}",
            context.Request.Path,
            context.Request.Method,
            context.Connection.RemoteIpAddress);

        return (HttpStatusCode.InternalServerError, new { detail = "An internal server error occurred" });
    }
}