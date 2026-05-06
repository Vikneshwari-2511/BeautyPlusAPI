using System.Security.Cryptography;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Helpers;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeautyPlusParlour.Services;

public sealed class OtpService : IOtpService
{
    private readonly AppDbContext _db;

    public OtpService(AppDbContext db) => _db = db;

    public async Task<string> GenerateAsync(
        Guid userId, string purpose, CancellationToken ct = default)
    {
        // Invalidate all previous unused OTPs for this purpose
        var existing = await _db.OtpVerifications
            .Where(o => o.UserId == userId
                     && o.Purpose == purpose
                     && !o.IsUsed)
            .ToListAsync(ct);

        foreach (var old in existing)
            old.MarkUsed();

        var otp = RandomNumberGenerator.GetInt32(100_000, 999_999).ToString();
        var otpHash = HashHelper.ToSha256Base64(otp);

        _db.OtpVerifications.Add(
            OtpVerification.Create(userId, otpHash, purpose));

        await _db.SaveChangesAsync(ct);
        return otp;
    }

    public async Task<bool> VerifyAsync(
        Guid userId, string otp,
        string purpose, CancellationToken ct = default)
    {
        var otpHash = HashHelper.ToSha256Base64(otp);

        var record = await _db.OtpVerifications
            .FirstOrDefaultAsync(
                o => o.UserId == userId
                  && o.OtpHash == otpHash
                  && o.Purpose == purpose
                  && !o.IsUsed, ct);

        if (record is null || record.IsExpired())
            return false;

        record.MarkUsed();
        await _db.SaveChangesAsync(ct);
        return true;
    }
}