namespace FatturaFacile.Domain.Shared.Abstractions;

/// <summary>
/// Defines a contract for entities that strictly belong to a specific tenant.
/// Access and mutations are automatically constrained to this tenant identifier.
/// </summary>
public interface IMustHaveTenant
{
    /// <summary>
    /// Gets the unique identifier of the tenant owning this entity.
    /// </summary>
    Guid TenantId { get; }
}

/// <summary>
/// Defines a contract for optional or host-level entities that may or may not be tied to a tenant.
/// </summary>
public interface IMayHaveTenant
{
    /// <summary>
    /// Gets the optional tenant identifier, or null if the entity belongs to system/host scope.
    /// </summary>
    Guid? TenantId { get; }
}
