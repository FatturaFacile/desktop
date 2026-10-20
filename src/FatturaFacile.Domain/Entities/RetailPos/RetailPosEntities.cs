using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.RetailPos;

public sealed class PosTerminalSession : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    /// <summary>
    /// POS-01
    /// </summary>
    public string TerminalCode { get; private set; } = string.Empty;
    public Guid CashierUserId { get; private set; }
    public DateTime OpenedAtUtc { get; private set; }
    public DateTime? ClosedAtUtc { get; private set; }
    public Money InitialCashFloat { get; private set; } = Money.Euro(100);
    public Money ClosingCashCalculated { get; private set; } = Money.Euro(0);
    public Money CashDiscrepancy { get; private set; } = Money.Euro(0);
    public bool IsClosed { get; private set; }

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private PosTerminalSession()
    {
    }

    public static PosTerminalSession Open(Guid tenantId, string terminalCode, Guid cashierId, Money cashFloat)
    {
        return new PosTerminalSession
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TerminalCode = terminalCode.Trim().ToUpperInvariant(),
            CashierUserId = cashierId,
            OpenedAtUtc = DateTime.UtcNow,
            InitialCashFloat = cashFloat,
            IsClosed = false,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void CloseSession(Money countedCash, Money calculatedCash)
    {
        ClosedAtUtc = DateTime.UtcNow;
        ClosingCashCalculated = calculatedCash;
        CashDiscrepancy = countedCash - calculatedCash;
        IsClosed = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

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
