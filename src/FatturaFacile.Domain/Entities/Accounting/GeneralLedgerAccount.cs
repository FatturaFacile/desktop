using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Accounting;

public sealed class GeneralLedgerAccount : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }

    /// <summary>
    /// Swiss KMU: &quot;1020&quot;, IT: &quot;01.01.001&quot;
    /// </summary>
    public string AccountCode { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public AccountCategory Category { get; private set; }
    public Jurisdiction Jurisdiction { get; private set; }
    public bool IsActive { get; private set; } = true;

    private GeneralLedgerAccount()
    {
    }

    public static GeneralLedgerAccount Create(
        Guid tenantId,
        string code,
        string name,
        AccountCategory category,
        Jurisdiction jurisdiction)
    {
        return new GeneralLedgerAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AccountCode = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Category = category,
            Jurisdiction = jurisdiction,
            IsActive = true,
        };
    }
}