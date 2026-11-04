using FatturaFacile.Domain.Shared.Primitives;

namespace FatturaFacile.Identity.Domain.Aggregates;

/// <summary>
/// Represents a company or organisation account acting as an isolated tenant.
/// </summary>
public sealed class Tenant : AggregateRoot<Guid>, IAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private Tenant() { }

    public static Tenant Create(string name, string slug, string? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Tenant name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("Tenant slug cannot be empty.", nameof(slug));

        return new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
