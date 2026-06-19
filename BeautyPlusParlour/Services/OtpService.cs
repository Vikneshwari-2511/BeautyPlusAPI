using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace BeautyPlusParlour.Services;

public sealed class OtpService : IOtpService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<OtpService> _logger;

    // OTP expires in 10 minutes
    private readonly TimeSpan _expiry = TimeSpan.FromMinutes(10);

    public OtpService(
        IDistributedCache cache,
        ILogger<OtpService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<string> GenerateAndStoreAsync(
        string email, string purpose,
        CancellationToken ct = default)
    {
        // Generate 6-digit OTP
        var otp = GenerateOtp();

        // Hash before storing
        var hashedOtp = HashOtp(otp);

        // Build cache key
        var key = BuildKey(email, purpose);

        // Store in Redis
        var payload = JsonSerializer.Serialize(new OtpPayload
        {
            HashedOtp = hashedOtp,
            Email = email,
            Purpose = purpose,
            CreatedAt = DateTime.UtcNow,
            IsUsed = false
        });

        await _cache.SetStringAsync(key, payload,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _expiry
            }, ct);

        _logger.LogInformation(
            "OTP generated for {Email} — Purpose: {Purpose}",
            email, purpose);

        return otp; // Return plain OTP to send via email
    }

    public async Task<bool> ValidateAsync(
        string email, string purpose,
        string otp, CancellationToken ct = default)
    {
        var key = BuildKey(email, purpose);
        var payload = await _cache.GetStringAsync(key, ct);

        if (payload is null)
        {
            _logger.LogWarning(
                "OTP not found for {Email} — expired or never created",
                email);
            return false;
        }

        var data = JsonSerializer.Deserialize<OtpPayload>(payload);

        if (data is null || data.IsUsed)
        {
            _logger.LogWarning("OTP already used for {Email}", email);
            return false;
        }

        var hashedInput = HashOtp(otp);
        if (data.HashedOtp != hashedInput)
        {
            _logger.LogWarning("OTP mismatch for {Email}", email);
            return false;
        }

        // Mark as used
        data.IsUsed = true;
        await _cache.SetStringAsync(key,
            JsonSerializer.Serialize(data),
            new DistributedCacheEntryOptions
            {
                // Keep for 2 min after use (prevent replay)
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            }, ct);

        return true;
    }

    public async Task InvalidateAsync(
        string email, string purpose,
        CancellationToken ct = default)
    {
        var key = BuildKey(email, purpose);
        await _cache.RemoveAsync(key, ct);
    }

    // ── private ───────────────────────────────────────────
    private static string GenerateOtp()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var num = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1000000;
        return num.ToString("D6");
    }

    private static string HashOtp(string otp)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(otp));
        return Convert.ToHexString(bytes);
    }

    private static string BuildKey(string email, string purpose) =>
        $"otp:{purpose.ToLower()}:{email.ToLower()}";

    private sealed class OtpPayload
    {
        public string HashedOtp { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsUsed { get; set; }
    }
}