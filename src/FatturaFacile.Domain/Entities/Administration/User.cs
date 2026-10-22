// <copyright file="User.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Administration;

/// <summary>
/// Represents an authenticated operator or employee user account within a tenant organization, controlling access roles, credentials, and audit tracking.
/// </summary>
public sealed class User : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class for entity framework or serialization.
    /// </summary>
    private User()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant organization to which this user belongs.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique normalized login username.
    /// </summary>
    public string Username { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the primary electronic mail address used for communication and notifications.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the full legal or display name of the user.
    /// </summary>
    public string FullName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the cryptographic hash of the user's password.
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the cryptographic salt used in conjunction with the password hash algorithm.
    /// </summary>
    public string PasswordSalt { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the authorization security role assigning platform privileges (e.g., Administrator, Manager, Operator).
    /// </summary>
    public UserRole Role { get; private set; } = UserRole.Admin;

    /// <summary>
    /// Gets a value indicating whether this user account is active and allowed to authenticate.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Gets the UTC timestamp when the user last authenticated successfully, or <see langword="null"/> if the user has never logged in.
    /// </summary>
    public DateTime? LastLoginUtc { get; private set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the user account was provisioned.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identity of the user or administrative process that created this account.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the user details or credentials were last modified.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identity of the user or process that last modified this account.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates and initializes a new <see cref="User"/> account aggregate.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the tenant organization owning this user account.</param>
    /// <param name="username">The normalized account username.</param>
    /// <param name="email">The primary email address.</param>
    /// <param name="fullName">The full display name of the operator.</param>
    /// <param name="passwordHash">The computed cryptographic password hash.</param>
    /// <param name="salt">The cryptographic salt string.</param>
    /// <param name="role">The assigned role and authorization level.</param>
    /// <returns>A configured and active <see cref="User"/> entity.</returns>
    public static User Create(
        Guid     tenantId,
        string   username,
        string   email,
        string   fullName,
        string   passwordHash,
        string   salt,
        UserRole role)
    {
        return new User
               {
                   Id           = Guid.NewGuid(),
                   TenantId     = tenantId,
                   Username     = username?.Trim().ToLowerInvariant() ?? string.Empty,
                   Email        = email?.Trim().ToLowerInvariant()    ?? string.Empty,
                   FullName     = fullName?.Trim()                    ?? string.Empty,
                   PasswordHash = passwordHash,
                   PasswordSalt = salt,
                   Role         = role,
                   IsActive     = true,
                   CreatedAtUtc = DateTime.UtcNow,
               };
    }

    /// <summary>
    /// Records the occurrence of a successful login authentication event with current UTC time.
    /// </summary>
    public void RecordLogin() => LastLoginUtc = DateTime.UtcNow;

    /// <summary>
    /// Updates the cryptographic password hash and salt credentials, updating the modification timestamp.
    /// </summary>
    /// <param name="newHash">The new hashed password string.</param>
    /// <param name="newSalt">The new salt string used for password hashing.</param>
    public void ChangePassword(
        string newHash,
        string newSalt)
    {
        PasswordHash = newHash;
        PasswordSalt = newSalt;
        UpdatedAtUtc = DateTime.UtcNow;
    }

}
