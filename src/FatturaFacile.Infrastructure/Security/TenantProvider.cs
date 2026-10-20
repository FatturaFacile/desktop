using FatturaFacile.Application.Common.Interfaces;

namespace FatturaFacile.Infrastructure.Security;

public sealed class TenantProvider : ITenantProvider
{
    public Guid CurrentTenantId { get; private set; } = Guid.Empty;
    public string CurrentTenantCode { get; private set; } = string.Empty;
    public bool HasTenant => CurrentTenantId != Guid.Empty;

    public void SetTenant(Guid tenantId, string tenantCode)
    {
        CurrentTenantId = tenantId;
        CurrentTenantCode = tenantCode;
    }
}