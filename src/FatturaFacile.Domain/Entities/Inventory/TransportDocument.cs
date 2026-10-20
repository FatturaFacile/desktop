using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Inventory;

public sealed class TransportDocument : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }

    /// <summary>
    /// DDT-2026/001
    /// </summary>
    public string DocumentNumber { get; private set; } = string.Empty;
    public DateTime DocumentDate { get; private set; }
    public Guid ClientId { get; private set; }
    public Address DestinationAddress { get; private set; } = null!;

    /// <summary>
    /// Vendita, Conto Visione, Riparazione
    /// </summary>
    public string TransportationReason { get; private set; } = "Vendita";
    public string CarrierName { get; private set; } = "Mittente";
    public string GoodsAppearance { get; } = "Cartoni";
    public int NumberOfPackages { get; private set; } = 1;
    public decimal TotalWeightKg { get; private set; }

    /// <summary>
    /// Cross-border customs notes IT-CH
    /// </summary>
    public string? CustomsCmrNotes { get; private set; }

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private TransportDocument()
    {
    }

    public static TransportDocument Create(
        Guid tenantId,
        string number,
        DateTime date,
        Guid clientId,
        Address destination,
        string reason,
        string carrier,
        int packages,
        decimal weightKg,
        string? customsCmr = null)
    {
        return new TransportDocument
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DocumentNumber = number,
            DocumentDate = date,
            ClientId = clientId,
            DestinationAddress = destination,
            TransportationReason = reason,
            CarrierName = carrier,
            NumberOfPackages = packages,
            TotalWeightKg = weightKg,
            CustomsCmrNotes = customsCmr,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}