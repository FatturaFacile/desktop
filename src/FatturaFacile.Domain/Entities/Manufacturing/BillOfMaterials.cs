using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities.Manufacturing;

public sealed class BillOfMaterials : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public Guid FinishedProductItemId { get; private set; }
    public string BomCode { get; private set; } = string.Empty;
    public string RevisionNumber { get; private set; } = "1.0";
    public decimal BaseYieldPercentage { get; } = 100m;
    public bool IsActive { get; } = true;

    private readonly List<BomLine> _lines = [];
    public IReadOnlyCollection<BomLine> Lines => _lines.AsReadOnly();

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private BillOfMaterials()
    {
    }

    public static BillOfMaterials Create(Guid tenantId, Guid finishedItemId, string code, string revision = "1.0")
    {
        return new BillOfMaterials
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FinishedProductItemId = finishedItemId,
            BomCode = code.Trim().ToUpperInvariant(),
            RevisionNumber = revision.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void AddComponent(Guid componentItemId, decimal quantity, decimal scrapRate = 0m) => _lines.Add(new BomLine(Id, componentItemId, quantity, scrapRate));
}