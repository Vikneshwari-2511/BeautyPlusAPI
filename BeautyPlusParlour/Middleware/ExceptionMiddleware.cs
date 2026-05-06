using System.Net;
using System.Text.Json;
using BeautyPlusParlour.Exceptions;
using BeautyPlusParlour.Models.DTOs.Common;

namespace BeautyPlusParlour.Middleware;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
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
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        _logger.LogError(ex,
            "Unhandled exception on {Method} {Path}: {Message}",
            context.Request.Method,
            context.Request.Path,
            ex.Message);

        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = ex switch
        {
            ValidationException ve =>
                (HttpStatusCode.UnprocessableEntity, ve.Message, ve.Errors),

            UnauthorizedException ue =>
                (HttpStatusCode.Unauthorized, ue.Message, (IEnumerable<string>?)null),

            NotFoundException nfe =>
                (HttpStatusCode.NotFound, nfe.Message, (IEnumerable<string>?)null),

            AppException ae =>
                ((HttpStatusCode)ae.StatusCode, ae.Message, (IEnumerable<string>?)null),

            _ =>
                (HttpStatusCode.InternalServerError,
                 "An unexpected error occurred. Please try again later.",
                 (IEnumerable<string>?)null)
        };

        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.Fail(message, errors);

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
    }
}