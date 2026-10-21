// <copyright file="User.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Administration;

/// <summary>
/// Represents user.
/// </summary>
public sealed class User : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private User()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the username.
    /// </summary>
    public string Username { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the email.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the full name.
    /// </summary>
    public string FullName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the password hash.
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the password salt.
    /// </summary>
    public string PasswordSalt { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the role.
    /// </summary>
    public UserRole Role { get; private set; } = UserRole.Admin;

    /// <summary>
    /// Gets a value indicating whether active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Gets the last login utc.
    /// </summary>
    public DateTime? LastLoginUtc { get; private set; }

    /// <summary>
    /// Gets or sets the created at utc.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the created by.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the updated at utc.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the updated by.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="username">The username.</param>
    /// <param name="email">The email.</param>
    /// <param name="fullName">The full name.</param>
    /// <param name="passwordHash">The password hash.</param>
    /// <param name="salt">The salt.</param>
    /// <param name="role">The role.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static User Create(
        Guid tenantId,
        string username,
        string email,
        string fullName,
        string passwordHash,
        string salt,
        UserRole role)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Username = username?.Trim().ToLowerInvariant() ?? string.Empty,
            Email = email?.Trim().ToLowerInvariant() ?? string.Empty,
            FullName = fullName?.Trim() ?? string.Empty,
            PasswordHash = passwordHash,
            PasswordSalt = salt,
            Role = role,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Record Login.
    /// </summary>
    public void RecordLogin() => LastLoginUtc = DateTime.UtcNow;

    /// <summary>
    /// Change Password.
    /// </summary>
    /// <param name="newHash">The new hash.</param>
    /// <param name="newSalt">The new salt.</param>
    public void ChangePassword(
        string newHash,
        string newSalt)
    {
        PasswordHash = newHash;
        PasswordSalt = newSalt;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
