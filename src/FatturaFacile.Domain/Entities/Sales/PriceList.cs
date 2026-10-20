using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Sales;

public sealed class PriceList : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; private set; } = string.Empty;
    public Currency Currency { get; private set; }
    public bool IsDefault { get; private set; }

    private PriceList()
    {
    }

    public static PriceList Create(Guid tenantId, string name, Currency currency, bool isDefault = false)
    {
        return new PriceList
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Currency = currency,
            IsDefault = isDefault,
        };
    }
}