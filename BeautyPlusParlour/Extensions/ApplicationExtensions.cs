using BeautyPlusParlour.Data;
using BeautyPlusParlour.Middleware;
using Microsoft.EntityFrameworkCore;

namespace BeautyPlusParlour.Extensions;

public static class ApplicationExtensions
{
    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public static WebApplication UseGlobalExceptionHandler(this WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        return app;
    }

    // Adds production-grade HTTP security headers
    public static WebApplication UseSecureHeaders(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;

            // Prevent browsers from MIME-sniffing
            headers["X-Content-Type-Options"] = "nosniff";

            // Block clickjacking
            headers["X-Frame-Options"] = "DENY";

            // Enable XSS filter in older browsers
            headers["X-XSS-Protection"] = "1; mode=block";

            // Only send referrer on same origin
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Restrict powerful browser features
            headers["Permissions-Policy"] =
                "camera=(), microphone=(), geolocation=()";

            // HSTS — only in production; tells browser to always use HTTPS
            if (!app.Environment.IsDevelopment())
                headers["Strict-Transport-Security"] =
                    "max-age=31536000; includeSubDomains";

            await next();
        });

        return app;
    }
}