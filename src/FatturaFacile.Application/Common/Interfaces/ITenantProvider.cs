namespace FatturaFacile.Application.Common.Interfaces;

public interface ITenantProvider
{
    public Guid CurrentTenantId { get; }
    public string CurrentTenantCode { get; }
    public void SetTenant(Guid tenantId, string tenantCode);
    public bool HasTenant { get; }
}
