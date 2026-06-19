using System.Net;
using System.Text.Json;
using BeautyPlusParlour.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BeautyPlusParlour.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ctx, ex);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext ctx, Exception ex)
    {
        var (statusCode, message, errors) =
            MapException(ex);

        // Log
        if (statusCode >= 500)
        {
            _logger.LogError(
                ex,
                "Unhandled exception: {Message} | " +
                "Path: {Path} | Method: {Method}",
                ex.Message,
                ctx.Request.Path,
                ctx.Request.Method);
        }
        else
        {
            _logger.LogWarning(
                "Handled exception [{Status}]: {Message} | " +
                "Path: {Path}",
                statusCode,
                ex.Message,
                ctx.Request.Path);
        }

        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode = statusCode;

        var response = new
        {
            success = false,
            message,
            errors,
            statusCode,
            traceId = ctx.TraceIdentifier,
            // Only include stack trace in development
            detail = _env.IsDevelopment()
                        ? ex.ToString() : null
        };

        var json = JsonSerializer.Serialize(response,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        await ctx.Response.WriteAsync(json);
    }

    private static (int statusCode, string message, string[] errors)
        MapException(Exception ex)
    {
        return ex switch
        {
            AppException app =>
                (app.StatusCode, app.Message, app.Errors is string[] arr ? arr : app.Errors.ToArray()),

            UnauthorizedAccessException =>
                (401, "Unauthorized. Please login.", Array.Empty<string>()),

            KeyNotFoundException =>
                (404, "Resource not found.", Array.Empty<string>()),

            ArgumentNullException =>
                (400, "A required value was null.", Array.Empty<string>()),

            ArgumentException =>
                (400, ex.Message, Array.Empty<string>()),

            InvalidOperationException =>
                (400, ex.Message, Array.Empty<string>()),

            OperationCanceledException =>
                (499, "Request cancelled.", Array.Empty<string>()),

            TimeoutException =>
                (408, "Request timed out. Please retry.", Array.Empty<string>()),

            _ =>
                (500, "An unexpected error occurred. " +
                      "Please try again later.", Array.Empty<string>())
        };
    }
}