namespace Atlas.Api.Middleware;

using Atlas.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

/// <summary>
/// Global exception handling middleware for consistent error responses.
/// Implements requirements 11.8, 13.5, 20.5-20.6.
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        var traceId = context.TraceIdentifier;

        _logger.LogError(
            exception,
            "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}",
            traceId,
            context.Request.Path);

        var problemDetails = exception switch
        {
            ValidationException validationEx => CreateValidationProblemDetails(context, validationEx, traceId),
            DbUpdateException dbEx => CreateDatabaseProblemDetails(context, dbEx, traceId),
            _ => CreateGenericProblemDetails(context, exception, traceId)
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(json);
    }

    /// <summary>
    /// Creates RFC 9110 Problem Details for validation errors.
    /// Returns 400 Bad Request with field-specific errors.
    /// </summary>
    private ProblemDetails CreateValidationProblemDetails(
        HttpContext context,
        ValidationException exception,
        string traceId)
    {
        var errors = new Dictionary<string, string[]>();

        foreach (var error in exception.Errors)
        {
            if (!errors.ContainsKey(error.Field))
            {
                errors[error.Field] = new[] { error.Message };
            }
            else
            {
                var existingErrors = errors[error.Field].ToList();
                existingErrors.Add(error.Message);
                errors[error.Field] = existingErrors.ToArray();
            }
        }

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Failed",
            Detail = "One or more validation errors occurred. See 'errors' for details.",
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"] = traceId
            }
        };
    }

    /// <summary>
    /// Creates Problem Details for database errors.
    /// Returns 500 Internal Server Error with trace ID (no internal details exposed).
    /// </summary>
    private ProblemDetails CreateDatabaseProblemDetails(
        HttpContext context,
        DbUpdateException exception,
        string traceId)
    {
        _logger.LogError(
            exception,
            "Database error occurred. TraceId: {TraceId}",
            traceId);

        return new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Database Error",
            Detail = "An error occurred while processing your request. Please try again later.",
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"] = traceId
            }
        };
    }

    /// <summary>
    /// Creates Problem Details for generic unhandled exceptions.
    /// Returns 500 Internal Server Error with trace ID (no internal details exposed).
    /// </summary>
    private ProblemDetails CreateGenericProblemDetails(
        HttpContext context,
        Exception exception,
        string traceId)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred while processing your request. Please try again later.",
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"] = traceId
            }
        };
    }
}

/// <summary>
/// Extension method to register the global exception handling middleware.
/// </summary>
public static class GlobalExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}
