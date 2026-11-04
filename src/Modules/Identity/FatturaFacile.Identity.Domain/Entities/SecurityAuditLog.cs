// <copyright file="SecurityAuditLog.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Abstractions;
using FatturaFacile.Domain.Shared.Primitives;

namespace FatturaFacile.Identity.Domain.Entities;

/// <summary>
/// Security audit log entry capturing authentication events
/// in compliance with OWASP and Swiss nLPD guidelines.
/// </summary>
public sealed class SecurityAuditLog : Entity<Guid>, IMayHaveTenant
{
    private const int MaxUserAgentLength = 255;
    private const int MaxMaskedIpLength = 16;
    private const int IPv4PartsCount = 4;

    private SecurityAuditLog()
    {
    }

    /// <inheritdoc />
    public Guid? TenantId { get; private set; }

    /// <summary>
    /// Gets the optional user identifier associated with the audit event.
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// Gets the event classification name
    /// (e.g. LoginSuccess, LoginFailed_InvalidPassword, TokenRefreshed).
    /// </summary>
    public string EventType { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the anonymized or masked IP address of the client request.
    /// </summary>
    public string? IpAddress { get; private set; }

    /// <summary>
    /// Gets the truncated User-Agent header string of the client.
    /// </summary>
    public string? UserAgent { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the security event occurred.
    /// </summary>
    public DateTimeOffset TimestampUtc { get; private set; }

    /// <summary>
    /// Creates a new security audit log entry.
    /// </summary>
    /// <param name="eventType">Security event classification.</param>
    /// <param name="tenantId">Optional tenant identifier.</param>
    /// <param name="userId">Optional user identifier.</param>
    /// <param name="ipAddress">
    /// Raw client IP address that will be automatically anonymized.
    /// </param>
    /// <param name="userAgent">Client User-Agent string.</param>
    /// <returns>A new <see cref="SecurityAuditLog"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="eventType"/> is empty.
    /// </exception>
    public static SecurityAuditLog Create(
        string eventType,
        Guid? tenantId = null,
        Guid? userId = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);

        return new SecurityAuditLog
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            TenantId = tenantId,
            UserId = userId,
            IpAddress = AnonymizeIp(ipAddress),
            UserAgent = TruncateUserAgent(userAgent),
            TimestampUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Truncates or masks the IP address to protect personal data
    /// under Swiss nLPD regulations.
    /// </summary>
    /// <param name="ip">Client IP address.</param>
    /// <returns>An anonymized IP address.</returns>
    private static string? AnonymizeIp(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
        {
            return null;
        }

        var parts = ip.Split('.');

        if (parts.Length == IPv4PartsCount)
        {
            return $"{parts[0]}.{parts[1]}.{parts[2]}.0";
        }

        return ip.Length > MaxMaskedIpLength
            ? ip[..MaxMaskedIpLength]
            : ip;
    }

    /// <summary>
    /// Truncates the User-Agent string to a safe persistence length.
    /// </summary>
    /// <param name="userAgent">User-Agent header value.</param>
    /// <returns>Truncated User-Agent value.</returns>
    private static string? TruncateUserAgent(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return userAgent;
        }

        return userAgent.Length > MaxUserAgentLength
            ? userAgent[..MaxUserAgentLength]
            : userAgent;
    }
}
