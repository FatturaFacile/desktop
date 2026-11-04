// <copyright file="User.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Abstractions;
using FatturaFacile.Domain.Shared.Primitives;

namespace FatturaFacile.Identity.Domain.Aggregates;

/// <summary>
/// Represents an authenticated user profile adhering to Swiss nLPD data protection guidelines.
/// Personal identifiers (PII) are stored encrypted or securely hashed for blind indexing.
/// </summary>
public sealed class User : AggregateRoot<Guid>, IMustHaveTenant, IAuditableEntity
{
    private User() { }

    /// <inheritdoc />
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

    /// <summary>
    /// Gets a value indicating whether the user profile is active and eligible to authenticate.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Gets the number of consecutive failed authentication attempts.
    /// </summary>
    public int AccessFailedCount { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when temporary account lockout ends, or null if not locked.
    /// </summary>
    public DateTimeOffset? LockoutEndUtc { get; private set; }

    /// <summary>
    /// Gets the user's preferred language ('en', 'it', 'de', 'fr').
    /// </summary>
    public string PreferredLanguage { get; private set; } = "en";

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; private set; }

    /// <inheritdoc />
    public string? CreatedBy { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAt { get; private set; }

    /// <inheritdoc />
    public string? UpdatedBy { get; }

    /// <summary>
    /// Factory method to create a new <see cref="User"/> aggregate root with encrypted PII and Argon2id hash.
    /// </summary>
    /// <param name="tenantId">The owning tenant identifier.</param>
    /// <param name="normalizedEmail">The blind index hash of the email address.</param>
    /// <param name="encryptedEmail">The AES-GCM encrypted email cipher text.</param>
    /// <param name="passwordHash">The Argon2id password hash.</param>
    /// <param name="preferredLanguage">The preferred UI culture code (defaults to "en").</param>
    /// <param name="createdBy">The optional user or system process creating the user.</param>
    /// <returns>A new <see cref="User"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when tenantId, normalizedEmail, or passwordHash are missing.</exception>
    public static User Create(
        Guid tenantId,
        string normalizedEmail,
        string encryptedEmail,
        string passwordHash,
        string preferredLanguage = "en",
        string? createdBy = null)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("TenantId is required.", nameof(tenantId));
        }

        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            throw new ArgumentException("NormalizedEmail is required.", nameof(normalizedEmail));
        }

        return string.IsNullOrWhiteSpace(passwordHash)
            ? throw new ArgumentException("PasswordHash is required.", nameof(passwordHash))
            : new User
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
            CreatedBy = createdBy,
        };
    }

    /// <summary>
    /// Checks whether the user is currently locked out from logging in.
    /// </summary>
    /// <param name="now">The current UTC timestamp.</param>
    /// <returns><see langword="true"/> if locked out; otherwise, <see langword="false"/>.</returns>
    public bool IsLockedOut(DateTimeOffset now) => LockoutEndUtc.HasValue && LockoutEndUtc.Value > now;

    /// <summary>
    /// Increments the failed login count and triggers temporary lockout if maximum attempts are reached.
    /// </summary>
    /// <param name="now">The current UTC timestamp.</param>
    /// <param name="maxAttempts">The maximum allowed failed attempts before lockout (default: 5).</param>
    /// <param name="lockoutDuration">The lockout duration (default: 15 minutes).</param>
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

    /// <summary>
    /// Resets the failed login counter and clears any active lockout state upon successful authentication.
    /// </summary>
    /// <param name="now">The current UTC timestamp.</param>
    public void ResetFailedLogin(DateTimeOffset now)
    {
        if (AccessFailedCount != 0 || LockoutEndUtc != null)
        {
            AccessFailedCount = 0;
            LockoutEndUtc = null;
            UpdatedAt = now;
        }
    }

    /// <summary>
    /// Updates the user's password hash and rotates the security stamp.
    /// </summary>
    /// <param name="newPasswordHash">The new Argon2id password hash.</param>
    /// <param name="now">The current UTC timestamp.</param>
    public void ChangePassword(string newPasswordHash, DateTimeOffset now)
    {
        PasswordHash = newPasswordHash;
        SecurityStamp = Guid.NewGuid().ToString("N");
        UpdatedAt = now;
    }

    /// <summary>
    /// Updates the user's preferred language code.
    /// </summary>
    /// <param name="language">The two-letter language culture code.</param>
    public void SetPreferredLanguage(string language)
    {
        PreferredLanguage = language;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
