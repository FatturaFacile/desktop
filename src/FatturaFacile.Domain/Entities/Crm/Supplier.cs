using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Crm;

public sealed class Supplier : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; private set; } = string.Empty;
    public Jurisdiction Jurisdiction { get; private set; }
    public VatNumber? VatNumber { get; private set; }
    public string? SwissUid { get; private set; }
    public string? SdiCode { get; private set; }
    public Address Address { get; private set; } = null!;
    public string? Iban { get; private set; }
    public string? BicSwift { get; private set; }
    public int PaymentTermsDays { get; private set; } = 30;
    public string? Notes { get; }
    public bool IsActive { get; } = true;

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private Supplier()
    {
    }

    public static Supplier Create(
        Guid tenantId,
        string name,
        Jurisdiction jurisdiction,
        Address address,
        VatNumber? vatNumber = null,
        string? swissUid = null,
        string? sdiCode = null,
        string? iban = null,
        string? bicSwift = null,
        int paymentTermsDays = 30)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Supplier
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name.Trim(),
            Jurisdiction = jurisdiction,
            Address = address,
            VatNumber = vatNumber,
            SwissUid = swissUid,
            SdiCode = sdiCode,
            Iban = iban?.Replace(" ", string.Empty, StringComparison.Ordinal),
            BicSwift = bicSwift?.Trim().ToUpperInvariant(),
            PaymentTermsDays = paymentTermsDays,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}