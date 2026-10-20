using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Purchasing;

public sealed class PassiveInvoice : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public Guid SupplierId { get; private set; }
    public string DocumentNumber { get; private set; } = string.Empty;
    public DateTime DocumentDate { get; private set; }
    public decimal TotalNet { get; private set; }
    public decimal TotalVat { get; private set; }
    public decimal TotalGross { get; private set; }
    public Currency Currency { get; private set; }
    public DateTime PaymentDueDate { get; private set; }
    public string? SdiIdentifier { get; private set; }
    public string? SwissQrPayload { get; private set; }
    public bool IsReconciled { get; private set; }

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private PassiveInvoice()
    {
    }

    public static PassiveInvoice Create(
        Guid tenantId,
        Guid supplierId,
        string documentNumber,
        DateTime documentDate,
        decimal totalNet,
        decimal totalVat,
        Currency currency,
        DateTime paymentDueDate,
        string? sdiId = null,
        string? qrPayload = null)
    {
        return new PassiveInvoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierId = supplierId,
            DocumentNumber = documentNumber,
            DocumentDate = documentDate,
            TotalNet = totalNet,
            TotalVat = totalVat,
            TotalGross = totalNet + totalVat,
            Currency = currency,
            PaymentDueDate = paymentDueDate,
            SdiIdentifier = sdiId,
            SwissQrPayload = qrPayload,
            IsReconciled = false,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void MarkReconciled()
    {
        IsReconciled = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}