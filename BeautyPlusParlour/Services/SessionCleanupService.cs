using BeautyPlusParlour.Data;
using Microsoft.EntityFrameworkCore;

namespace BeautyPlusParlour.Services;

public sealed class SessionCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SessionCleanupService> _logger;

    // Runs once every 24 hours
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    public SessionCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<SessionCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Session cleanup service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupAsync(stoppingToken);
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var cutoff = DateTime.UtcNow;

            // Delete sessions that are expired OR revoked older than 30 days
            var deleted = await db.UserSessions
                .Where(s => s.ExpiresAt < cutoff
                         || (s.IsRevoked && s.RevokedAt < DateTime.UtcNow.AddDays(-30)))
                .ExecuteDeleteAsync(ct);

            // Also clean up used/expired OTPs and tokens
            await db.OtpVerifications
                .Where(o => o.ExpiresAt < cutoff || o.IsUsed)
                .ExecuteDeleteAsync(ct);

            await db.EmailVerificationTokens
                .Where(e => e.ExpiresAt < cutoff || e.IsUsed)
                .ExecuteDeleteAsync(ct);

            await db.PasswordResetTokens
                .Where(p => p.ExpiresAt < cutoff || p.IsUsed)
                .ExecuteDeleteAsync(ct);

            _logger.LogInformation(
                "Session cleanup complete. Removed {Count} expired sessions.",
                deleted);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Session cleanup job failed.");
        }
    }
}