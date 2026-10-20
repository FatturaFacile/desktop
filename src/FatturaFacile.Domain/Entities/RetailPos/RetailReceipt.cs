using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.RetailPos;

public sealed class RetailReceipt : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public Guid PosTerminalSessionId { get; private set; }
    public string ReceiptNumber { get; private set; } = string.Empty;
    public DateTime TimestampUtc { get; private set; }
    public decimal TotalAmount { get; private set; }
    public Currency Currency { get; private set; } = Currency.EUR;
    public PaymentMethod PaymentMethod { get; private set; }

    /// <summary>
    /// Electronic Cash Register (RT - Registratore Telematico) / Fiscal lot mapping
    /// </summary>
    public string? ItalianRtTransmissionLot { get; private set; }
    public string? ItalianZReportNumber { get; private set; }
    public string? SwissTerminalTxReference { get; private set; }

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private RetailReceipt()
    {
    }

    public static RetailReceipt Create(
        Guid tenantId,
        Guid sessionId,
        string receiptNumber,
        decimal amount,
        Currency currency,
        PaymentMethod paymentMethod,
        string? rtLot = null,
        string? zReport = null,
        string? chTerminalTx = null)
    {
        return new RetailReceipt
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PosTerminalSessionId = sessionId,
            ReceiptNumber = receiptNumber.Trim(),
            TimestampUtc = DateTime.UtcNow,
            TotalAmount = amount,
            Currency = currency,
            PaymentMethod = paymentMethod,
            ItalianRtTransmissionLot = rtLot,
            ItalianZReportNumber = zReport,
            SwissTerminalTxReference = chTerminalTx,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}