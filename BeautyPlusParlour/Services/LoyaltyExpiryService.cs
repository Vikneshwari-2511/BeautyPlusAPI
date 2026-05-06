using BeautyPlusParlour.Interfaces;

namespace BeautyPlusParlour.Services;

public sealed class LoyaltyExpiryService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LoyaltyExpiryService> _logger;

    // Runs daily at 2 AM
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    public LoyaltyExpiryService(
        IServiceScopeFactory scopeFactory,
        ILogger<LoyaltyExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Loyalty expiry service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunAsync(stoppingToken);
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var loyaltyService = scope.ServiceProvider
                .GetRequiredService<ILoyaltyService>();

            await loyaltyService.ExpireOldPointsAsync(ct);

            _logger.LogInformation(
                "Loyalty expiry job completed at {Time}", DateTime.UtcNow);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Loyalty expiry job failed.");
        }
    }
}