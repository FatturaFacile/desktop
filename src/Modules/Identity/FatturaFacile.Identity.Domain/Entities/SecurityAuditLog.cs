// <copyright file="SecurityAuditLog.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Abstractions;
using FatturaFacile.Domain.Shared.Primitives;

namespace FatturaFacile.Identity.Domain.Entities;

/// <summary>
/// Security audit log entry capturing authentication events in compliance with OWASP and Swiss nLPD guidelines.
/// </summary>
public sealed class SecurityAuditLog : Entity<Guid>, IMayHaveTenant
{
    private SecurityAuditLog() { }

    /// <inheritdoc />
    public Guid? TenantId { get; private set; }

    /// <summary>
    /// Gets the optional user identifier associated with the audit event.
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// Gets the event classification name (e.g., "LoginSuccess", "LoginFailed_InvalidPassword", "TokenRefreshed").
    /// </summary>
    public string EventType { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the anonymized / masked IP address of the client request.
    /// </summary>
    public string? IpAddress { get; private set; }

    /// <summary>
    /// Gets the truncated User-Agent header string of the client.
    /// </summary>
    public string? UserAgent { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the security event occurred.
    /// </summary>
    public DateTimeOffset TimestampUtc { get; private set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Factory method to create an anonymized <see cref="SecurityAuditLog"/> entry.
    /// </summary>
    /// <param name="eventType">The security event classification name.</param>
    /// <param name="tenantId">The optional tenant identifier.</param>
    /// <param name="userId">The optional user identifier.</param>
    /// <param name="ipAddress">The raw client IP address, which will be automatically anonymized.</param>
    /// <param name="userAgent">The client User-Agent string.</param>
    /// <returns>A new <see cref="SecurityAuditLog"/> instance.</returns>
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
            TimestampUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Truncates/masks the IP address to protect personal data under Swiss nLPD.
    /// </summary>
    /// <param name="ip"></param>
    private static string? AnonymizeIp(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
        {
            return null;
        }

        var parts = ip.Split('.');
        return parts.Length == 4 ? $"{parts[0]}.{parts[1]}.{parts[2]}.0" : ip.Length > 16 ? ip[..16] : ip;
    }
}
