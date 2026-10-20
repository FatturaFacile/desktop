using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Construction;

public sealed class BillOfQuantitiesItem : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid ConstructionSiteId { get; private set; }

    /// <summary>
    /// Regional tariff code (e.g. Lombardia or SIA/CRB/NPK)
    /// </summary>
    public string ItemCode { get; private set; } = string.Empty;
    public StandardType StandardType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string UnitOfMeasure { get; private set; } = "m2";
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Subtotal => decimal.Round(Quantity * UnitPrice, 2, MidpointRounding.AwayFromZero);

    private BillOfQuantitiesItem()
    {
    }

    public static BillOfQuantitiesItem Create(
        Guid tenantId,
        Guid siteId,
        string code,
        StandardType standard,
        string desc,
        string uom,
        decimal qty,
        decimal unitPrice)
    {
        return new BillOfQuantitiesItem
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ConstructionSiteId = siteId,
            ItemCode = code.Trim(),
            StandardType = standard,
            Description = desc.Trim(),
            UnitOfMeasure = uom.Trim(),
            Quantity = qty,
            UnitPrice = unitPrice,
        };
    }
}