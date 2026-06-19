using BeautyPlusParlour.Middleware;

namespace BeautyPlusParlour.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder
        UseGlobalExceptionHandler(
            this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}