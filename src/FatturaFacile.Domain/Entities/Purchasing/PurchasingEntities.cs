using System.Globalization;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Purchasing;

public sealed class PurchaseOrder : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid SupplierId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public DateTime? ExpectedDeliveryDate { get; private set; }
    public Money TotalAmount { get; private set; } = Money.Euro(0);
    public string Status { get; } = "Placed";

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private PurchaseOrder()
    {
    }

    public static PurchaseOrder Create(Guid tenantId, string orderNumber, Guid supplierId, Money total, DateTime? expectedDelivery = null)
    {
        return new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OrderNumber = orderNumber,
            SupplierId = supplierId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = total,
            ExpectedDeliveryDate = expectedDelivery,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}

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

public sealed class Iso20022PaymentInstruction : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string InstructionId { get; private set; } = string.Empty;
    public string EndToEndId { get; private set; } = string.Empty;
    public DateTime ExecutionDate { get; private set; }
    public string DebtorIban { get; private set; } = string.Empty;
    public string CreditorIban { get; private set; } = string.Empty;
    public string CreditorName { get; private set; } = string.Empty;
    public Money Amount { get; private set; } = Money.Euro(0);
    public string RemittanceInformation { get; private set; } = string.Empty;
    /// <summary>
    /// Draft, GeneratedPain001, Executed
    /// </summary>
    public string Status { get; private set; } = "Draft";

    private Iso20022PaymentInstruction()
    {
    }

    public static Iso20022PaymentInstruction Create(
        Guid tenantId,
        string debtorIban,
        string creditorIban,
        string creditorName,
        Money amount,
        string remittance,
        DateTime executionDate)
    {
        var id = Guid.NewGuid();
        return new Iso20022PaymentInstruction
        {
            Id = id,
            TenantId = tenantId,
            InstructionId = string.Create(CultureInfo.InvariantCulture, $"PMT-{DateTime.UtcNow:yyyyMMdd}-{id.ToString()[..8]}"),
            EndToEndId = string.Create(CultureInfo.InvariantCulture, $"E2E-{DateTime.UtcNow:yyyyMMddHHmmss}"),
            DebtorIban = debtorIban.Replace(" ", string.Empty, StringComparison.Ordinal),
            CreditorIban = creditorIban.Replace(" ", string.Empty, StringComparison.Ordinal),
            CreditorName = creditorName,
            Amount = amount,
            RemittanceInformation = remittance,
            ExecutionDate = executionDate,
            Status = "Draft",
        };
    }

    public void MarkPain001Generated() => Status = "GeneratedPain001";
}
