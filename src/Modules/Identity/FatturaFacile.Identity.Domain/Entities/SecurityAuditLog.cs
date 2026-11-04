using FatturaFacile.Domain.Shared.Abstractions;
using FatturaFacile.Domain.Shared.Primitives;

namespace FatturaFacile.Identity.Domain.Entities;

/// <summary>
/// Security audit log entry capturing authentication events in compliance with OWASP and Swiss nLPD guidelines.
/// </summary>
public sealed class SecurityAuditLog : Entity<Guid>, IMayHaveTenant
{
    public Guid? TenantId { get; private set; }
    public Guid? UserId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTimeOffset TimestampUtc { get; private set; } = DateTimeOffset.UtcNow;

    private SecurityAuditLog() { }

    public static SecurityAuditLog Create(
        string eventType,
        Guid? tenantId = null,
        Guid? userId = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new SecurityAuditLog
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            TenantId = tenantId,
            UserId = userId,
            IpAddress = AnonymizeIp(ipAddress),
            UserAgent = userAgent != null && userAgent.Length > 255 ? userAgent[..255] : userAgent,
            TimestampUtc = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Truncates/masks the IP address to protect personal data under Swiss nLPD.
    /// </summary>
    private static string? AnonymizeIp(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip)) return null;
        var parts = ip.Split('.');
        if (parts.Length == 4)
        {
            // Mask IPv4 last octet (e.g. 192.168.1.0)
            return $"{parts[0]}.{parts[1]}.{parts[2]}.0";
        }
        return ip;
    }
}
