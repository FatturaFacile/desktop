// <copyright file="Tenant.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Primitives;

namespace FatturaFacile.Identity.Domain.Aggregates;

/// <summary>
/// Represents a company or organisation account acting as an isolated tenant.
/// </summary>
public sealed class Tenant : AggregateRoot<Guid>, IAuditableEntity
{
    private Tenant() { }

    /// <summary>
    /// Gets the display name of the tenant enterprise or organisation.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the unique URL slug and workspace sub-domain identifying this tenant.
    /// </summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this tenant account is currently active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; private set; }

    /// <inheritdoc />
    public string? CreatedBy { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAt { get; private set; }

    /// <inheritdoc />
    public string? UpdatedBy { get; }

    /// <summary>
    /// Factory method to create a new <see cref="Tenant"/> aggregate root.
    /// </summary>
    /// <param name="name">The display name of the company or organisation.</param>
    /// <param name="slug">The unique URL slug identifier.</param>
    /// <param name="createdBy">The optional user or process creating the tenant.</param>
    /// <returns>A new <see cref="Tenant"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when name or slug is empty or whitespace.</exception>
    public static Tenant Create(
        string name,
        string slug,
        string? createdBy = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        return new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy,
        };
    }

    /// <summary>
    /// Deactivates the tenant, preventing user authentication and mutations.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Re-activates the tenant account.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
