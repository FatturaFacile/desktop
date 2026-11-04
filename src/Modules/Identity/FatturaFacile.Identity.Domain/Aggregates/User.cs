using FatturaFacile.Domain.Shared.Abstractions;
using FatturaFacile.Domain.Shared.Primitives;

namespace FatturaFacile.Identity.Domain.Aggregates;

/// <summary>
/// Represents an authenticated user profile adhering to Swiss nLPD data protection guidelines.
/// Personal identifiers (PII) are stored encrypted or securely hashed for blind indexing.
/// </summary>
public sealed class User : AggregateRoot<Guid>, IMustHaveTenant, IAuditableEntity
{
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Gets the SHA-256 blind-index hash of the upper-cased email address for fast O(1) indexed lookup.
    /// </summary>
    public string NormalizedEmail { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the AES-256-GCM cipher text of the email for data-at-rest protection under nLPD / FADP.
    /// </summary>
    public string EncryptedEmail { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the Argon2id salted/hashed password string.
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a random security stamp updated on security-sensitive actions (password change, lockout).
    /// </summary>
    public string SecurityStamp { get; private set; } = Guid.NewGuid().ToString("N");

    public bool IsActive { get; private set; } = true;
    public int AccessFailedCount { get; private set; }
    public DateTimeOffset? LockoutEndUtc { get; private set; }

    /// <summary>
    /// Gets the user's preferred language ('en', 'it', 'de', 'fr').
    /// </summary>
    public string PreferredLanguage { get; private set; } = "en";

    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private User() { }

    public static User Create(
        Guid tenantId,
        string normalizedEmail,
        string encryptedEmail,
        string passwordHash,
        string preferredLanguage = "en",
        string? createdBy = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(normalizedEmail)) throw new ArgumentException("NormalizedEmail is required.", nameof(normalizedEmail));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("PasswordHash is required.", nameof(passwordHash));

        return new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            NormalizedEmail = normalizedEmail,
            EncryptedEmail = encryptedEmail,
            PasswordHash = passwordHash,
            SecurityStamp = Guid.NewGuid().ToString("N"),
            IsActive = true,
            AccessFailedCount = 0,
            LockoutEndUtc = null,
            PreferredLanguage = preferredLanguage,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };
    }

    public bool IsLockedOut(DateTimeOffset now)
    {
        return LockoutEndUtc.HasValue && LockoutEndUtc.Value > now;
    }

    public void RecordFailedLogin(DateTimeOffset now, int maxAttempts = 5, TimeSpan? lockoutDuration = null)
    {
        AccessFailedCount++;
        if (AccessFailedCount >= maxAttempts)
        {
            LockoutEndUtc = now.Add(lockoutDuration ?? TimeSpan.FromMinutes(15));
            SecurityStamp = Guid.NewGuid().ToString("N");
        }
        UpdatedAt = now;
    }

    public void ResetFailedLogin(DateTimeOffset now)
    {
        if (AccessFailedCount != 0 || LockoutEndUtc != null)
        {
            AccessFailedCount = 0;
            LockoutEndUtc = null;
            UpdatedAt = now;
        }
    }

    public void ChangePassword(string newPasswordHash, DateTimeOffset now)
    {
        PasswordHash = newPasswordHash;
        SecurityStamp = Guid.NewGuid().ToString("N");
        UpdatedAt = now;
    }

    public void SetPreferredLanguage(string language)
    {
        PreferredLanguage = language;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
