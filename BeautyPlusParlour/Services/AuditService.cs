using System.Security.Claims;
using BeautyPlusParlour.Data;
using BeautyPlusParlour.Interfaces;
using BeautyPlusParlour.Models.Entities;

namespace BeautyPlusParlour.Services;

public sealed class AuditService : IAuditService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public AuditService(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public async Task LogAsync(
        Guid? userId, string action,
        string entityName, string? entityId = null,
        string? oldValues = null, string? newValues = null,
        CancellationToken ct = default)
    {
        var ip = _http.HttpContext?
            .Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        var log = AuditLog.Create(
            userId, action, entityName,
            entityId, oldValues, newValues, ip);

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync(ct);
    }
}