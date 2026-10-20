namespace FatturaFacile.Domain.Common;

/// <summary>
/// Interface implemented by any entity subject to multi-tenant isolation.
/// </summary>
public interface ITenantEntity
{
    public Guid TenantId { get; set; }
}