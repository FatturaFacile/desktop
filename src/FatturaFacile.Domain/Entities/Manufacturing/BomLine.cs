using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities.Manufacturing;

public sealed class BomLine : Entity<Guid>
{
    public Guid BillOfMaterialsId { get; }
    public Guid ComponentItemId { get; }
    public decimal QuantityPerUnit { get; }
    public decimal ScrapRatePercentage { get; }

    internal BomLine(Guid bomId, Guid componentItemId, decimal quantity, decimal scrapRate)
    {
        Id = Guid.NewGuid();
        BillOfMaterialsId = bomId;
        ComponentItemId = componentItemId;
        QuantityPerUnit = quantity;
        ScrapRatePercentage = scrapRate;
    }

    private BomLine()
    {
    }
}